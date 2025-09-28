using System;

namespace Otus.Server.ConsoleApp;

public class SimpleStore
{
    private Dictionary<string, byte[]?> _dsta = new();

    public void Set(string key, byte[] value)
    {
        _dsta[key] = value;
    }
    public byte[]? Get(string key)
    {
        _dsta.TryGetValue(key, out var value);
        return value;
    }
    public void Delete(string key)
    {
        _dsta.Remove(key); 
    }
}
