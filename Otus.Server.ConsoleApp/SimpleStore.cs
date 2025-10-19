using System;

namespace Otus.Server.ConsoleApp;

public class SimpleStore : IDisposable
{
    private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    private long _setCount, _getCount, _deleteCount;
    private Dictionary<string, byte[]?> _dsta = new();

    public void Set(string key, byte[] value)
    {
        try
        {
            _lock.EnterWriteLock();
            Interlocked.Increment(ref _setCount);
            _dsta[key] = value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }

    }
    public byte[]? Get(string key)
    {
        try
        {
            _lock.EnterReadLock();
            Interlocked.Increment(ref _getCount);
            _dsta.TryGetValue(key, out var value);
            return value;
        }
        finally
        {
            _lock.ExitReadLock();
        }

    }
    public void Delete(string key)
    {
        try
        {
            _lock.EnterWriteLock();
             Interlocked.Increment(ref _deleteCount);
            _dsta.Remove(key);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

    }
    public (long setCount, long getCount, long deleteCount) GetStatistics()
    {
        return (_setCount, _getCount, _deleteCount);
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}
