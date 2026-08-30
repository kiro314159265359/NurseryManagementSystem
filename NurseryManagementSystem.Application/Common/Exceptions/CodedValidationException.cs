namespace NurseryManagementSystem.Application.Common.Exceptions;

public sealed class CodedValidationException : Exception
{
    public CodedValidationException(string code, string message, string field, params string[] errors)
        : base(message)
    {
        Code = code;
        Errors = new Dictionary<string, string[]>
        {
            [field] = errors.Length == 0 ? new[] { message } : errors
        };
    }

    public string Code { get; }
    public IDictionary<string, string[]> Errors { get; }
}
