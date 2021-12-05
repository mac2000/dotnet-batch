namespace Api;

public interface IStorage
{
    Task SaveAsync(IEnumerable<Payload> records);
}

public class Storage: IStorage
{
    private readonly ILogger<Storage> _logger;

    public Storage(ILogger<Storage> logger)
    {
        _logger = logger;
        _logger.LogDebug("Constructing demo storage");
    }

    public Task SaveAsync(IEnumerable<Payload> records)
    {
        _logger.LogDebug("Emulating saving of {Count} records into storage", records.Count());
        Thread.Sleep(1000);
        return Task.CompletedTask;
    }
}