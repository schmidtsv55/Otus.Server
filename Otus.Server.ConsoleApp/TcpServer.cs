using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Otus.Server.ConsoleApp;

public class TcpServer : IDisposable
{

    private Socket? listenerSocket = null;
    private int _port;
    private SimpleStore _simpleStore;

    public TcpServer(SimpleStore simpleStore, int port)
    {
        _simpleStore = simpleStore;
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
        ArrayPool<byte>? arrayPool = ArrayPool<byte>.Shared;
        byte[] buffer = arrayPool.Rent(1_024);

        try
        {
            while (true)
            {
                int recieveSize = await clientSocket.ReceiveAsync(buffer, cancellationToken);
                if (recieveSize == 0)
                {
                    break;
                }
                CommandParts<ReadOnlySpan<byte>> commandParts =
                    CommandParser.Parse(buffer.AsSpan().Slice(0, recieveSize));

                ArraySegment<byte> response = GetResponse(commandParts);
                await clientSocket.SendAsync(response, cancellationToken);
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

    private ArraySegment<byte> GetResponse(CommandParts<ReadOnlySpan<byte>> commandParts)
    {
        if (commandParts.Key.Length == 0)
        {
            return TcpServerResponses.EmptyKey;
        }
        else if (commandParts.Command.SequenceEqual(SimpleStoreCommands.GetCommand))
        {
            byte[]? value = _simpleStore.Get(Encoding.UTF8.GetString(commandParts.Key));
            return value == null || value.Length == 0 ? TcpServerResponses.Nil : value;
        }
        else if (commandParts.Command.SequenceEqual(SimpleStoreCommands.SetCommand))
        {
            _simpleStore.Set(Encoding.UTF8.GetString(commandParts.Key), commandParts.Value.ToArray());
            return TcpServerResponses.Ok;
        }
        else if (commandParts.Command.SequenceEqual(SimpleStoreCommands.DeleteCommand))
        {
            _simpleStore.Delete(Encoding.UTF8.GetString(commandParts.Key));
            return TcpServerResponses.Ok;
        }
        else
        {
            return TcpServerResponses.UnknownCommand;
        }
    }
}