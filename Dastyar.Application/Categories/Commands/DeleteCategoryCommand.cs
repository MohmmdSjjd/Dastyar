using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Categories.Commands;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<Unit>;

public sealed class DeleteCategoryCommandHandler(IRepository<Category, Guid> repository)
    : IRequestHandler<DeleteCategoryCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
        {
            throw new InvalidOperationException("دسته‌بندی پیدا نشد.");
        }

        await repository.DeleteAsync(category, cancellationToken);
        return Unit.Value;
    }
}
