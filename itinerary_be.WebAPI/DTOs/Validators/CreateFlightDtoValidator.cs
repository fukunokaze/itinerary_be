namespace itinerary_be.WebAPI.DTOs.Validators;

using FluentValidation;

public class CreateFlightDtoValidator : AbstractValidator<CreateFlightDto>
{
    public CreateFlightDtoValidator()
    {
        RuleFor(x => x.FlightNumber).NotEmpty().WithMessage("FlightNumber is required.")
            .MaximumLength(20).WithMessage("FlightNumber must not exceed 20 characters.");

        RuleFor(x => x.ArrivalTime).GreaterThan(x => x.DepartureTime)
            .WithMessage("ArrivalTime must be after DepartureTime.");

        RuleFor(x => x.Airline).MaximumLength(100).WithMessage("Airline must not exceed 100 characters.");

        RuleFor(x => x.Seat).MaximumLength(20).WithMessage("Seat must not exceed 20 characters.");

        RuleFor(x => x.ConfirmationCode).MaximumLength(50).WithMessage("ConfirmationCode must not exceed 50 characters.");
    }
}
