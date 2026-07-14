using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Domain.Models;
using FluentValidation;

namespace CryptoJackpot.Lottery.Application.Validators;

public class UpdatePrizeCommandValidator : AbstractValidator<UpdatePrizeCommand>
{
    public UpdatePrizeCommandValidator()
    {
        RuleFor(c => c.PrizeId)
            .NotEmpty().WithMessage("PrizeId is required");

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("Description is required");

        RuleFor(c => c.EstimatedValue)
            .GreaterThanOrEqualTo(0).WithMessage("EstimatedValue must be greater than or equal to 0");

        RuleFor(c => c.Type)
            .IsInEnum().WithMessage("Type must be a valid PrizeType");

        RuleFor(c => c.Tier)
            .GreaterThan(0).WithMessage("Tier must be greater than 0");

        RuleFor(c => c.MainImageUrl)
            .NotEmpty().WithMessage("MainImageUrl is required")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("MainImageUrl must be a valid absolute URL");

        RuleForEach(c => c.AdditionalImages)
            .ChildRules(img =>
            {
                img.RuleFor(i => i.ImageUrl)
                    .NotEmpty().WithMessage("ImageUrl is required")
                    .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                    .WithMessage("ImageUrl must be a valid absolute URL");
                
                img.RuleFor(i => i.DisplayOrder)
                    .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder must be greater than or equal to 0");
            });

        RuleFor(c => c.CashAlternative)
            .GreaterThan(0).When(c => c.CashAlternative.HasValue)
            .WithMessage("CashAlternative must be greater than 0 when provided");

        // Translations: base fields are Spanish; "en" and "pt" are required so users
        // always see the content in their selected language (no Spanish fallback)
        RuleFor(c => c.Translations)
            .NotNull().WithMessage("Translations are required (en, pt)")
            .Must(tr => tr is null || TranslationLanguages.Supported.All(tr.ContainsKey))
            .WithMessage("Translations must include: en, pt");

        RuleForEach(c => c.Translations)
            .ChildRules(entry =>
            {
                entry.RuleFor(e => e.Key)
                    .Must(TranslationLanguages.Supported.Contains)
                    .WithMessage("Translation language must be one of: en, pt");

                entry.RuleFor(e => e.Value)
                    .NotNull().WithMessage("Translation content is required");

                entry.RuleFor(e => e.Value.Name)
                    .NotEmpty().WithMessage("Translated Name is required")
                    .MaximumLength(100).WithMessage("Translated Name must not exceed 100 characters")
                    .When(e => e.Value is not null);

                entry.RuleFor(e => e.Value.Description)
                    .NotEmpty().WithMessage("Translated Description is required")
                    .When(e => e.Value is not null);
            })
            .When(c => c.Translations is not null);
    }
}

