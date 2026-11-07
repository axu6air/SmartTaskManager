using FluentAssertions;
using SmartTaskManager.Infrastructure.Persistence.Initialization;

namespace SmartTaskManager.Infrastructure.UnitTests.Persistence;

public class SqlBatchSplitterTests
{
    [Fact]
    public void Split_ScriptWithGoLines_ReturnsOneBatchPerSection()
    {
        const string script = "SELECT 1;\nGO\nSELECT 2;\r\nGO\r\n  go  \nSELECT 3;";

        var batches = SqlBatchSplitter.Split(script);

        batches.Should().Equal("SELECT 1;", "SELECT 2;", "SELECT 3;");
    }

    [Fact]
    public void Split_GoInsideAWord_IsNotASeparator()
    {
        const string script = "SELECT 'GO';\nINSERT INTO Cargo VALUES (1);";

        var batches = SqlBatchSplitter.Split(script);

        batches.Should().ContainSingle();
    }

    [Fact]
    public void Split_EmptyScript_ReturnsNoBatches()
    {
        SqlBatchSplitter.Split("  \nGO\n  ").Should().BeEmpty();
    }

    [Fact]
    public void Split_EmbeddedSchemaScript_ContainsEveryTableAndNoGoLines()
    {
        using var stream = typeof(DependencyInjection).Assembly.GetManifestResourceStream("rbac-schema.sql");
        stream.Should().NotBeNull("the schema script is embedded in the Infrastructure assembly");
        var script = new StreamReader(stream).ReadToEnd();

        var batches = SqlBatchSplitter.Split(script);

        batches.Count(batch => batch.Contains("CREATE TABLE")).Should().Be(11);
        batches.Should().NotContain(batch => batch.Split('\n').Any(line => line.Trim() == "GO"));
    }
}
