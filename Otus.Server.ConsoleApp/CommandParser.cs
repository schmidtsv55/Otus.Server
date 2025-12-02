namespace Otus.Server.ConsoleApp;

public static class CommandParser
{
    private static byte space = 32;
    private static byte empty = 0;

    public static CommandParts<ReadOnlySpan<byte>>
        Parse(ReadOnlySpan<byte> input)
    {
        int firstIndex = GetIndex(input);
        if (firstIndex == -1)
        {
            return new CommandParts<ReadOnlySpan<byte>>
            {
                Command = ReadOnlySpan<byte>.Empty,
                Key = ReadOnlySpan<byte>.Empty,
                Value = ReadOnlySpan<byte>.Empty
            };
        }
        int secondIndex = GetIndex(input[(firstIndex + 1)..]);
        if (secondIndex == -1)
        {
            return new CommandParts<ReadOnlySpan<byte>>
            {
                Command = input[..firstIndex],
                Key = input[(firstIndex + 1)..],
                Value = ReadOnlySpan<byte>.Empty
            };
        }

        return new CommandParts<ReadOnlySpan<byte>>
        {
            Command = input[..firstIndex],
            Key = input.Slice(firstIndex + 1, secondIndex),
            Value = input[(firstIndex + secondIndex + 2)..].Trim(empty)
        };

    }
    private static int GetIndex(ReadOnlySpan<byte> input)
    {
        int index = input.IndexOf(space);

        if (index == -1 || input.Length == index + 1)
        {
            return -1;
        }
        return index;
    }
}
