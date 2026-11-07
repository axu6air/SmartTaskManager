using System.Text.RegularExpressions;

namespace SmartTaskManager.Infrastructure.Persistence.Initialization;

// GO is an SSMS/sqlcmd batch separator, not T-SQL, so a script has to be split before SqlClient runs it.
public static partial class SqlBatchSplitter
{
    public static IReadOnlyList<string> Split(string script)
    {
        return GoSeparator()
            .Split(script)
            .Select(batch => batch.Trim())
            .Where(batch => batch.Length > 0)
            .ToList();
    }

    [GeneratedRegex(@"^[ \t]*GO[ \t]*\r?$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex GoSeparator();
}
