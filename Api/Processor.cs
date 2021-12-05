using System.Collections.Concurrent;

namespace Api;

/// <summary>
/// Process items in batch or timeout
/// <example>
/// <remarks>
/// How it works:
///   infinite loop dequeue
///       collect batch "5" rows or "10 sec" timeout or app closing
///       process batch
/// </remarks>
/// <code>
/// builder.Services.AddHostedService&lt;Processor&lt;string&gt;&gt;();
/// </code>
/// </example>
/// </summary>
public class Processor : BackgroundService
{
    private readonly ILogger<Processor> _logger;
    private readonly IStorage _storage;
    private static readonly ConcurrentQueue<Payload> Queue = new ConcurrentQueue<Payload>();

    public Processor(ILogger<Processor> logger, IStorage storage)
    {
        _logger = logger;
        _storage = storage;
        _logger.LogDebug("Constructing processor");
    }
    
    public static void Enqueue(Payload payload) => Queue.Enqueue(payload);

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
    {
        _logger.LogDebug("Starting");
        _logger.LogDebug("Creating batch for 5 items");
        var batch = new List<Payload>(5);
        _logger.LogDebug("Creating 10 seconds timer");
        var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        _logger.LogDebug("Starting infinite loop till app dies");
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug("Starting loop till batch size is 5 or timeout of 10 seconds occured");
            while (batch.Count < 5 && !cancel.IsCancellationRequested)
            {
                if (Queue.TryDequeue(out var payload))
                {
                    batch.Add(payload);
                    _logger.LogDebug("Dequeued item, batch size is {Count}", batch.Count);
                }
            }

            _logger.LogDebug("We are here because one of: batch of 5 items collected, timeout of 10 seconds occurred, application is closing. Batch: {Count}, Timer: {IsCancellationRequested}, App: {IsCancellationRequested}", batch.Count, cancel.IsCancellationRequested, stoppingToken.IsCancellationRequested);
            if (batch.Count > 0)
            {
                try
                {
                    _logger.LogDebug("Sending {Count} items from batch to storage save method", batch.Count);
                    await _storage.SaveAsync(batch);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing batch, try catch here needed, otherwise app will crash");
                }
            }

            _logger.LogDebug("Clear batch, restart timer, repeat");
            batch.Clear();
            cancel = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        }
    }, stoppingToken);
}