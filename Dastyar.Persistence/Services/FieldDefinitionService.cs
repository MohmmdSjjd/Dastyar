using Dastyar.Application.Common.Interfaces;
using Dastyar.Application.Common;
using Dastyar.Domain.Entities;

namespace Dastyar.Persistence.Services;

public sealed class FieldDefinitionService : IFieldDefinitionService
{
    private readonly IRepository<FieldDefinition, Guid> _repo;

    public FieldDefinitionService(IRepository<FieldDefinition, Guid> repo)
    {
        _repo = repo;
    }

    public async Task<string[]> GetFilterableFieldNamesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repo.ListAsync(fd => fd.IsActive && fd.IsFilterable, cancellationToken);
        return items.Select(x => x.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToArray();
    }
}
