using System.Data;

namespace AppSimple.DataLib.Db;

/// <summary>
/// Abstracts the creation of database connections for use by repositories.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates and opens a new database connection.
    /// </summary>
    /// <returns>An open <see cref="IDbConnection"/>.</returns>
    IDbConnection CreateConnection();
}
