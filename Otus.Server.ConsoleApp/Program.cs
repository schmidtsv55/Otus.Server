using Otus.Server.ConsoleApp;

int port = 8080;
System.Console.WriteLine($"Server start on port {port}");
var cts = new CancellationTokenSource();
CancellationToken cancellationToken = cts.Token;
using var simpleStore = new SimpleStore();
using var server = new TcpServer(simpleStore, port);
_ = server.StartAsync(cancellationToken);
Console.ReadLine();
cts.Cancel();