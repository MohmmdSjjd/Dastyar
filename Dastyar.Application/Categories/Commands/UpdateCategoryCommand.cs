using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Categories.Commands;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Code,
    string Name,
    string? ParentCategoryCode,
    string? UnitDefault) : IRequest<Unit>;

public sealed class UpdateCategoryCommandHandler(IRepository<Category, Guid> repository)
    : IRequestHandler<UpdateCategoryCommand, Unit>
{
    public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
        {
            throw new InvalidOperationException("دسته‌بندی پیدا نشد.");
        }

        var code = request.Code.Trim();
        var duplicate = await repository.FindAsync(x => x.Code == code, cancellationToken);
        if (duplicate is not null && duplicate.Id != category.Id)
        {
            throw new InvalidOperationException($"دسته‌بندی با کد '{code}' قبلاً وجود دارد.");
        }

        Category? parentCategory = null;
        if (!string.IsNullOrWhiteSpace(request.ParentCategoryCode))
        {
            var parentCode = request.ParentCategoryCode.Trim();
            parentCategory = await repository.FindAsync(x => x.Code == parentCode, cancellationToken);
            if (parentCategory is null)
            {
                throw new InvalidOperationException("دسته‌بندی والد پیدا نشد.");
            }

            if (parentCategory.Id == category.Id)
            {
                throw new InvalidOperationException("دسته‌بندی نمی‌تواند والد خودش باشد.");
            }
        }

        category.Code = code;
        category.Name = request.Name.Trim();
        category.ParentCategoryId = parentCategory?.Id;
        category.UnitDefault = string.IsNullOrWhiteSpace(request.UnitDefault)
            ? null
            : request.UnitDefault.Trim();

        await repository.UpdateAsync(category, cancellationToken);
        return Unit.Value;
    }
}
