using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.FieldDefinitions.Commands;

public sealed record DeleteFieldDefinitionCommand(Guid Id) : IRequest;

public sealed class DeleteFieldDefinitionCommandHandler : IRequestHandler<DeleteFieldDefinitionCommand>
{
    private readonly IRepository<FieldDefinition, Guid> _repository;

    public DeleteFieldDefinitionCommandHandler(IRepository<FieldDefinition, Guid> repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteFieldDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
