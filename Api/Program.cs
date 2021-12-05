using Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IStorage, Storage>();
builder.Services.AddHostedService<Processor>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }