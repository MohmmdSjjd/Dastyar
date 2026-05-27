using Dastyar.Application.Common.Interfaces;
using Dastyar.Application.Materials.Dtos;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Materials.Queries;

public sealed record GetMaterialAddonAssignmentsQuery : IRequest<IReadOnlyList<MaterialAddonAssignmentDto>>;

public sealed class GetMaterialAddonAssignmentsQueryHandler(
    IRepository<MaterialAddonAssignment, Guid> repository)
    : IRequestHandler<GetMaterialAddonAssignmentsQuery, IReadOnlyList<MaterialAddonAssignmentDto>>
{
    public async Task<IReadOnlyList<MaterialAddonAssignmentDto>> Handle(
        GetMaterialAddonAssignmentsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await repository.ListAsync(
            cancellationToken,
            x => x.AddonMaterial!,
            x => x.TargetCategory!,
            x => x.TargetMaterial!);

        return items
            .OrderBy(x => x.TargetCategory?.Name ?? x.TargetMaterial?.Name ?? string.Empty)
            .ThenBy(x => x.AddonMaterial?.Name ?? string.Empty)
            .Select(x => new MaterialAddonAssignmentDto
            {
                Id = x.Id,
                AddonMaterialId = x.AddonMaterialId,
                AddonMaterialCode = x.AddonMaterial?.Code,
                AddonMaterialName = x.AddonMaterial?.Name,
                TargetCategoryId = x.TargetCategoryId,
                TargetCategoryCode = x.TargetCategory?.Code,
                TargetCategoryName = x.TargetCategory?.Name,
                TargetMaterialId = x.TargetMaterialId,
                TargetMaterialCode = x.TargetMaterial?.Code,
                TargetMaterialName = x.TargetMaterial?.Name,
                Quantity = x.Quantity,
                UnitOverride = x.UnitOverride
            })
            .ToList();
    }
}
