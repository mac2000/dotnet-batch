using System.Linq;
using Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests;

public class WebApplication : WebApplicationFactory<Program>
{
    private readonly IStorage _storage;
    private const string Environment = "Development";

    public WebApplication(IStorage storage)
    {
        _storage = storage;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(Environment);
        builder.ConfigureServices(services =>
        {
            // Not required, seems like DI uses last found item
            // var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStorage));
            // if (descriptor != null)
            // {
            //     services.Remove(descriptor);
            // }
            services.AddSingleton<IStorage>(_storage);
        });
        return base.CreateHost(builder);
    }
}