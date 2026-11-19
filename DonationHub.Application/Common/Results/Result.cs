using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Common.Results
{
    public enum ErrorType
    {
        Failure,               // 500
        Validation,            // 400
        Unauthorized,          // 401
        Forbidden,             // 403
        NotFound,              // 404
        Conflict,              // 409
        PayloadTooLarge,       // 413
        UnprocessableEntity,   // 422
        TooManyRequests        // 429
    }
    public sealed record Error(
    string Code,
    string Message,
    ErrorType Type,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null);
    public class Result
    {
        private readonly Error? _error;
        protected Result(Error? error) => _error = error;
        public bool IsSuccess => _error is null;
        public bool IsFailure => !IsSuccess;
        public Error Error => _error ?? throw new InvalidOperationException("No error available for a successful result.");

        public static Result Success() => new Result(null);
        public static Result Failure(Error error) => new Result(error);
        public static implicit operator Result(Error error) => Failure(error);
    }
    public sealed class Result<T> : Result
    {
        private readonly T? _value;

        private Result(T value) : base(null) => _value = value;
        private Result(Error error) : base(error) { }

        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("A failed result has no value.");

        public static Result<T> Success(T value) => new(value);
        public new static Result<T> Failure(Error error) => new(error);

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(Error error) => Failure(error);
    }
}
