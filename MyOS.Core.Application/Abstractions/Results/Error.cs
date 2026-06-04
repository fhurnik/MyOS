namespace MyOS.Core.Application.Abstractions.Results
{
    public sealed record Error(
        string Code,
        string Message,
        ErrorType Type,
        IReadOnlyDictionary<string, string>? Parameters = null)
    {
        public static readonly Error None = new(
            Code: string.Empty,
            Message: string.Empty,
            Type: ErrorType.None);

        public static Error Validation(string code, string message = "",
            IReadOnlyDictionary<string, string>? parameters = null) =>
            new(code, message, ErrorType.Validation, parameters);

        public static Error NotFound(string code,
            IReadOnlyDictionary<string, string>? parameters = null,
            string message = "") =>
            new(code, message, ErrorType.NotFound, parameters);

        public static Error Conflict(string code,
            IReadOnlyDictionary<string, string>? parameters = null,
            string message = "") =>
            new(code, message, ErrorType.Conflict, parameters);

        public static Error Unauthorized(string code,
            IReadOnlyDictionary<string, string>? parameters = null,
            string message = "") =>
            new(code, message, ErrorType.Unauthorized, parameters);

        public static Error Forbidden(string code,
            IReadOnlyDictionary<string, string>? parameters = null,
            string message = "") =>
            new(code, message, ErrorType.Forbidden, parameters);

        public static Error Failure(string code,
            IReadOnlyDictionary<string, string>? parameters = null,
            string message = "") =>
            new(code, message, ErrorType.Failure, parameters);

        public static Error Unexpected(string code,
            IReadOnlyDictionary<string, string>? parameters = null,
            string message = "") =>
            new(code, message, ErrorType.Unexpected, parameters);
    }
}
