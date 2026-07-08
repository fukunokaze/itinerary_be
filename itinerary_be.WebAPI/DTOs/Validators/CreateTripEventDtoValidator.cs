namespace itinerary_be.WebAPI.DTOs.Validators;

using FluentValidation;

public class CreateTripEventDtoValidator : AbstractValidator<CreateTripEventDto>
{
    public CreateTripEventDtoValidator()
    {
        RuleFor(x => x.Type).IsInEnum().WithMessage("Valid event type is required.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
        
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime)
            .When(x => x.StartTime.HasValue && x.EndTime.HasValue)
            .WithMessage("StartTime must be before EndTime.");
    }
}
