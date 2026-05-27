using Dastyar.Application.FieldDefinitions.Commands;
using FluentValidation;

namespace Dastyar.Application.FieldDefinitions.Validators;

public sealed class CreateFieldDefinitionValidator : AbstractValidator<CreateFieldDefinitionCommand>
{
    public CreateFieldDefinitionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DisplayName).MaximumLength(300);
        RuleFor(x => x.DataType).NotEmpty().MaximumLength(50);
    }
}
