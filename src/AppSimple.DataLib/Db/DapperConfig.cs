using System.Data;
using Dapper;

namespace AppSimple.DataLib.Db;

/// <summary>
/// Registers custom Dapper type handlers for SQLite type mappings.
/// Call <see cref="Register"/> once at application startup.
/// </summary>
public static class DapperConfig
{
    /// <summary>
    /// Registers all custom type handlers with Dapper.
    /// This must be called before any database queries are executed.
    /// </summary>
    public static void Register()
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeUtcTypeHandler());
        SqlMapper.AddTypeHandler(JsonStringListTypeHandler.Instance);
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
    }

    private sealed class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
            => parameter.Value = value.ToString();

        public override Guid Parse(object value)
            => Guid.Parse(value.ToString()!);
    }

    private sealed class DateTimeUtcTypeHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override void SetValue(IDbDataParameter parameter, DateTime value)
            => parameter.Value = value.ToString("O");

        public override DateTime Parse(object value)
        {
            var dt = DateTime.Parse(value.ToString()!, null, System.Globalization.DateTimeStyles.RoundtripKind);
            return dt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                : dt.ToUniversalTime();
        }
    }
}
