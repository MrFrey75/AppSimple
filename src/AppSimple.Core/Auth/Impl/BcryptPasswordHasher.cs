using AppSimple.Core.Auth;
using BC = BCrypt.Net.BCrypt;

namespace AppSimple.Core.Auth.Impl;

/// <summary>
/// BCrypt-backed implementation of <see cref="IPasswordHasher"/>.
/// Uses a work factor of 12 by default, which balances security and performance.
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    /// <inheritdoc />
    public string Hash(string plainPassword)
        => BC.HashPassword(plainPassword, WorkFactor);

    /// <inheritdoc />
    public bool Verify(string plainPassword, string hash)
        => BC.Verify(plainPassword, hash);
}
