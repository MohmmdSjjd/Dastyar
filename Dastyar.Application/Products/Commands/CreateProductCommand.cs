using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.Products.Commands;

public sealed record CreateProductCommand(string Name, string? Code = null, string? CategoryCode = null) : IRequest<Guid>;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IRepository<Product, Guid> _repository;
    private readonly IRepository<Category, Guid> _categoryRepository;

    public CreateProductCommandHandler(
        IRepository<Product, Guid> repository,
        IRepository<Category, Guid> categoryRepository)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = string.IsNullOrWhiteSpace(request.CategoryCode)
            ? null
            : await _categoryRepository.FindAsync(x => x.Code == request.CategoryCode.Trim(), cancellationToken);

        var codeToUse = string.IsNullOrWhiteSpace(request.Code)
            ? Guid.NewGuid().ToString("N")
            : request.Code.Trim();

        // Check if code already exists (case-insensitive)
        var existingProduct = await _repository.FindAsync(x => x.Code.ToLower() == codeToUse.ToLower(), cancellationToken);
        if (existingProduct != null)
        {
            throw new InvalidOperationException($"محصول با کد '{codeToUse}' قبلاً وجود دارد.");
        }

        var entity = new Product
        {
            Id = Guid.NewGuid(),
            Code = codeToUse,
            Name = request.Name.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            CategoryId = category?.Id
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
