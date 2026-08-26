using Pipeline.Worker;
using Pipeline.Worker.Configuration;
using Pipeline.Worker.Data;
using Pipeline.Worker.Discovery;
using Pipeline.Worker.Jobs;

var builder = Host.CreateApplicationBuilder(args);

// Get dataset configurations
builder.Services.Configure<List<DatasetOptions>>(
    builder.Configuration.GetSection("Datasets"));

// register services
builder.Services.AddSqlServer(builder.Configuration);
builder.Services.AddSingleton<FileStabilityChecker>();
builder.Services.AddSingleton<FileDiscoveryService>();
builder.Services.AddSingleton<FileJobRepository>();
builder.Services.AddSingleton<FileJobService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
