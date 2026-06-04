namespace MyOS.Core.Application.Abstractions.Results
{
    public sealed class Result<T> : IResult<Result<T>>
    {
        private readonly T? _value;
        private Result(bool isSuccess, T? value, Error error)
        {
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException(
                    "Successful result cannot contain an error.");

            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException(
                    "Failed result must contain an error.");

            IsSuccess = isSuccess;
            _value = value;
            Error = error;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }
        public T Value => IsSuccess ? _value! : 
            throw new InvalidOperationException("Cannot access value of a failed result.");

        public static Result<T> Success(T value) =>
            new(true, value, Error.None);

        public static Result<T> Failure(Error error) =>
            new(false, default, error);
    }
}
