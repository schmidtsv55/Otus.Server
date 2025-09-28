namespace Otus.Server.ConsoleApp;

public static class CommandParser
{
    private static byte bit2 = 32;

    public static CommandParts
        Parse(ReadOnlySpan<byte> input)
    {
        ReadOnlySpan<byte> command = ReadOnlySpan<byte>.Empty;
        ReadOnlySpan<byte> key = ReadOnlySpan<byte>.Empty;
        ReadOnlySpan<byte> value = ReadOnlySpan<byte>.Empty;

        foreach (Range segment in input.Split(bit2))
        {
            if (command.IsEmpty)
            {
                if (segment.End.Value + 1 >= input.Length)
                {
                    break;
                }
                command = input[segment];
                continue;
            }
            key = input[segment];

            int valueIndex = segment.End.Value + 1;
            if (valueIndex < input.Length)
            {
                value = input.Slice(valueIndex);
            }
            break;
        }
        return new CommandParts
        {
            Command = command,
            Key = key,
            Value = value
        };
    }
}
