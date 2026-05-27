using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Products.Commands;

public sealed record UpdateProductCommand(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    string? DynamicFieldsJson = null) : IRequest<Unit>;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly IRepository<Product, Guid> _repository;

    public UpdateProductCommandHandler(IRepository<Product, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            throw new InvalidOperationException("محصول یافت نشد.");
        }

        var currentCode = product.Code?.Trim() ?? string.Empty;
        var newCode = request.Code.Trim();

        if (!System.String.Equals(currentCode, newCode, System.StringComparison.OrdinalIgnoreCase))
        {
            var existingProduct = await _repository.FindAsync(
                x => x.Code.ToLower() == newCode.ToLower(),
                cancellationToken);

            if (existingProduct != null && existingProduct.Id != product.Id)
            {
                throw new InvalidOperationException($"محصول با کد '{newCode}' قبلاً وجود دارد.");
            }
        }

        product.Code = request.Code.Trim();
        product.Name = request.Name.Trim();
        product.IsActive = request.IsActive;

        // به‌روزرسانی فیلدهای دینامیک
        if (request.DynamicFieldsJson != null)
        {
            product.DynamicFieldsJson = request.DynamicFieldsJson;
        }

        await _repository.UpdateAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
