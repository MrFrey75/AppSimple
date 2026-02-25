namespace AppSimple.Core.Services;

/// <summary>
/// Combined user management contract that provides both query and command operations.
/// Extends <see cref="IUserQueryService"/> and <see cref="IUserCommandService"/> for
/// callers that need the full surface area.
/// </summary>
public interface IUserService : IUserQueryService, IUserCommandService
{
}
