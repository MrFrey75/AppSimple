using System.Data;
using Dapper;
using System.Text.Json;

namespace AppSimple.DataLib.Db;

/// <summary>
/// Dapper type handler that serialises and deserialises <see cref="List{T}"/> of strings
/// as a JSON TEXT column in SQLite.
/// </summary>
public sealed class JsonStringListTypeHandler : SqlMapper.TypeHandler<List<string>>
{
    /// <summary>Singleton instance for registration.</summary>
    public static readonly JsonStringListTypeHandler Instance = new();

    private JsonStringListTypeHandler() { }

    /// <inheritdoc />
    public override void SetValue(IDbDataParameter parameter, List<string>? value)
    {
        parameter.Value = value is null ? "[]" : JsonSerializer.Serialize(value);
        parameter.DbType = DbType.String;
    }

    /// <inheritdoc />
    public override List<string> Parse(object value)
    {
        if (value is string s && !string.IsNullOrWhiteSpace(s))
            return JsonSerializer.Deserialize<List<string>>(s) ?? [];
        return [];
    }
}
