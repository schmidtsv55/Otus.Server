using System;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;

namespace Otus.Server.ConsoleApp.Tests;

public class SimpleStore_GetSetResult
{
    [Fact]
    public async Task GetSet_Result()
    {
        using SimpleStore store = new SimpleStore();
        string key = "key";
        UserProfile firstValue = new()
        {
            Id = 1,
            Username = "Some User 1",
            CreatedAt = DateTime.Parse("2025-12-01")
        };
        store.Set(key, firstValue);
        UserProfile lastValue = new()
        {
            Id = 2,
            Username = "Some User 2",
            CreatedAt = DateTime.Parse("2025-12-02")
        };
        var value = JsonSerializer.SerializeToUtf8Bytes(firstValue);
        var valueObj = JsonSerializer.Deserialize<UserProfile>(value);
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < 1000; i++)
        {
            tasks.Add(Task.Run(() => store.Get(key)));
            tasks.Add(Task.Run(() => store.Set(key, lastValue)));
        }
        await Task.WhenAll(tasks);

        (long setCount, long getCount, long deleteCount) = store.GetStatistics();
        UserProfile? actualValue = store.Get(key);

        Assert.Equal(1001, setCount);
        Assert.Equal(1000, getCount);
        Assert.Equal(lastValue, actualValue);
    }
}
