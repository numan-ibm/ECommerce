using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.BackgroundJobs;

public class OrderBackgroundProcessor : IOrderBackgroundProcessor
{
    private readonly ILogger<OrderBackgroundProcessor> _logger;

    public OrderBackgroundProcessor(
        ILogger<OrderBackgroundProcessor> logger)
    {
        _logger = logger;
    }

    public Task ProcessOrderAsync(
        int orderId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Background processing completed for order {OrderId}.",
            orderId);

        return Task.CompletedTask;
    }
}