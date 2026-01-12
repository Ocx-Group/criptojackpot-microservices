namespace CryptoJackpot.Domain.Core.Responses.Errors;

public class ValidationError : ApplicationError
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationError(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred", 400)
    {
        Errors = errors;
        Metadata.Add("ValidationErrors", Errors);
    }

    public ValidationError(string propertyName, string errorMessage)
        : base("One or more validation errors occurred", 400)
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, [errorMessage] }
        };

        Metadata.Add("ValidationErrors", Errors);
    }
}

