using SqlFormatter.Services;

namespace SqlFormatter.Tests;

public class TSqlFormatterServiceTests
{
    private readonly TSqlFormatterService _formatter;

    public TSqlFormatterServiceTests()
    {
        _formatter = new TSqlFormatterService();
    }

    [Fact]
    public void Format_EmptyString_ReturnsEmpty()
    {
        // Arrange
        var input = string.Empty;

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Format_WhitespaceOnly_ReturnsEmpty()
    {
        // Arrange
        var input = "   \t\n  ";

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Format_SimpleSelectQuery_FormatsCorrectly()
    {
        // Arrange
        var input = "select code_tag, count(code_site) from idex..TAB_BULK_TW_SiteTag group by code_tag order by code_tag";

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("SELECT", result);
        Assert.Contains("FROM", result);
        Assert.Contains("GROUP BY", result);
        Assert.Contains("ORDER BY", result);
        Assert.Contains("    code_tag,", result); // Check indentation
    }

    [Fact]
    public void Format_SelectWithMultipleColumns_IndentsEachColumn()
    {
        // Arrange
        var input = "select id, name, email from users";

        // Act
        var result = _formatter.Format(input);

        // Assert
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.Contains(lines, l => l.Trim().StartsWith("id,"));
        Assert.Contains(lines, l => l.Trim().StartsWith("name,"));
        Assert.Contains(lines, l => l.Contains("email")); // Last column doesn't have comma
    }

    [Fact]
    public void Format_KeywordsAreUppercased()
    {
        // Arrange
        var input = "select id from users where status = 'active'";

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("SELECT", result);
        Assert.Contains("FROM", result);
        Assert.Contains("WHERE", result);
        Assert.DoesNotContain("select", result);
        Assert.DoesNotContain("from", result);
        Assert.DoesNotContain("where", result);
    }

    [Fact]
    public void Format_SelectWithJoin_FormatsCorrectly()
    {
        // Arrange
        var input = "select u.id, u.name from users u inner join orders o on u.id = o.user_id";

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("SELECT", result);
        Assert.Contains("FROM", result);
        Assert.Contains("INNER JOIN", result);
    }

    [Fact]
    public void Format_ComplexQueryWithGroupByAndOrderBy_FormatsCorrectly()
    {
        // Arrange
        var input = "select department, count(*) as emp_count from employees group by department order by emp_count desc";

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("SELECT", result);
        Assert.Contains("FROM", result);
        Assert.Contains("GROUP BY", result);
        Assert.Contains("ORDER BY", result);

        // Verify structure has line breaks
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length > 4, "Formatted query should have multiple lines");
    }

    [Fact]
    public void Format_QueryWithParentheses_PreservesParentheses()
    {
        // Arrange
        var input = "select count(id) from users";

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("count(id)", result);
    }

    [Fact]
    public void Format_QueryWithStringLiterals_PreservesStrings()
    {
        // Arrange
        var input = "select * from users where name = 'John Doe'";

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("'John Doe'", result);
    }

    [Theory]
    [InlineData("select * from users")]
    [InlineData("SELECT * FROM USERS")]
    [InlineData("SeLeCt * FrOm UsErS")]
    public void Format_DifferentCasing_ProducesConsistentOutput(string input)
    {
        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("SELECT", result);
        Assert.Contains("FROM", result);
    }

    [Fact]
    public void Format_ExampleFromRequirements_MatchesExpectedOutput()
    {
        // Arrange
        var input = "select code_tag, count(code_site) from idex..TAB_BULK_TW_SiteTag group by code_tag order by code_tag";

        // Act
        var result = _formatter.Format(input);

        // Assert - verify key formatting elements
        Assert.StartsWith("SELECT", result.TrimStart());
        Assert.Contains("code_tag,", result);
        Assert.Contains("count(code_site)", result);
        Assert.Contains("FROM", result);
        Assert.Contains("idex..TAB_BULK_TW_SiteTag", result);
        Assert.Contains("GROUP BY", result);
        Assert.Contains("ORDER BY", result);

        // Verify it's multi-line
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length >= 5, "Formatted query should have at least 5 lines");
    }

