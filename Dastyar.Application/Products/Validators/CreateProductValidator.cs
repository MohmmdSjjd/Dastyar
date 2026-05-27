using Dastyar.Application.Products.Commands;
using FluentValidation;

namespace Dastyar.Application.Products.Validators;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .MaximumLength(100);

        RuleFor(x => x.CategoryCode)
            .MaximumLength(100);
    }
}
