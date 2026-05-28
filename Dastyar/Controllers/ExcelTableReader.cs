using System.Globalization;
using NPOI.SS.UserModel;

namespace Dastyar.Controllers;

internal static class ExcelTableReader
{
    public static async Task<(IReadOnlyList<Dictionary<string, string>> Rows, IReadOnlyList<string> Headers)> ReadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;

        IWorkbook workbook;
        try
        {
            workbook = WorkbookFactory.Create(stream);
        }
        catch
        {
            throw new InvalidOperationException("Excel file format is not supported. Use .xls or .xlsx.");
        }

        var worksheet = workbook.GetSheetAt(0);
        var headerRow = worksheet.GetRow(worksheet.FirstRowNum);
        if (headerRow is null)
        {
            throw new InvalidOperationException("Excel file must contain a header row.");
        }

        var headers = headerRow.Cells
            .Where(c => c.CellType != CellType.Blank)
            .Select(c => GetCellString(c).Trim())
            .ToList();

        var columns = headerRow.Cells
            .Where(c => c.CellType != CellType.Blank)
            .ToDictionary(c => GetCellString(c).Trim(), c => c.ColumnIndex, StringComparer.OrdinalIgnoreCase);

        var rows = new List<Dictionary<string, string>>();
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
                rowData[kv.Key] = GetCellString(row.GetCell(kv.Value)).Trim();
            }

            if (rowData.Values.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            rows.Add(rowData);
        }

        return (rows, headers);
    }

    public static string GetCellString(ICell? cell)
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
}
