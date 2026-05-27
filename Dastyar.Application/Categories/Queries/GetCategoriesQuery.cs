using Dastyar.Application.Categories.Dtos;
using Dastyar.Domain.Entities;
using MediatR;
using Dastyar.Application.Common.Interfaces;

namespace Dastyar.Application.Categories.Queries;

public sealed record GetCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryDto>>
{
    private readonly IRepository<Category, Guid> _repository;

    public GetCategoriesQueryHandler(IRepository<Category, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _repository.ListAsync(cancellationToken, x => x.ParentCategory!);
        return categories.Select(x => new CategoryDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            ParentCategoryId = x.ParentCategoryId,
            ParentCategoryCode = x.ParentCategory?.Code,
            UnitDefault = x.UnitDefault
        });
    }
}
