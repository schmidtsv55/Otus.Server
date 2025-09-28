using System.Text;

namespace Otus.Server.ConsoleApp.Tests;

public class CommandParser_ParseResult
{
    [Theory]
    [InlineData("SET user:1 Admin User", "SET")]
    [InlineData("GET user:1", "GET")]
    [InlineData("GET   user:1", "GET")]
    [InlineData("GET ", "")]
    [InlineData("DEL", "")]
    public void Parse_Command_Equal(string data, string command)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(data);

        var result = CommandParser.Parse(byteArray);
        string actual = Encoding.UTF8.GetString(result.Command);
        Assert.Equal(command, actual);
    }

    [Theory]
    [InlineData("SET user:1 Admin User", "user:1")]
    [InlineData("GET user:1", "user:1")]
    [InlineData("GET   user:1", "")]
    [InlineData("GET ", "")]
    [InlineData("DEL", "")]
    public void Parse_Key_Equal(string data, string key)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(data);

        var result = CommandParser.Parse(byteArray);
        string actual = Encoding.UTF8.GetString(result.Key);
        Assert.Equal(key, actual);
    }

    [Theory]
    [InlineData("SET user:1 Admin User", "Admin User")]
    [InlineData("SET user:1  Admin User", " Admin User")]
    [InlineData("GET user:1", "")]
    [InlineData("GET   user:1 ", " user:1 ")]
    [InlineData("GET ", "")]
    [InlineData("DEL", "")]
    public void Parse_Value_Equal(string data, string value)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(data);

        var result = CommandParser.Parse(byteArray);
        string actual = Encoding.UTF8.GetString(result.Value);
        Assert.Equal(value, actual);
    }
}
