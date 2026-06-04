using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Core.Domain.Enums;
using System.Text.RegularExpressions;

namespace MyOS.Core.Infrastructure.Services
{
    internal sealed class ErrorTranslator(IEnumerable<IErrorMessageProvider> providers) : IErrorTranslator
    {
        public string Translate(Error error, Language language)
        {
            if (!string.IsNullOrEmpty(error.Message))
                return error.Message;

            string? message = null;
            foreach (var provider in providers)
            {
                message = provider.TryGet(error.Code, language);
                if (message is not null)
                    break;
            }

            message ??= error.Code;

            if (error.Parameters is { Count: > 0 })
                message = ReplacePlaceholders(message, error.Parameters);

            return message;
        }

        private static string ReplacePlaceholders(string message, IReadOnlyDictionary<string, string> parameters)
        {
            return Regex.Replace(message, @"\{(\w+)\}", match =>
            {
                var key = match.Groups[1].Value;
                return parameters.TryGetValue(key, out var value) ? value : match.Value;
            });
        }
    }
}
