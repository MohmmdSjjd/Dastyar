using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Materials.Commands;

public sealed record CreateMaterialAddonAssignmentCommand(
    string AddonMaterialCode,
    string? TargetCategoryCode,
    string? TargetMaterialCode,
    int Quantity,
    string? UnitOverride) : IRequest<Guid>;

public sealed class CreateMaterialAddonAssignmentCommandHandler(
    IRepository<MaterialAddonAssignment, Guid> assignmentRepository,
    IRepository<Material, Guid> materialRepository,
    IRepository<Category, Guid> categoryRepository)
    : IRequestHandler<CreateMaterialAddonAssignmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateMaterialAddonAssignmentCommand request, CancellationToken cancellationToken)
    {
        var addon = await FindMaterialAsync(request.AddonMaterialCode, cancellationToken);
        var targetCategory = await FindCategoryAsync(request.TargetCategoryCode, cancellationToken);
        var targetMaterial = await FindTargetMaterialAsync(request.TargetMaterialCode, cancellationToken);

        ValidateTarget(targetCategory, targetMaterial);

        if (targetMaterial?.Id == addon.Id)
        {
            throw new InvalidOperationException("ماده افزودنی نمی‌تواند همان ماده هدف باشد.");
        }

        var targetCategoryId = targetCategory?.Id;
        var targetMaterialId = targetMaterial?.Id;
        var existing = await assignmentRepository.FindAsync(
            x => x.AddonMaterialId == addon.Id &&
                 x.TargetCategoryId == targetCategoryId &&
                 x.TargetMaterialId == targetMaterialId,
            cancellationToken);

        if (existing is not null)
        {
            throw new InvalidOperationException("این افزودنی قبلاً برای این هدف ثبت شده است.");
        }

        var entity = new MaterialAddonAssignment
        {
            Id = Guid.NewGuid(),
            AddonMaterialId = addon.Id,
            TargetCategoryId = targetCategoryId,
            TargetMaterialId = targetMaterialId,
            Quantity = request.Quantity <= 0 ? 1 : request.Quantity,
            UnitOverride = string.IsNullOrWhiteSpace(request.UnitOverride) ? null : request.UnitOverride.Trim()
        };

        await assignmentRepository.AddAsync(entity, cancellationToken);
        await assignmentRepository.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    private async Task<Material> FindMaterialAsync(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException("کد ماده افزودنی الزامی است.");
        }

        var material = await materialRepository.FindAsync(x => x.Code == code.Trim(), cancellationToken);
        return material ?? throw new InvalidOperationException("ماده افزودنی پیدا نشد.");
    }

    private async Task<Category?> FindCategoryAsync(string? code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var category = await categoryRepository.FindAsync(x => x.Code == code.Trim(), cancellationToken);
        return category ?? throw new InvalidOperationException("دسته‌بندی هدف پیدا نشد.");
    }

    private async Task<Material?> FindTargetMaterialAsync(string? code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var material = await materialRepository.FindAsync(x => x.Code == code.Trim(), cancellationToken);
        return material ?? throw new InvalidOperationException("ماده هدف پیدا نشد.");
    }

    private static void ValidateTarget(Category? targetCategory, Material? targetMaterial)
    {
        if (targetCategory is null && targetMaterial is null)
        {
            throw new InvalidOperationException("یک دسته‌بندی هدف یا ماده هدف انتخاب کنید.");
        }

        if (targetCategory is not null && targetMaterial is not null)
        {
            throw new InvalidOperationException("فقط یکی از دسته‌بندی هدف یا ماده هدف باید انتخاب شود.");
        }
    }
}
