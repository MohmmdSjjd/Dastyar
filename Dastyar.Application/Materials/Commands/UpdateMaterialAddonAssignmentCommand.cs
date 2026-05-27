using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Materials.Commands;

public sealed record UpdateMaterialAddonAssignmentCommand(
    Guid Id,
    string AddonMaterialCode,
    string? TargetCategoryCode,
    string? TargetMaterialCode,
    int Quantity,
    string? UnitOverride) : IRequest<Unit>;

public sealed class UpdateMaterialAddonAssignmentCommandHandler(
    IRepository<MaterialAddonAssignment, Guid> assignmentRepository,
    IRepository<Material, Guid> materialRepository,
    IRepository<Category, Guid> categoryRepository)
    : IRequestHandler<UpdateMaterialAddonAssignmentCommand, Unit>
{
    public async Task<Unit> Handle(UpdateMaterialAddonAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (assignment is null)
        {
            throw new InvalidOperationException("افزودنی پیدا نشد.");
        }

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
        var duplicate = await assignmentRepository.FindAsync(
            x => x.Id != assignment.Id &&
                 x.AddonMaterialId == addon.Id &&
                 x.TargetCategoryId == targetCategoryId &&
                 x.TargetMaterialId == targetMaterialId,
            cancellationToken);

        if (duplicate is not null)
        {
            throw new InvalidOperationException("این افزودنی قبلاً برای این هدف ثبت شده است.");
        }

        assignment.AddonMaterialId = addon.Id;
        assignment.TargetCategoryId = targetCategoryId;
        assignment.TargetMaterialId = targetMaterialId;
        assignment.Quantity = request.Quantity <= 0 ? 1 : request.Quantity;
        assignment.UnitOverride = string.IsNullOrWhiteSpace(request.UnitOverride) ? null : request.UnitOverride.Trim();

        await assignmentRepository.UpdateAsync(assignment, cancellationToken);

        return Unit.Value;
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
