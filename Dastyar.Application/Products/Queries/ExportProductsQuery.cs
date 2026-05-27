using Dastyar.Application.Common.Interfaces;
using Dastyar.Application.Products.Dtos;
using Dastyar.Domain.Entities;
using MediatR;
using ClosedXML.Excel;

namespace Dastyar.Application.Products.Queries;

public sealed record ExportProductsQuery(IReadOnlyList<string>? Fields) : IRequest<ExportProductsResultDto>;

public sealed class ExportProductsQueryHandler : IRequestHandler<ExportProductsQuery, ExportProductsResultDto>
{
    private readonly IRepository<Product, Guid> _repository;

    public ExportProductsQueryHandler(IRepository<Product, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<ExportProductsResultDto> Handle(ExportProductsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.ListAsync<ProductDto>(cancellationToken);

        var headers = new List<string>();
        if (request.Fields is { Count: > 0 })
        {
            headers.AddRange(request.Fields);
        }
        else
        {
            var allKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var it in items)
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

            headers.AddRange(allKeys.OrderBy(x => x));
        }

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Products");

        for (var i = 0; i < headers.Count; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
        }

        var rowIndex = 2;
        foreach (var it in items)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(it.DynamicFieldsJson))
            {
                try { dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(it.DynamicFieldsJson) ?? dict; } catch { }
            }

            for (var col = 0; col < headers.Count; col++)
            {
                dict.TryGetValue(headers[col], out var v);
                ws.Cell(rowIndex, col + 1).Value = v ?? string.Empty;
            }

            rowIndex++;
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        var fileName = $"export_products_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return new ExportProductsResultDto(ms.ToArray(), fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
