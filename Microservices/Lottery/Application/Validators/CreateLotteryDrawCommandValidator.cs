using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Domain.Enums;
using CryptoJackpot.Lottery.Domain.Models;
using FluentValidation;

namespace CryptoJackpot.Lottery.Application.Validators;

public class CreateLotteryDrawCommandValidator : AbstractValidator<CreateLotteryDrawCommand>
{
    public CreateLotteryDrawCommandValidator()
    {
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(c => c.MinNumber)
            .GreaterThanOrEqualTo(0).WithMessage("MinNumber must be greater than or equal to 0");

        RuleFor(c => c.MaxNumber)
            .GreaterThan(0).WithMessage("MaxNumber must be greater than 0")
            .GreaterThan(c => c.MinNumber).WithMessage("MaxNumber must be greater than MinNumber");
        
        RuleFor(c => c.TicketPrice)
            .GreaterThan(0).WithMessage("TicketPrice must be greater than 0");

        RuleFor(c => c.MaxTickets)
            .GreaterThan(0).WithMessage("MaxTickets must be greater than 0");

        RuleFor(c => c.ReferralCommissionPercentage)
            .InclusiveBetween(0, 100).WithMessage("ReferralCommissionPercentage must be between 0 and 100");

        RuleFor(c => c.StartDate)
            .NotEmpty().WithMessage("StartDate is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("StartDate must be today or in the future");

        RuleFor(c => c.EndDate)
            .NotEmpty().WithMessage("EndDate is required")
            .GreaterThan(c => c.StartDate).WithMessage("EndDate must be after StartDate");

        RuleFor(c => c.Status)
            .IsInEnum().WithMessage("Status must be a valid LotteryStatus");

        RuleFor(c => c.Type)
            .IsInEnum().WithMessage("Type must be a valid LotteryType");

        RuleFor(c => c.Terms)
            .NotEmpty().WithMessage("Terms is required");

        RuleFor(c => c.MinimumAge)
            .GreaterThanOrEqualTo(18).When(c => c.HasAgeRestriction)
            .WithMessage("MinimumAge must be at least 18 when age restriction is enabled");

        RuleFor(c => c.CryptoCurrencyId)
            .NotEmpty().WithMessage("CryptoCurrencyId is required")
            .MaximumLength(100).WithMessage("CryptoCurrencyId must not exceed 100 characters");

        RuleFor(c => c.CryptoCurrencySymbol)
            .NotEmpty().WithMessage("CryptoCurrencySymbol is required")
            .MaximumLength(100).WithMessage("CryptoCurrencySymbol must not exceed 100 characters");

        // Standard (rifa/promoción): fixed configuration (0000-9999, 10000 numbers, 1 series).
        // Each ticket is two 00-99 pairs sold as a single 4-digit number.
        When(c => c.Type == LotteryType.Standard, () =>
        {
            RuleFor(c => c.MinNumber)
                .Equal(0).WithMessage("Standard promotion MinNumber must be 0");

            RuleFor(c => c.MaxNumber)
                .Equal(9999).WithMessage("Standard promotion MaxNumber must be 9999");

            RuleFor(c => c.MaxTickets)
                .Equal(10000).WithMessage("Standard promotion MaxTickets must be 10000");
        });

        // Pick3: fixed configuration (000-999, 1000 numbers)
        When(c => c.Type == LotteryType.Pick3, () =>
        {
            RuleFor(c => c.MinNumber)
                .Equal(0).WithMessage("Pick3 MinNumber must be 0");

            RuleFor(c => c.MaxNumber)
                .Equal(999).WithMessage("Pick3 MaxNumber must be 999");

            RuleFor(c => c.MaxTickets)
                .Equal(1000).WithMessage("Pick3 MaxTickets must be 1000");
        });

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

                entry.RuleFor(e => e.Value.Title)
                    .NotEmpty().WithMessage("Translated Title is required")
                    .MaximumLength(200).WithMessage("Translated Title must not exceed 200 characters")
                    .When(e => e.Value is not null);

                entry.RuleFor(e => e.Value.Description)
                    .NotEmpty().WithMessage("Translated Description is required")
                    .MaximumLength(2000).WithMessage("Translated Description must not exceed 2000 characters")
                    .When(e => e.Value is not null);

                entry.RuleFor(e => e.Value.Terms)
                    .NotEmpty().WithMessage("Translated Terms is required")
                    .When(e => e.Value is not null);
            })
            .When(c => c.Translations is not null);
    }
}

