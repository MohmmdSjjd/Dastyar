using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Materials.Commands;

public sealed record DeleteMaterialAddonAssignmentCommand(Guid Id) : IRequest<Unit>;

public sealed class DeleteMaterialAddonAssignmentCommandHandler(
    IRepository<MaterialAddonAssignment, Guid> repository)
    : IRequestHandler<DeleteMaterialAddonAssignmentCommand, Unit>
{
    public async Task<Unit> Handle(DeleteMaterialAddonAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (assignment is null)
        {
            throw new InvalidOperationException("افزودنی پیدا نشد.");
        }

        await repository.DeleteAsync(assignment, cancellationToken);

        return Unit.Value;
    }
}
