namespace AppSimple.DataLib.Services;

/// <summary>
/// Defines the database reset and reseed operation.
/// Drops all user data, recreates the schema, and seeds the default admin user
/// plus a set of sample users.
/// </summary>
/// <remarks>
/// ⚠️ This operation is destructive and irreversible.
/// It should only be accessible to authenticated admin users.
/// Any active session should be invalidated immediately after calling this.
/// </remarks>
public interface IDatabaseResetService
{
    /// <summary>
    /// Erases all data, recreates the schema, and reseeds the default
    /// admin user and sample users.
    /// </summary>
    Task ResetAndReseedAsync();
}
