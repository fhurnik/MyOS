using MyOS.Core.Domain.Enums;

namespace MyOS.Core.Application.Abstractions
{
    public interface IErrorMessageProvider
    {
        string? TryGet(string errorCode, Language language);
    }
}
