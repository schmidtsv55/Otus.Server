using System;
using System.Text.Json;

namespace Otus.Server.ConsoleApp;

public class SimpleStore : IDisposable
{
    private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    private long _setCount, _getCount, _deleteCount;
    private Dictionary<string, byte[]?> _dsta = new();

    public void Set(string key, UserProfile? profile)
    {
        try
        {
            _lock.EnterWriteLock();
            Interlocked.Increment(ref _setCount);
            _dsta[key] = profile == null ? null : JsonSerializer.SerializeToUtf8Bytes(profile);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

    }
    public UserProfile? Get(string key)
    {
        try
        {
            _lock.EnterReadLock();
            Interlocked.Increment(ref _getCount);
            if(_dsta.TryGetValue(key, out var value))
            {
                return JsonSerializer.Deserialize<UserProfile>(value);
            }
            return null;
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
