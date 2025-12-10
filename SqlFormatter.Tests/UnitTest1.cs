namespace SqlFormatter.Tests;

public class BasicTests
{
    [Fact]
    public void Test_Setup_Works()
    {
        // Simple test to verify the test infrastructure is working
        var expected = 2;
        var actual = 1 + 1;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SqlFormatter_Assembly_Can_Be_Referenced()
    {
        // Verify we can reference types from the main assembly
        var assembly = typeof(Program).Assembly;
        Assert.NotNull(assembly);
        Assert.Contains("SqlFormatter", assembly.FullName);
    }
}
