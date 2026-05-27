using Dastyar.Application.FieldDefinitions.Commands;
using FluentValidation;

namespace Dastyar.Application.FieldDefinitions.Validators;

public sealed class UpdateFieldDefinitionValidator : AbstractValidator<UpdateFieldDefinitionCommand>
{
    public UpdateFieldDefinitionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DisplayName).MaximumLength(300);
        RuleFor(x => x.DataType).NotEmpty().MaximumLength(50);
    }
}
