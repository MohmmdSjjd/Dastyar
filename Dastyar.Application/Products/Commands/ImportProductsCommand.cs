using System;
using Dastyar.Application.Common.Interfaces;
using Dastyar.Application.Products.Queries;
using Dastyar.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using System.Text.Json;

namespace Dastyar.Application.Products.Commands;

public sealed record ImportProductsCommand(IFormFile File) : IRequest<ImportProductsResultDto>;

public sealed class ImportProductsCommandHandler : IRequestHandler<ImportProductsCommand, ImportProductsResultDto>
{
    private readonly IRepository<Product, Guid> _repository;
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<FieldDefinition, Guid> _fieldRepository;

    public ImportProductsCommandHandler(
        IRepository<Product, Guid> repository,
        IRepository<Category, Guid> categoryRepository,
        IRepository<FieldDefinition, Guid> fieldRepository)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _fieldRepository = fieldRepository;
    }

    public async Task<ImportProductsResultDto> Handle(ImportProductsCommand request, CancellationToken cancellationToken)
    {

        await using var stream = new MemoryStream();
        await request.File.CopyToAsync(stream, cancellationToken);
        // مرحله: خواندن محتوای فایل اکسل
        IWorkbook workbook;

        stream.Position = 0;
        if (!TryCreateWorkbook(stream, out workbook))
        {
            throw new InvalidOperationException("Excel file format is not supported. Use .xls or .xlsx.");
        }

        var worksheet = workbook.GetSheetAt(0);
        var headerRow = worksheet.GetRow(worksheet.FirstRowNum);

        if (headerRow is null)
        {
            throw new InvalidOperationException("Excel file must contain a header row.");
        }

        var columns = headerRow.Cells
            .Where(c => c.CellType != CellType.Blank)
            .ToDictionary(c => GetCellString(c).Trim(), c => c.ColumnIndex, StringComparer.OrdinalIgnoreCase);

        // ensure field definitions exist for headers
        var existingFields = await _fieldRepository.ListAsync(cancellationToken);
        var existingNames = new HashSet<string>(existingFields.Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
        var newFieldDefs = new List<FieldDefinition>();
        foreach (var header in columns.Keys)
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                continue;
            }

            if (existingNames.Contains(header))
            {
                continue;
            }

            newFieldDefs.Add(new FieldDefinition
            {
                Id = Guid.NewGuid(),
                Name = header.Trim(),
                DisplayName = header.Trim(),
                DataType = "string",
                IsFilterable = true,
                IsSortable = false,
                IsActive = true
            });
        }

        if (newFieldDefs.Count > 0)
        {
            await _fieldRepository.AddRangeAsync(newFieldDefs, cancellationToken);
            await _fieldRepository.SaveChangesAsync(cancellationToken);
        }

        // columns: map header -> columnIndex (case-insensitive)
        var nameColumn = columns.TryGetValue("Name", out var nameCol) ? nameCol : -1;
        var isActiveColumn = columns.TryGetValue("IsActive", out var activeCol) ? activeCol : -1;
        var categoryCodeColumn = columns.TryGetValue("CategoryCode", out var categoryCodeCol) ? categoryCodeCol : -1;

        // مرحله: آماده سازی شمارنده ها و لیست درج تجمیعی
        var added = 0;
        var updated = 0;
        var skipped = 0;
        var newEntities = new List<Product>();
        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // مرحله: پردازش سطرهای اکسل
        for (var rowIndex = worksheet.FirstRowNum + 1; rowIndex <= worksheet.LastRowNum; rowIndex++)
        {
            var row = worksheet.GetRow(rowIndex);
            if (row is null)
            {
                continue;
            }

            // Build a dictionary of all header -> value for this row
            var rowData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in columns)
            {
                var header = kv.Key;
                var colIndex = kv.Value;
                var cellValue = GetCellString(row.GetCell(colIndex)).Trim();
                rowData[header] = cellValue;
            }

            // If a Code column exists, try to deduplicate/update by Code
            if (rowData.TryGetValue("Code", out var code) && !string.IsNullOrWhiteSpace(code))
            {
                if (!seenCodes.Add(code))
                {
                    throw new InvalidOperationException($"کد محصول '{code}' در فایل اکسل تکراری است.");
                }

                var existing = await _repository.FindAsync(x => x.Code != null && x.Code.ToLower() == code.ToLower(), cancellationToken);
                if (existing is not null)
                {
                    // update dynamic json and also common fields if present
                    if (rowData.TryGetValue("Name", out var n)) existing.Name = n;
                    if (rowData.TryGetValue("IsActive", out var a))
                    {
                        if (bool.TryParse(a, out var pb)) existing.IsActive = pb;
                        else if (int.TryParse(a, out var pi)) existing.IsActive = pi != 0;
                        else existing.IsActive = a.Equals("yes", StringComparison.OrdinalIgnoreCase) || a.Equals("true", StringComparison.OrdinalIgnoreCase);
                    }
                    if (rowData.TryGetValue("CategoryCode", out var cc) && !string.IsNullOrWhiteSpace(cc))
                    {
                        var category = await _categoryRepository.FindAsync(x => x.Code == cc, cancellationToken);
                        existing.CategoryId = category?.Id;
                    }

                    existing.DynamicFieldsJson = JsonSerializer.Serialize(rowData);
                    _repository.Update(existing);
                    updated++;
                    continue;
                }
            }

            // ایجاد محصول جدید: نگه داشتن هرچه در rowData است
            // Skip rows with empty Code (they can't be unique)
            if (!rowData.TryGetValue("Code", out var newCode) || string.IsNullOrWhiteSpace(newCode))
            {
                skipped++;
                continue;
            }

            newCode = newCode.Trim();

            var entity = new Product
            {
                Id = Guid.NewGuid(),
                Code = newCode,
                Name = rowData.TryGetValue("Name", out var nn) ? nn : null,
                IsActive = rowData.TryGetValue("IsActive", out var aa) && (bool.TryParse(aa, out var pb2) ? pb2 : (int.TryParse(aa, out var pi2) ? pi2 != 0 : aa.Equals("yes", StringComparison.OrdinalIgnoreCase) || aa.Equals("true", StringComparison.OrdinalIgnoreCase))),
                CreatedAtUtc = DateTime.UtcNow,
                CategoryId = rowData.TryGetValue("CategoryCode", out var ccode) && !string.IsNullOrWhiteSpace(ccode) ? (await _categoryRepository.FindAsync(x => x.Code == ccode, cancellationToken))?.Id : null,
                DynamicFieldsJson = JsonSerializer.Serialize(rowData)
            };

            newEntities.Add(entity);
            added++;
        }

        // مرحله: درج تجمیعی محصولات جدید
        if (newEntities.Count > 0)
        {
            // Prevent unique index conflicts: if any of the new entities' Codes
            // already exist in the DB (possibly inserted earlier), update those
            // instead of attempting to insert duplicates.
            var toInsert = new List<Product>();
            foreach (var ne in newEntities)
            {
                if (string.IsNullOrWhiteSpace(ne.Code))
                {
                    // should not happen due to earlier guards, but skip defensively
                    skipped++;
                    continue;
                }

                var existingByCode = await _repository.FindAsync(x => x.Code != null && x.Code.ToLower() == ne.Code.ToLower(), cancellationToken);
                if (existingByCode is not null)
                {
                    // Update existing record
                    existingByCode.Name = ne.Name ?? existingByCode.Name;
                    existingByCode.IsActive = ne.IsActive;
                    existingByCode.CategoryId = ne.CategoryId ?? existingByCode.CategoryId;
                    existingByCode.DynamicFieldsJson = ne.DynamicFieldsJson;
                    _repository.Update(existingByCode);
                    updated++;
                }
                else
                {
                    toInsert.Add(ne);
                }
            }

            if (toInsert.Count > 0)
            {
                await _repository.AddRangeAsync(toInsert, cancellationToken);
            }
        }

        // مرحله: ذخیره سازی تغییرات
        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Map DB unique constraint failures to a clearer error without
            // forcing an EF Core dependency in this project. Look for the
            // unique index name in inner exception messages.
            var message = ex.InnerException?.Message ?? ex.Message;
            if (!string.IsNullOrEmpty(message) && message.Contains("IX_Products_Code", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("خطا: یک یا چند ردیف دارای کد تکراری هستند یا مقدار کد نامعتبر است. لطفاً فایل را بررسی کنید.");
            }

            throw;
        }

        return new ImportProductsResultDto(added, updated, skipped);
    }

    private static bool TryCreateWorkbook(Stream stream, out IWorkbook workbook)
    {
        try
        {
            workbook = WorkbookFactory.Create(stream);
            return true;
        }
        catch
        {
            workbook = null!;
            return false;
        }
    }

    private static string GetCellString(ICell? cell)
    {
        if (cell is null)
        {
            return string.Empty;
        }

        return cell.CellType switch
        {
            CellType.String => cell.StringCellValue,
            CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                ? cell.DateCellValue?.ToString("o") ?? string.Empty
                : cell.NumericCellValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
            CellType.Boolean => cell.BooleanCellValue.ToString(),
            CellType.Formula => GetFormulaValue(cell),
            _ => string.Empty,
        };
    }

    private static string GetFormulaValue(ICell cell)
    {
        return cell.CachedFormulaResultType switch
        {
            CellType.String => cell.StringCellValue,
            CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                ? cell.DateCellValue?.ToString("o") ?? string.Empty
                : cell.NumericCellValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
            CellType.Boolean => cell.BooleanCellValue.ToString(),
            _ => string.Empty,
        };
    }
}
