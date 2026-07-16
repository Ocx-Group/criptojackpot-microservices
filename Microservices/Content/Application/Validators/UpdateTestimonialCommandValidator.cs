using CryptoJackpot.Content.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Content.Application.Validators;

public class UpdateTestimonialCommandValidator : AbstractValidator<UpdateTestimonialCommand>
{
    public UpdateTestimonialCommandValidator()
    {
        RuleFor(c => c.TestimonialId)
            .NotEmpty().WithMessage("Testimonial ID is required");

        RuleFor(c => c.AuthorName)
            .NotEmpty().WithMessage("Author name is required")
            .MaximumLength(100).WithMessage("Author name must not exceed 100 characters");

        RuleFor(c => c.AuthorLocation)
            .NotEmpty().WithMessage("Author location is required")
            .MaximumLength(100).WithMessage("Author location must not exceed 100 characters");

        RuleFor(c => c.AuthorImageUrl)
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Author image URL must be a valid absolute URL");

        RuleFor(c => c.Text)
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(500).WithMessage("Text must not exceed 500 characters");

        RuleFor(c => c.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(c => c.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be greater than or equal to 0");
    }
}
