using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Otus.NBomber.ConsoleApp;


public class Client : IDisposable
{
    public static byte[] GetCommand = Encoding.UTF8.GetBytes("GET");
    public static byte[] SetCommand = Encoding.UTF8.GetBytes("SET");
    public static byte[] Space = Encoding.UTF8.GetBytes(" ");
    private Socket? _socket;
    public  async Task ConnectAsync()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 8080);

        await _socket.ConnectAsync(localEndPoint);
    }
    public async Task<string> SetAsync(string key, byte[] value)
    {
        await SentAsync(SetCommand, Encoding.UTF8.GetBytes(key), value);
        return await ReceiveAsync();
    }
    async Task SentAsync(params byte[][] args)
    {
        if (_socket == null)
        {
            throw new NullReferenceException("Not connect");
        }
        int size = args.Sum(x => x.Length) + args.Length - 1;
        ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;

        byte[] buffer = arrayPool.Rent(size);
        int cursor = 0;
        try
        {
            Array.Copy(args[0], 0, buffer, 0, args[0].Length);
            cursor = args[0].Length;
            Array.Copy(Space, 0, buffer, cursor, Space.Length);
            cursor += Space.Length;
            for (int i = 1; i < args.Length; i++)
            {
                Array.Copy(args[i], 0, buffer, cursor, args[i].Length);
                cursor += args[i].Length;
                if (i == args.Length - 1)
                {
                    break;
                }
                Array.Copy(Space, 0, buffer, cursor, Space.Length);
                cursor += Space.Length;
            }

            await _socket.SendAsync(buffer);

        }
        finally
        {
            arrayPool.Return(buffer);
        }

    }
    async Task<string> ReceiveAsync()
    {
        if (_socket == null)
        {
            throw new NullReferenceException("Not connect");
        }
        ArrayPool<byte>? arrayPool = ArrayPool<byte>.Shared;
        byte[] buffer = arrayPool.Rent(1_024);
        try
        {
            int bytesRead = await _socket.ReceiveAsync(buffer);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
        finally
        {
            arrayPool.Return(buffer);

        }

    }
    public async Task<string> GetAsync(string key)
    {
        await SentAsync(GetCommand, Encoding.UTF8.GetBytes(key));
        return await ReceiveAsync();
    }

    public void Dispose()
    {
        if (_socket == null)
        {
            return;
        }
        _socket.Shutdown(SocketShutdown.Send);
        _socket.Close();
        _socket.Dispose();
    }
}

