using System.Text.Json;
using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using System.Globalization;

namespace Dastyar.Application.Materials.Commands;

public sealed record ImportMaterialsCommand(IFormFile File) : IRequest<ImportMaterialsResultDto>;

public sealed class ImportMaterialsCommandHandler : IRequestHandler<ImportMaterialsCommand, ImportMaterialsResultDto>
{
    private readonly IRepository<Material, Guid> _repository;
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<FieldDefinition, Guid> _fieldRepository;

    public ImportMaterialsCommandHandler(
        IRepository<Material, Guid> repository,
        IRepository<Category, Guid> categoryRepository,
        IRepository<FieldDefinition, Guid> fieldRepository)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _fieldRepository = fieldRepository;
    }

    public async Task<ImportMaterialsResultDto> Handle(ImportMaterialsCommand request, CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await request.File.CopyToAsync(stream, cancellationToken);
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

        var added = 0;
        var updated = 0;
        var skipped = 0;
        var newEntities = new List<Material>();
        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var rowIndex = worksheet.FirstRowNum + 1; rowIndex <= worksheet.LastRowNum; rowIndex++)
        {
            var row = worksheet.GetRow(rowIndex);
            if (row is null)
            {
                continue;
            }

            var rowData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in columns)
            {
                var header = kv.Key;
                var colIndex = kv.Value;
                var cellValue = GetCellString(row.GetCell(colIndex)).Trim();
                rowData[header] = cellValue;
            }

            if (rowData.TryGetValue("Code", out var code) && !string.IsNullOrWhiteSpace(code))
            {
                if (!seenCodes.Add(code))
                {
                    throw new InvalidOperationException($"کد ماده اولیه '{code}' در فایل اکسل تکراری است.");
                }

                var existing = await _repository.FindAsync(
                    x => x.Code != null && x.Code.ToLower() == code.ToLower(),
                    cancellationToken);

                if (existing is not null)
                {
                    if (rowData.TryGetValue("Name", out var name)) existing.Name = name;
                    if (rowData.TryGetValue("IsActive", out var active)) existing.IsActive = ParseBool(active);
                    if (rowData.TryGetValue("BasePrice", out var basePrice)) existing.BasePrice = ParseInt(basePrice);
                    if (rowData.TryGetValue("Unit", out var unit)) existing.Unit = string.IsNullOrWhiteSpace(unit) ? null : unit;

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

            if (!rowData.TryGetValue("Code", out var newCode) || string.IsNullOrWhiteSpace(newCode))
            {
                skipped++;
                continue;
            }

            newCode = newCode.Trim();

            var entity = new Material
            {
                Id = Guid.NewGuid(),
                Code = newCode,
                Name = rowData.TryGetValue("Name", out var nn) ? nn : null,
                IsActive = rowData.TryGetValue("IsActive", out var aa) && ParseBool(aa),
                CreatedAtUtc = DateTime.UtcNow,
                BasePrice = rowData.TryGetValue("BasePrice", out var bp) ? ParseInt(bp) : 0,
                Unit = rowData.TryGetValue("Unit", out var unit2) && !string.IsNullOrWhiteSpace(unit2) ? unit2 : null,
                CategoryId = rowData.TryGetValue("CategoryCode", out var ccode) && !string.IsNullOrWhiteSpace(ccode)
                    ? (await _categoryRepository.FindAsync(x => x.Code == ccode, cancellationToken))?.Id
                    : null,
                DynamicFieldsJson = JsonSerializer.Serialize(rowData)
            };

            newEntities.Add(entity);
            added++;
        }

        if (newEntities.Count > 0)
        {
            var toInsert = new List<Material>();
            foreach (var ne in newEntities)
            {
                if (string.IsNullOrWhiteSpace(ne.Code))
                {
                    skipped++;
                    continue;
                }

                var existingByCode = await _repository.FindAsync(
                    x => x.Code != null && x.Code.ToLower() == ne.Code.ToLower(),
                    cancellationToken);

                if (existingByCode is not null)
                {
                    existingByCode.Name = ne.Name ?? existingByCode.Name;
                    existingByCode.IsActive = ne.IsActive;
                    existingByCode.CategoryId = ne.CategoryId ?? existingByCode.CategoryId;
                    existingByCode.BasePrice = ne.BasePrice;
                    existingByCode.Unit = ne.Unit;
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

        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            if (!string.IsNullOrEmpty(message) && message.Contains("IX_Materials_Code", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("خطا: یک یا چند ردیف دارای کد تکراری هستند یا مقدار کد نامعتبر است. لطفاً فایل را بررسی کنید.");
            }

            throw;
        }

        return new ImportMaterialsResultDto(added, updated, skipped);
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
                : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
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
                : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
            CellType.Boolean => cell.BooleanCellValue.ToString(),
            _ => string.Empty,
        };
    }

    private static bool ParseBool(string value)
    {
        if (bool.TryParse(value, out var pb)) return pb;
        if (int.TryParse(value, out var pi)) return pi != 0;
        return value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("1", StringComparison.OrdinalIgnoreCase);
    }

    private static int ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) return i;
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out var j)) return j;
        return 0;
    }
}
