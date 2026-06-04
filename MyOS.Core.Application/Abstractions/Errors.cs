namespace MyOS.Core.Application.Abstractions
{
    public abstract class ErrorCodes
    {
        // This hierarchy exists solely as a reflection target:
        // the translation completeness test discovers all error code classes via typeof(ErrorCodes).
        // Subclasses must declare a private constructor to prevent instantiation.
        protected ErrorCodes() { }
    }
}
