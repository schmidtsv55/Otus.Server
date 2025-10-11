using Otus.Server.ConsoleApp;

int port = 8080;
System.Console.WriteLine($"Server start on port {port}");
var cts = new CancellationTokenSource();
CancellationToken cancellationToken = cts.Token;
using var server = new TcpServer(port);
_ = server.StartAsync(cancellationToken);
Console.ReadLine();
cts.Cancel();