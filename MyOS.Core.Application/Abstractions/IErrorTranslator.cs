using MyOS.Core.Application.Abstractions.Results;
using MyOS.Core.Domain.Enums;

namespace MyOS.Core.Application.Abstractions
{
    public interface IErrorTranslator
    {
        string Translate(Error error, Language language);
    }
}