    [Fact]
    public void Format_InsertStatement_FormatsCorrectly()
    {
        // Arrange
        var input = "insert into users (id, name, email) values (1, 'John Doe', 'john@example.com')";

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("INSERT INTO", result);
        Assert.Contains("users", result);
        Assert.Contains("id,", result);
        Assert.Contains("name,", result);
        Assert.Contains("email", result);
        Assert.Contains("VALUES", result);
        Assert.Contains("'John Doe'", result);

        // Verify it's multi-line
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length >= 3, "Formatted INSERT should have multiple lines");
    }

    [Fact]
    public void Format_InsertWithMultipleColumns_IndentsEachColumn()
    {
        // Arrange
        var input = "insert into products (product_id, product_name, price, stock) values (101, 'Widget', 19.99, 50)";

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("INSERT INTO", result);
        Assert.Contains("products", result);
        Assert.Contains("product_id,", result);
        Assert.Contains("product_name,", result);
        Assert.Contains("price,", result);
        Assert.Contains("stock", result);
        Assert.Contains("VALUES", result);

        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length >= 4, "Formatted INSERT with multiple columns should have multiple lines");
    }

    [Fact]
    public void Format_UpdateStatement_FormatsCorrectly()
    {
        // Arrange
        var input = "update users set name = 'Jane Doe', email = 'jane@example.com' where id = 1";

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("UPDATE", result);
        Assert.Contains("users", result);
        Assert.Contains("SET", result);
        Assert.Contains("name =", result);
        Assert.Contains("'Jane Doe'", result);
        Assert.Contains("email =", result);
        Assert.Contains("'jane@example.com'", result);
        Assert.Contains("WHERE", result);
        Assert.Contains("id = 1", result);

        // Verify it's multi-line
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length >= 3, "Formatted UPDATE should have multiple lines");
    }

    [Fact]
    public void Format_UpdateWithMultipleColumns_IndentsEachAssignment()
    {
        // Arrange
        var input = "update products set price = 24.99, stock = 45, last_updated = '2025-01-01' where product_id = 101";

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("UPDATE", result);
        Assert.Contains("SET", result);
        Assert.Contains("price =", result);
        Assert.Contains("stock =", result);
        Assert.Contains("last_updated =", result);
        Assert.Contains("WHERE", result);

        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length >= 4, "Formatted UPDATE with multiple columns should have multiple lines");
    }

    [Fact]
    public void Format_DeleteStatement_FormatsCorrectly()
    {
        // Arrange
        var input = "delete from users where id = 1";

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("DELETE FROM", result);
        Assert.Contains("users", result);
        Assert.Contains("WHERE", result);
        Assert.Contains("id = 1", result);

        // Verify it's multi-line
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length >= 2, "Formatted DELETE should have multiple lines");
    }

    [Fact]
    public void Format_DeleteWithComplexWhere_FormatsCorrectly()
    {
        // Arrange
        var input = "delete from orders where status = 'cancelled' and order_date < '2020-01-01'";

        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("DELETE FROM", result);
        Assert.Contains("orders", result);
        Assert.Contains("WHERE", result);
        Assert.Contains("status =", result);
        Assert.Contains("'cancelled'", result);
        Assert.Contains("and", result);
        Assert.Contains("order_date <", result);

        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length >= 2, "Formatted DELETE should have multiple lines");
    }

    [Theory]
    [InlineData("insert into users (name) values ('John')")]
    [InlineData("INSERT INTO USERS (NAME) VALUES ('JOHN')")]
    [InlineData("InSeRt InTo UsErS (NaMe) VaLuEs ('JoHn')")]
    public void Format_InsertDifferentCasing_ProducesConsistentOutput(string input)
    {
        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("INSERT INTO", result);
        Assert.Contains("VALUES", result);
    }

    [Theory]
    [InlineData("update users set name = 'John' where id = 1")]
    [InlineData("UPDATE USERS SET NAME = 'JOHN' WHERE ID = 1")]
    [InlineData("UpDaTe UsErS sEt NaMe = 'JoHn' WhErE iD = 1")]
    public void Format_UpdateDifferentCasing_ProducesConsistentOutput(string input)
    {
        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("UPDATE", result);
        Assert.Contains("SET", result);
        Assert.Contains("WHERE", result);
    }

    [Theory]
    [InlineData("delete from users where id = 1")]
    [InlineData("DELETE FROM USERS WHERE ID = 1")]
    [InlineData("DeLeTe FrOm UsErS wHeRe Id = 1")]
    public void Format_DeleteDifferentCasing_ProducesConsistentOutput(string input)
    {
        // Act
        var result = _formatter.Format(input);

        // Assert
        Assert.Contains("DELETE FROM", result);
        Assert.Contains("WHERE", result);
    }
}
