using System;
using System.Collections.Generic;
using System.Linq;

namespace Antifraud.Application.DTOs;

public class Result<T>
{
    public bool IsSuccess { get; private init; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; private init; }
    public string Error { get; private init; } = string.Empty;
    public IEnumerable<string> Errors { get; private init; } = Enumerable.Empty<string>();

    private Result(bool isSuccess, T? value, string error, IEnumerable<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Errors = errors ?? Enumerable.Empty<string>();
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);
    
    public static Result<T> Failure(string error) => new(false, default, error);
    
    public static Result<T> Failure(IEnumerable<string> errors) => 
        new(false, default, string.Join("; ", errors), errors);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Error);
    }
}

public static class Result
{
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
    public static Result<T> Failure<T>(IEnumerable<string> errors) => Result<T>.Failure(errors);
}