namespace itinerary_be.WebAPI.DTOs.Validators;

using FluentValidation;

public class GoogleLoginDtoValidator : AbstractValidator<GoogleLoginDto>
{
    public GoogleLoginDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Code is required.");
    }
}
