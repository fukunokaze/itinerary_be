namespace itinerary_be.WebAPI.DTOs.Validators;

using FluentValidation;

public class CreateLodgingDtoValidator : AbstractValidator<CreateLodgingDto>
{
    public CreateLodgingDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.CheckIn).NotEmpty().WithMessage("CheckIn is required.");

        RuleFor(x => x.CheckOut).NotEmpty().WithMessage("CheckOut is required.")
            .GreaterThan(x => x.CheckIn).WithMessage("CheckOut must be after CheckIn.");

        RuleFor(x => x.Address).MaximumLength(1000).WithMessage("Address must not exceed 1000 characters.");

        RuleFor(x => x.ConfirmationCode).MaximumLength(100).WithMessage("ConfirmationCode must not exceed 100 characters.");
    }
}
