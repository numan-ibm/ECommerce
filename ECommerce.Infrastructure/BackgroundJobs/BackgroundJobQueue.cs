using System.Threading.Channels;
using ECommerce.Application.Interfaces;

namespace ECommerce.Infrastructure.BackgroundJobs;

public class BackgroundJobQueue : IBackgroundJobQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue;

    public BackgroundJobQueue()
    {
        _queue =
            Channel.CreateUnbounded<Func<CancellationToken, Task>>();
    }

    public async ValueTask QueueAsync(
        Func<CancellationToken, Task> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        await _queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<Func<CancellationToken, Task>> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}