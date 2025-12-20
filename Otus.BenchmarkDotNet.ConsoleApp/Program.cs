// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Otus.Server.ConsoleApp;

BenchmarkRunner.Run<SerializeJTest>();

[MemoryDiagnoser]
public class SerializeJTest
{
    [Benchmark]
    public byte[] SerializeJson()
    {
        UserProfile userProfile = new()
        {
            Id = 1,
            CreatedAt = DateTime.Parse("2025-12-31"),
            Username = "Name"
        };
        return JsonSerializer.SerializeToUtf8Bytes(userProfile);
    }
    [Benchmark]
    public byte[] SerializeBinary()
    {
        UserProfile userProfile = new()
        {
            Id = 1,
            CreatedAt = DateTime.Parse("2025-12-31"),
            Username = "Name"
        };
        using (MemoryStream ms = new MemoryStream())
        {
            userProfile.SerializeToBinary(ms);
            return ms.ToArray();
        }
    }
}