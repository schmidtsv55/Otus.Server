using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Otus.Server.ConsoleApp;

public class TcpServer : IDisposable
{
    public int Port { get; private set; }
    private Socket? listenerSocket = null;

    private SimpleStore _simpleStore;
    private int _maxRecieveSize;
    private SemaphoreSlim _semaphore;

    private static readonly ActivitySource TcpServerActivitySource = new ActivitySource(
        "Otus.Server.ConsoleApp");
    private static readonly Meter TcpServerMeter = new("Otus.Server.ConsoleApp", "1.0");
    private static readonly Counter<long> CommandCounter = TcpServerMeter.CreateCounter<long>("CommandCounter", description: "Count of processed commands");
    private static readonly Histogram<double> CommandHistogram = TcpServerMeter.CreateHistogram<double>("CommandHistogram", description: "Duration of processed command");
    private static long TotalCommnads = 0;
    public TcpServer(
        SimpleStore simpleStore,
        int port,
        int maxRecieveSize,
        int connectionCount)
    {
        _simpleStore = simpleStore;
        Port = port;
        _maxRecieveSize = maxRecieveSize;
        _semaphore = new SemaphoreSlim(connectionCount, connectionCount);
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

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Port);

            listenerSocket.Bind(localEndPoint);

            listenerSocket.Listen();

            while (true)
            {
                await _semaphore.WaitAsync();
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
        using Activity activity = TcpServerActivitySource.StartActivity("ProcessClient");
        ArrayPool<byte>? arrayPool = ArrayPool<byte>.Shared;
        byte[] buffer = arrayPool.Rent(_maxRecieveSize);

        try
        {
            while (true)
            {
                int recieveSize = await clientSocket.ReceiveAsync(buffer, cancellationToken);
                if (recieveSize > _maxRecieveSize)
                {
                    return;
                }
                if (recieveSize == 0)
                {
                    break;
                }
                var stopwatch = Stopwatch.StartNew();
                CommandParts<ReadOnlySpan<byte>> commandParts =
                    CommandParser.Parse(buffer.AsSpan().Slice(0, recieveSize));

                ArraySegment<byte> response = GetResponse(commandParts, activity);
                Interlocked.Increment(ref TotalCommnads);
                CommandCounter?.Add(TotalCommnads);
                await clientSocket.SendAsync(response, cancellationToken);
                stopwatch.Stop();
                CommandHistogram?.Record(stopwatch.Elapsed.TotalMilliseconds);

            }
        }
        finally
        {
            _semaphore.Release();
            arrayPool.Return(buffer, clearArray: true);
            clientSocket.Shutdown(SocketShutdown.Receive);
            clientSocket.Close();
            clientSocket.Dispose();
        }

    }

    private ArraySegment<byte> GetResponse(
        CommandParts<ReadOnlySpan<byte>> commandParts,
        Activity? activity)
    {
        if (commandParts.Key.Length == 0)
        {
            activity?.SetTag("command.status", TcpServerResponses.EmptyKeyTitle);
            return TcpServerResponses.EmptyKey;
        }

        string key = Encoding.UTF8.GetString(commandParts.Key);
        activity?.SetTag("command.key", key);

        if (commandParts.Command.SequenceEqual(SimpleStoreCommands.GetCommand))
        {
            activity?.SetTag("command.name", "GET");
            UserProfile? userProfile = _simpleStore.Get(key);
            if (userProfile == null)
            {
                activity?.SetTag("command.status", TcpServerResponses.NilTitle);
                return TcpServerResponses.Nil;
            }

            activity?.SetTag("command.status", TcpServerResponses.OkTitle);
            return JsonSerializer.SerializeToUtf8Bytes(userProfile);

        }
        else if (commandParts.Command.SequenceEqual(SimpleStoreCommands.SetCommand) && !commandParts.Value.IsEmpty)
        {
            activity?.SetTag("command.name", "SET");
            UserProfile? userProfile = JsonSerializer.Deserialize<UserProfile>(commandParts.Value);
            _simpleStore.Set(key, userProfile);
            activity?.SetTag("command.status", TcpServerResponses.OkTitle);
            return TcpServerResponses.Ok;
        }
        else if (commandParts.Command.SequenceEqual(SimpleStoreCommands.DeleteCommand))
        {
            activity?.SetTag("command.name", "DELETE");
            _simpleStore.Delete(key);
            activity?.SetTag("command.status", TcpServerResponses.OkTitle);
            return TcpServerResponses.Ok;
        }
        else
        {
            activity?.SetTag("command.key", Encoding.UTF8.GetString(commandParts.Command));
            activity?.SetTag("command.status", TcpServerResponses.UnknownCommandTitle);
            return TcpServerResponses.UnknownCommand;
        }
    }
}