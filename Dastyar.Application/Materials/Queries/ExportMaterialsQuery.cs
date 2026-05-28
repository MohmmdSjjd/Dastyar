using ClosedXML.Excel;
using Dastyar.Application.Common.Interfaces;
using Dastyar.Application.Materials.Dtos;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Materials.Queries;

public sealed record ExportMaterialsQuery(IReadOnlyList<string>? Fields) : IRequest<ExportMaterialsResultDto>;

public sealed class ExportMaterialsQueryHandler : IRequestHandler<ExportMaterialsQuery, ExportMaterialsResultDto>
{
    private static readonly string[] StandardHeaders =
    {
        "Code",
        "Name",
        "IsActive",
        "CategoryCode",
        "BasePrice",
        "LastPurchasePrice",
        "DailyPurchasePrice",
        "Unit",
    };

    private readonly IRepository<Material, Guid> _repository;

    public ExportMaterialsQueryHandler(IRepository<Material, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<ExportMaterialsResultDto> Handle(ExportMaterialsQuery request, CancellationToken cancellationToken)
    {
        var materials = await _repository.ListAsync(cancellationToken, x => x.Category);

        var headers = new List<string>();
        if (request.Fields is { Count: > 0 })
        {
            headers.AddRange(request.Fields);
        }
        else
        {
            headers.AddRange(StandardHeaders);

            var allKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var it in materials)
            {
                if (string.IsNullOrWhiteSpace(it.DynamicFieldsJson)) continue;
                try
                {
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(it.DynamicFieldsJson);
                    if (dict is null) continue;
                    foreach (var k in dict.Keys) allKeys.Add(k);
                }
                catch { }
            }

            foreach (var k in allKeys.OrderBy(x => x))
            {
                if (!headers.Contains(k, StringComparer.OrdinalIgnoreCase))
                {
                    headers.Add(k);
                }
            }
        }

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Materials");

        for (var i = 0; i < headers.Count; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
        }

        var rowIndex = 2;
        foreach (var it in materials)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(it.DynamicFieldsJson))
            {
                try
                {
                    dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(it.DynamicFieldsJson) ?? dict;
                }
                catch { }
            }

            var baseValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Code"] = it.Code ?? string.Empty,
                ["Name"] = it.Name ?? string.Empty,
                ["IsActive"] = it.IsActive.ToString(),
                ["CategoryCode"] = it.Category?.Code ?? string.Empty,
                ["BasePrice"] = it.BasePrice.ToString(),
                ["LastPurchasePrice"] = it.LastPurchasePrice.ToString(),
                ["DailyPurchasePrice"] = it.DailyPurchasePrice.ToString(),
                ["Unit"] = it.Unit ?? string.Empty,
            };

            for (var col = 0; col < headers.Count; col++)
            {
                var header = headers[col];
                if (baseValues.TryGetValue(header, out var baseValue))
                {
                    ws.Cell(rowIndex, col + 1).Value = baseValue;
                    continue;
                }

                dict.TryGetValue(header, out var v);
                ws.Cell(rowIndex, col + 1).Value = v ?? string.Empty;
            }

            rowIndex++;
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        var fileName = $"export_materials_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return new ExportMaterialsResultDto(
            ms.ToArray(),
            fileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
