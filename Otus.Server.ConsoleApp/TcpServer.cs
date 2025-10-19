using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Otus.Server.ConsoleApp;

public class TcpServer : IDisposable
{

    private Socket? listenerSocket = null;
    private int _port;

    public TcpServer(int port)
    {
        _port = port;
    }

    public void Dispose()
    {
        listenerSocket?.Dispose();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, _port);

            listenerSocket.Bind(localEndPoint);

            listenerSocket.Listen();

            while (true)
            {
                Socket socket = await listenerSocket.AcceptAsync(cancellationToken);
                _ = ProcessClientAsync(socket, cancellationToken);
            }
        }
        finally
        {
            listenerSocket?.Shutdown(SocketShutdown.Both);
            listenerSocket?.Close();
        }
    }
    private async Task ProcessClientAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        var arrayPool = ArrayPool<byte>.Shared;
        var buffer = arrayPool.Rent(1_024);

        try
        {
            while (true)
            {
                int recieveSize = await clientSocket.ReceiveAsync(buffer, cancellationToken);
                if (recieveSize == 0)
                {
                    break;
                }
                WriteData(buffer, recieveSize);
            }
        }
        finally
        {
            arrayPool.Return(buffer);
            clientSocket.Shutdown(SocketShutdown.Receive);
            clientSocket.Close();
            clientSocket.Dispose();
        }

    }

    private void WriteData(byte[] buffer, int size)
    {
        CommandParts<ReadOnlySpan<byte>> result =
            CommandParser.Parse(buffer.AsSpan().Slice(0, size));
        Console.WriteLine("------------------");
        string command = Encoding.UTF8.GetString(result.Command);
        Console.WriteLine($"Command {command}");
        string key = Encoding.UTF8.GetString(result.Key);
        Console.WriteLine($"Key {key}");
        string value = Encoding.UTF8.GetString(result.Value);
        Console.WriteLine($"Value {value}");
        Console.WriteLine("------------------");
    }
}