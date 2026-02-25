namespace AppSimple.DataLib.Db;

/// <summary>
/// Configuration options for the SQLite database connection.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Gets or sets the SQLite connection string.
    /// Example: <c>Data Source=app.db</c>
    /// </summary>
    public required string ConnectionString { get; set; }
}
