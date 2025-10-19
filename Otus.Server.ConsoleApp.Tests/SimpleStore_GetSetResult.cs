using System;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace Otus.Server.ConsoleApp.Tests;

public class SimpleStore_GetSetResult
{
    [Fact]
    public async Task GetSet_Result()
    {
        using SimpleStore store = new SimpleStore();
        string key = "key";

        store.Set(key, Encoding.UTF8.GetBytes("firstValue"));
        byte[] lastValue = Encoding.UTF8.GetBytes("lastValue");;
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < 1000; i++)
        {
            tasks.Add(Task.Run(() => store.Get(key)));
            tasks.Add(Task.Run(() => store.Set(key, lastValue)));
        }
        await Task.WhenAll(tasks);

        (long setCount, long getCount, long deleteCount) = store.GetStatistics();
        byte[]? actualValue =  store.Get(key);

        Assert.Equal(1001, setCount);
        Assert.Equal(1000, getCount);
        Assert.Equal(lastValue, actualValue);
    }
}
