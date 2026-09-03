using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.BackgroundJobs;

public class OrderBackgroundWorker : BackgroundService
{
    private readonly IBackgroundJobQueue _backgroundJobQueue;
    private readonly ILogger<OrderBackgroundWorker> _logger;

    public OrderBackgroundWorker(
        IBackgroundJobQueue backgroundJobQueue,
        ILogger<OrderBackgroundWorker> logger)
    {
        _backgroundJobQueue = backgroundJobQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Order background worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem =
                    await _backgroundJobQueue.DequeueAsync(
                        stoppingToken);

                await workItem(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while processing a background job.");
            }
        }

        _logger.LogInformation(
            "Order background worker stopped.");
    }
}