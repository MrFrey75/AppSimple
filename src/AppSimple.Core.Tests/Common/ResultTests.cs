using AppSimple.Core.Common;

namespace AppSimple.Core.Tests.Common;

/// <summary>Tests for <see cref="Result{T}"/> and <see cref="Result"/>.</summary>
public sealed class ResultTests
{
    // -------------------------------------------------------------------------
    // Result<T> — Success
    // -------------------------------------------------------------------------

    [Fact]
    public void GenericResult_Success_Succeeded_IsTrue()
    {
        var r = Result<int>.Success(42);
        Assert.True(r.Succeeded);
    }

    [Fact]
    public void GenericResult_Success_Value_IsSet()
    {
        var r = Result<string>.Success("hello");
        Assert.Equal("hello", r.Value);
    }

    [Fact]
    public void GenericResult_Success_Errors_IsEmpty()
    {
        var r = Result<int>.Success(1);
        Assert.Empty(r.Errors);
    }

    [Fact]
    public void GenericResult_Success_Error_IsNull()
    {
        var r = Result<int>.Success(1);
        Assert.Null(r.Error);
    }

    // -------------------------------------------------------------------------
    // Result<T> — Failure
    // -------------------------------------------------------------------------

    [Fact]
    public void GenericResult_Failure_Succeeded_IsFalse()
    {
        var r = Result<int>.Failure("oops");
        Assert.False(r.Succeeded);
    }

    [Fact]
    public void GenericResult_Failure_Value_IsDefault()
    {
        var r = Result<int>.Failure("oops");
        Assert.Equal(default, r.Value);
    }

    [Fact]
    public void GenericResult_Failure_SingleError_SetOnError()
    {
        var r = Result<int>.Failure("bad input");
        Assert.Equal("bad input", r.Error);
    }

    [Fact]
    public void GenericResult_Failure_MultipleErrors_AllPresent()
    {
        var r = Result<int>.Failure(["err1", "err2", "err3"]);
        Assert.Equal(3, r.Errors.Count);
        Assert.Contains("err2", r.Errors);
    }

    // -------------------------------------------------------------------------
    // Result<T> — implicit operator
    // -------------------------------------------------------------------------

    [Fact]
    public void GenericResult_ImplicitConversion_CreatesSuccess()
    {
        Result<string> r = "implicit";
        Assert.True(r.Succeeded);
        Assert.Equal("implicit", r.Value);
    }

    // -------------------------------------------------------------------------
    // Result<T> — ToString
    // -------------------------------------------------------------------------

    [Fact]
    public void GenericResult_Success_ToString_ContainsValue()
    {
        var r = Result<int>.Success(99);
        Assert.Contains("99", r.ToString());
    }

    [Fact]
    public void GenericResult_Failure_ToString_ContainsError()
    {
        var r = Result<int>.Failure("error msg");
        Assert.Contains("error msg", r.ToString());
    }

    // -------------------------------------------------------------------------
    // Result (non-generic) — Success
    // -------------------------------------------------------------------------

    [Fact]
    public void Result_Success_Succeeded_IsTrue()
    {
        Assert.True(Result.Success().Succeeded);
    }

    [Fact]
    public void Result_Success_Errors_IsEmpty()
    {
        Assert.Empty(Result.Success().Errors);
    }

    [Fact]
    public void Result_Success_Error_IsNull()
    {
        Assert.Null(Result.Success().Error);
    }

    // -------------------------------------------------------------------------
    // Result (non-generic) — Failure
    // -------------------------------------------------------------------------

    [Fact]
    public void Result_Failure_Succeeded_IsFalse()
    {
        Assert.False(Result.Failure("oops").Succeeded);
    }

    [Fact]
    public void Result_Failure_SingleError_SetOnError()
    {
        var r = Result.Failure("something went wrong");
        Assert.Equal("something went wrong", r.Error);
    }

    [Fact]
    public void Result_Failure_MultipleErrors_AllPresent()
    {
        var r = Result.Failure(["a", "b"]);
        Assert.Equal(2, r.Errors.Count);
    }

    [Fact]
    public void Result_Success_ToString_ContainsSuccess()
    {
        Assert.Contains("Success", Result.Success().ToString());
    }

    [Fact]
    public void Result_Failure_ToString_ContainsError()
    {
        var r = Result.Failure("fail message");
        Assert.Contains("fail message", r.ToString());
    }
}
