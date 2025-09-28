using System.Text;
using Otus.Server.ConsoleApp;

string input = "COMMAND";

byte[] byteArray = Encoding.UTF8.GetBytes(input);

var result = CommandParser.Parse(byteArray);

Console.WriteLine($"command: {result.Command.Length}");
Console.WriteLine($"key: {result.Key.Length}");
Console.WriteLine($"value: {result.Value.Length}");
