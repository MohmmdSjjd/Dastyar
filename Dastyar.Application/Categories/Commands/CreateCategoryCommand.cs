using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Categories.Commands;

public sealed record CreateCategoryCommand(
    string Code,
    string Name,
    string? ParentCategoryCode,
    string? UnitDefault,
    string Scope = "Material",
    bool IsActive = true,
    int SortOrder = 0) : IRequest<Guid>;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly IRepository<Category, Guid> _repository;

    public CreateCategoryCommandHandler(IRepository<Category, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();
        var existing = await _repository.FindAsync(x => x.Code == code, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"دسته‌بندی با کد '{code}' قبلاً وجود دارد.");
        }

        Category? parentCategory = null;
        if (!string.IsNullOrWhiteSpace(request.ParentCategoryCode))
        {
            var parentCode = request.ParentCategoryCode.Trim();
            parentCategory = await _repository.FindAsync(x => x.Code == parentCode, cancellationToken);
            if (parentCategory is null)
            {
                throw new InvalidOperationException("دسته‌بندی والد پیدا نشد.");
            }
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name.Trim(),
            Scope = Enum.TryParse(request.Scope, true, out Dastyar.Domain.Enums.CategoryScope parsedScope)
                ? parsedScope
                : Dastyar.Domain.Enums.CategoryScope.Material,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            ParentCategoryId = parentCategory?.Id,
            UnitDefault = string.IsNullOrWhiteSpace(request.UnitDefault) ? null : request.UnitDefault.Trim()
        };

        await _repository.AddAsync(category, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
