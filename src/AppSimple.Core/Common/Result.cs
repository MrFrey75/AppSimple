namespace AppSimple.Core.Common;

/// <summary>
/// Represents the outcome of an operation that returns a value.
/// Use <see cref="Success"/> and <see cref="Failure(string)"/> factories instead of the constructor.
/// </summary>
/// <typeparam name="T">The type of the returned value on success.</typeparam>
public sealed class Result<T>
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool Succeeded { get; }

    /// <summary>Gets the returned value when the operation succeeded; otherwise <c>default</c>.</summary>
    public T? Value { get; }

    /// <summary>Gets the list of error messages when the operation failed; empty on success.</summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>Gets the first error message, or <c>null</c> if the operation succeeded.</summary>
    public string? Error => Errors.Count > 0 ? Errors[0] : null;

    private Result(bool succeeded, T? value, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Value     = value;
        Errors    = errors;
    }

    /// <summary>Creates a successful result carrying <paramref name="value"/>.</summary>
    public static Result<T> Success(T value)
        => new(true, value, []);

    /// <summary>Creates a failed result with a single error message.</summary>
    public static Result<T> Failure(string error)
        => new(false, default, [error]);

    /// <summary>Creates a failed result with multiple error messages.</summary>
    public static Result<T> Failure(IEnumerable<string> errors)
        => new(false, default, errors.ToList());

    /// <summary>Implicitly converts a value to a successful <see cref="Result{T}"/>.</summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <inheritdoc />
    public override string ToString()
        => Succeeded ? $"Success({Value})" : $"Failure({string.Join("; ", Errors)})";
}

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// </summary>
public sealed class Result
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool Succeeded { get; }

    /// <summary>Gets the list of error messages when the operation failed; empty on success.</summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>Gets the first error message, or <c>null</c> if the operation succeeded.</summary>
    public string? Error => Errors.Count > 0 ? Errors[0] : null;

    private Result(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors    = errors;
    }

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, []);

    /// <summary>Creates a failed result with a single error message.</summary>
    public static Result Failure(string error) => new(false, [error]);

    /// <summary>Creates a failed result with multiple error messages.</summary>
    public static Result Failure(IEnumerable<string> errors) => new(false, errors.ToList());

    /// <inheritdoc />
    public override string ToString()
        => Succeeded ? "Success" : $"Failure({string.Join("; ", Errors)})";
}
