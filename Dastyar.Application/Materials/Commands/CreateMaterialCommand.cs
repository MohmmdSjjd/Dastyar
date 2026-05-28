using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Materials.Commands;

public sealed record CreateMaterialCommand(
    string? Name,
    string? Code = null,
    string? CategoryCode = null,
    int BasePrice = 0,
    int? LastPurchasePrice = null,
    int? DailyPurchasePrice = null,
    string? Unit = null,
    string? DynamicFieldsJson = null) : IRequest<Guid>;

public sealed class CreateMaterialCommandHandler(
    IRepository<Material, Guid> repository,
    IRepository<Category, Guid> categoryRepository)
    : IRequestHandler<CreateMaterialCommand, Guid>
{
    public async Task<Guid> Handle(CreateMaterialCommand request, CancellationToken cancellationToken)
    {
        Category? category = null;
        if (!string.IsNullOrWhiteSpace(request.CategoryCode))
        {
            var categoryCode = request.CategoryCode.Trim();
            category = await categoryRepository.FindAsync(x => x.Code == categoryCode, cancellationToken);
            if (category is null)
            {
                throw new InvalidOperationException("دسته‌بندی پیدا نشد.");
            }
        }

        var codeToUse = string.IsNullOrWhiteSpace(request.Code)
            ? Guid.NewGuid().ToString("N")
            : request.Code.Trim();

        var existingMaterial = await repository.FindAsync(
            x => x.Code != null && x.Code.ToLower() == codeToUse.ToLower(),
            cancellationToken);

        if (existingMaterial != null)
        {
            throw new InvalidOperationException($"ماده اولیه با کد '{codeToUse}' قبلاً وجود دارد.");
        }

        var entity = new Material
        {
            Id = Guid.NewGuid(),
            Code = codeToUse,
            Name = request.Name?.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            CategoryId = category?.Id,
            BasePrice = request.BasePrice,
            LastPurchasePrice = request.LastPurchasePrice ?? request.BasePrice,
            DailyPurchasePrice = request.DailyPurchasePrice ?? request.BasePrice,
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? null : request.Unit.Trim(),
            DynamicFieldsJson = request.DynamicFieldsJson ?? string.Empty
        };

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
