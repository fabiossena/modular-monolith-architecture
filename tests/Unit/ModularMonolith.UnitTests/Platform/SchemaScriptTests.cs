using ModularMonolith.Shared.Platform.Persistence;
using Xunit;

namespace ModularMonolith.UnitTests.Platform;

public class SchemaScriptTests
{
    [Fact]
    public void Create_table_becomes_idempotent()
    {
        var statements = SchemaScript.Split("CREATE TABLE \"Products\" (\"Id\" INTEGER NOT NULL);");

        Assert.Equal("CREATE TABLE IF NOT EXISTS \"Products\" (\"Id\" INTEGER NOT NULL)", Assert.Single(statements));
    }

    [Fact]
    public void Create_index_becomes_idempotent()
    {
        var statements = SchemaScript.Split("CREATE INDEX \"IX_Products_Sku\" ON \"Products\" (\"Sku\");");

        Assert.Equal("CREATE INDEX IF NOT EXISTS \"IX_Products_Sku\" ON \"Products\" (\"Sku\")", Assert.Single(statements));
    }

    [Fact]
    public void Create_unique_index_becomes_idempotent()
    {
        var statements = SchemaScript.Split("CREATE UNIQUE INDEX \"IX_Products_Sku\" ON \"Products\" (\"Sku\");");

        Assert.Equal(
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Products_Sku\" ON \"Products\" (\"Sku\")",
            Assert.Single(statements));
    }

    [Fact]
    public void Every_statement_of_a_script_is_returned()
    {
        var script = """
                     CREATE TABLE "A" ("Id" INTEGER NOT NULL);
                     CREATE TABLE "B" ("Id" INTEGER NOT NULL);
                     CREATE INDEX "IX_A_Id" ON "A" ("Id");
                     """;

        var statements = SchemaScript.Split(script).ToList();

        Assert.Equal(3, statements.Count);
        Assert.All(statements, statement => Assert.Contains("IF NOT EXISTS", statement));
    }
}
