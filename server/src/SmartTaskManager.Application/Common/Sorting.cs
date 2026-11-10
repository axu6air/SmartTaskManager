using System.Linq.Expressions;

namespace SmartTaskManager.Application.Common;

// The only way a list query is sorted: a request names a field from a whitelist, never an expression.
public sealed class SortFields<T>(string defaultField, IReadOnlyDictionary<string, Expression<Func<T, object?>>> fields)
{
    public const string Ascending = "asc";
    public const string Descending = "desc";

    private readonly Dictionary<string, Expression<Func<T, object?>>> _fields =
        new(fields, StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> Names => _fields.Keys;

    public bool IsAllowed(string? field) => string.IsNullOrEmpty(field) || _fields.ContainsKey(field);

    public static bool IsDirection(string? direction) =>
        string.IsNullOrEmpty(direction)
        || direction.Equals(Ascending, StringComparison.OrdinalIgnoreCase)
        || direction.Equals(Descending, StringComparison.OrdinalIgnoreCase);

    // Call only after validation: an unknown field falls back to the default.
    public (Expression<Func<T, object?>> SortBy, bool Ascending) Resolve(string? field, string? direction) =>
        (
            _fields.TryGetValue(field ?? defaultField, out var sortBy) ? sortBy : _fields[defaultField],
            !Descending.Equals(direction, StringComparison.OrdinalIgnoreCase)
        );
}
