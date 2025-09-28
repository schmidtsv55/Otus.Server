namespace Otus.Server.ConsoleApp;

public readonly ref struct CommandParts<T> where T: allows ref struct
{
    public T Command { get;  init; }
    public T Key { get;  init; }
    public T Value { get;  init; }
    public void Deconstruct(
        out T command,
        out T key,
        out T value)
    {
        command = Command;
        key = Key;
        value = Value;
    }
}
