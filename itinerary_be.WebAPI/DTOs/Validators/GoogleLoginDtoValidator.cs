namespace itinerary_be.WebAPI.DTOs.Validators;

using FluentValidation;

public class GoogleLoginDtoValidator : AbstractValidator<GoogleLoginDto>
{
    public GoogleLoginDtoValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty().WithMessage("IdToken is required.");
    }
}
