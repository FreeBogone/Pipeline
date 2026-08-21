using Pipeline.Worker;
using Pipeline.Worker.Configuration;
using Pipeline.Worker.Discovery;

var builder = Host.CreateApplicationBuilder(args);

// Get dataset configurations
builder.Services.Configure<List<DatasetOptions>>(
    builder.Configuration.GetSection("Datasets"));

// register services
builder.Services.AddSingleton<FileStabilityChecker>();
builder.Services.AddSingleton<FileDiscoveryService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
