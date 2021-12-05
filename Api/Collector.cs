using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Api;

[ApiController]
public class Collector
{
    private readonly ILogger<Collector> _logger;

    public Collector(ILogger<Collector> logger)
    {
        _logger = logger;
    }

    [HttpGet("/collect")]
    public void Collect([Required][FromQuery]string id)
    {
        _logger.LogDebug("Enqueuing {id}", id);
        Processor.Enqueue(new Payload { Id = id });
    }
}