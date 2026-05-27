using Dastyar.Application.Common;
using Dastyar.Application.Common.Interfaces;
using Dastyar.Application.Products.Dtos;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Products.Queries;

public sealed record GetProductsQuery : BaseSearchQuery, IRequest<IReadOnlyList<ProductDto>>;

public sealed class GetProductsQueryHandler(IRepository<Product, Guid> repository) : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // مرحله: دریافت داده ها با فیلتر و مپ به DTO
        var items = request.SearchFilters is { Count: > 0 }
            ? await repository.ListAsync<ProductDto>(request.SearchFilters, cancellationToken)
            : await repository.ListAsync<ProductDto>(cancellationToken);

        // مرحله: مرتب سازی خروجی
        return items.OrderByDescending(x => x.CreatedAtUtc).ToList();
    }
}
