namespace MyOS.Core.Application.Exceptions
{
    public sealed class UniqueConstraintViolationException(Exception innerException)
        : Exception("A unique constraint was violated.", innerException);
}
