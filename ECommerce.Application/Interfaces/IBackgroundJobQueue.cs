namespace ECommerce.Application.Interfaces;

public interface IBackgroundJobQueue
{
    ValueTask QueueAsync(Func<CancellationToken, Task> workItem);

    ValueTask<Func<CancellationToken, Task>> DequeueAsync(
        CancellationToken cancellationToken);
}