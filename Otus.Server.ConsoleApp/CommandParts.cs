namespace Otus.Server.ConsoleApp;

public readonly ref struct CommandParts
{
    public ReadOnlySpan<byte> Command { get;  init; }
    public ReadOnlySpan<byte> Key { get;  init; }
    public ReadOnlySpan<byte> Value { get;  init; }
    public void Deconstruct(
        out ReadOnlySpan<byte> command,
        out ReadOnlySpan<byte> key,
        out ReadOnlySpan<byte> value)
    {
        command = Command;
        key = Key;
        value = Value;
    }
}
