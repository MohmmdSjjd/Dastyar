using Dastyar.Application.Common;
using Dastyar.Application.Common.Interfaces;
using Dastyar.Application.Materials.Dtos;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Materials.Queries;

public sealed record GetMaterialsQuery : BaseSearchQuery, IRequest<IReadOnlyList<MaterialDto>>;

public sealed class GetMaterialsQueryHandler : IRequestHandler<GetMaterialsQuery, IReadOnlyList<MaterialDto>>
{
    private readonly IRepository<Material, Guid> _materialRepository;
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<MaterialAddonAssignment, Guid> _assignmentRepository;
    private readonly IRepository<ProductMaterial, Guid> _productMaterialRepository;

    public GetMaterialsQueryHandler(
        IRepository<Material, Guid> materialRepository,
        IRepository<Category, Guid> categoryRepository,
        IRepository<MaterialAddonAssignment, Guid> assignmentRepository,
        IRepository<ProductMaterial, Guid> productMaterialRepository)
    {
        _materialRepository = materialRepository;
        _categoryRepository = categoryRepository;
        _assignmentRepository = assignmentRepository;
        _productMaterialRepository = productMaterialRepository;
    }

    public async Task<IReadOnlyList<MaterialDto>> Handle(GetMaterialsQuery request, CancellationToken cancellationToken)
    {
        var materials = request.SearchFilters is { Count: > 0 }
            ? await _materialRepository.ListAsync(request.SearchFilters, cancellationToken)
            : await _materialRepository.ListAsync(cancellationToken);

        var allMaterials = request.SearchFilters is { Count: > 0 }
            ? await _materialRepository.ListAsync(cancellationToken)
            : materials;

        var categories = await _categoryRepository.ListAsync(cancellationToken);
        var assignments = await _assignmentRepository.ListAsync(cancellationToken);

        var usageCounts = await _productMaterialRepository.ListAsync(cancellationToken);
        var usageByMaterialId = usageCounts
            .GroupBy(x => x.MaterialId)
            .ToDictionary(group => group.Key, group => group.Select(x => x.ProductId).Distinct().Count());

        var categoryById = categories.ToDictionary(x => x.Id, x => x);
        var materialsById = allMaterials.ToDictionary(x => x.Id, x => x);

        var results = new List<MaterialDto>(materials.Count);

        foreach (var material in materials)
        {
            categoryById.TryGetValue(material.CategoryId ?? Guid.Empty, out var category);
            var categoryChain = GetCategoryChain(material.CategoryId, categoryById);

            var categoryAssignments = new Dictionary<Guid, MaterialAddonAssignment>();
            foreach (var categoryId in categoryChain)
            {
                foreach (var assignment in assignments.Where(x => x.TargetCategoryId == categoryId))
                {
                    if (!categoryAssignments.ContainsKey(assignment.AddonMaterialId))
                    {
                        categoryAssignments[assignment.AddonMaterialId] = assignment;
                    }
                }
            }

            var materialAssignments = assignments
                .Where(x => x.TargetMaterialId == material.Id)
                .GroupBy(x => x.AddonMaterialId)
                .ToDictionary(g => g.Key, g => g.Last());

            foreach (var kv in materialAssignments)
            {
                categoryAssignments[kv.Key] = kv.Value;
            }

            var appliedAddons = new List<MaterialAppliedAddonDto>();
            var addonTotal = 0;
            foreach (var assignment in categoryAssignments.Values)
            {
                if (assignment.AddonMaterialId == material.Id)
                {
                    continue;
                }

                if (!materialsById.TryGetValue(assignment.AddonMaterialId, out var addonMaterial))
                {
                    continue;
                }

                var quantity = assignment.Quantity < 0 ? 0 : assignment.Quantity;
                var totalPrice = addonMaterial.LastPurchasePrice * quantity;
                addonTotal += totalPrice;

                appliedAddons.Add(new MaterialAppliedAddonDto
                {
                    AssignmentId = assignment.Id,
                    AddonMaterialId = addonMaterial.Id,
                    Code = addonMaterial.Code,
                    Name = addonMaterial.Name,
                    Quantity = quantity,
                    Unit = !string.IsNullOrWhiteSpace(assignment.UnitOverride)
                        ? assignment.UnitOverride
                        : addonMaterial.Unit,
                    UnitPrice = addonMaterial.LastPurchasePrice,
                    TotalPrice = totalPrice
                });
            }

            var basePrice = material.LastPurchasePrice;
            var finalPrice = basePrice + addonTotal;

            var unitEffective = !string.IsNullOrWhiteSpace(material.Unit)
                ? material.Unit
                : category?.UnitDefault;

            results.Add(new MaterialDto
            {
                Id = material.Id,
                Code = material.Code,
                Name = material.Name,
                IsActive = material.IsActive,
                CreatedAtUtc = material.CreatedAtUtc,
                CategoryCode = category?.Code,
                CategoryName = category?.Name,
                BasePrice = basePrice,
                LastPurchasePrice = material.LastPurchasePrice,
                DailyPurchasePrice = material.DailyPurchasePrice,
                AddonTotalPrice = addonTotal,
                FinalPrice = finalPrice,
                Unit = material.Unit,
                UnitEffective = unitEffective,
                DynamicFieldsJson = material.DynamicFieldsJson,
                UsageProductCount = usageByMaterialId.TryGetValue(material.Id, out var usageCount) ? usageCount : 0,
                AppliedAddons = appliedAddons
                    .OrderBy(x => x.Name ?? string.Empty)
                    .ToList()
            });
        }

        return results
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();
    }

    private static List<Guid> GetCategoryChain(Guid? categoryId, Dictionary<Guid, Category> categoryById)
    {
        var chain = new List<Guid>();
        var visited = new HashSet<Guid>();
        var currentId = categoryId;

        while (currentId is not null && visited.Add(currentId.Value))
        {
            chain.Add(currentId.Value);

            if (!categoryById.TryGetValue(currentId.Value, out var category))
            {
                break;
            }

            currentId = category.ParentCategoryId;
        }

        return chain;
    }
}
