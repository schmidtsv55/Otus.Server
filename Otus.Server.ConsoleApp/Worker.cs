using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Otus.Server.ConsoleApp;

public class Worker : BackgroundService
{
    private ILogger<Worker> _logger;
    private TcpServer _tcpServer;
    public Worker(ILogger <Worker> logger, TcpServer tcpServer)
    {
        _logger = logger;
        _tcpServer = tcpServer;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _tcpServer.StartAsync(stoppingToken);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Server start on port {_tcpServer.Port}");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Server stop");
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _tcpServer.Dispose();
        base.Dispose();
    }
}