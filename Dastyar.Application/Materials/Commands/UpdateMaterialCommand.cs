using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Materials.Commands;

public sealed record UpdateMaterialCommand(
    Guid Id,
    string Code,
    string? Name,
    bool IsActive,
    int BasePrice,
    int? LastPurchasePrice,
    int? DailyPurchasePrice,
    string? Unit,
    string? CategoryCode = null,
    string? DynamicFieldsJson = null) : IRequest<Unit>;

public sealed class UpdateMaterialCommandHandler(
    IRepository<Material, Guid> repository,
    IRepository<Category, Guid> categoryRepository)
    : IRequestHandler<UpdateMaterialCommand, Unit>
{
    public async Task<Unit> Handle(UpdateMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (material is null)
        {
            throw new InvalidOperationException("ماده اولیه پیدا نشد.");
        }

        var currentCode = material.Code?.Trim() ?? string.Empty;
        var newCode = request.Code.Trim();

        if (!string.Equals(currentCode, newCode, StringComparison.OrdinalIgnoreCase))
        {
            var existingMaterial = await repository.FindAsync(
                x => x.Code != null && x.Code.ToLower() == newCode.ToLower(),
                cancellationToken);

            if (existingMaterial != null && existingMaterial.Id != material.Id)
            {
                throw new InvalidOperationException($"ماده اولیه با کد '{newCode}' قبلاً وجود دارد.");
            }
        }

        material.Code = newCode;
        material.Name = request.Name?.Trim();
        material.IsActive = request.IsActive;
        material.BasePrice = request.BasePrice;
        material.LastPurchasePrice = request.LastPurchasePrice ?? request.BasePrice;
        material.DailyPurchasePrice = request.DailyPurchasePrice ?? request.BasePrice;
        material.Unit = string.IsNullOrWhiteSpace(request.Unit) ? null : request.Unit.Trim();

        if (request.DynamicFieldsJson != null)
        {
            material.DynamicFieldsJson = request.DynamicFieldsJson;
        }

        if (request.CategoryCode is null)
        {
            material.CategoryId = null;
        }
        else
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

            material.CategoryId = category?.Id;
        }

        await repository.UpdateAsync(material, cancellationToken);

        return Unit.Value;
    }
}
