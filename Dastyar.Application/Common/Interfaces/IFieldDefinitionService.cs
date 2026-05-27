namespace Dastyar.Application.Common.Interfaces;

public interface IFieldDefinitionService
{
    Task<string[]> GetFilterableFieldNamesAsync(CancellationToken cancellationToken = default);
}
