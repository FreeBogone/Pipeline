using Pipeline.Worker;
using Pipeline.Worker.Configuration;
using Pipeline.Worker.Data;
using Pipeline.Worker.Discovery;
using Pipeline.Worker.Jobs;
using Pipeline.Worker.Processing;

var builder = Host.CreateApplicationBuilder(args);

// Get processing configurations
builder.Services.Configure<ProcessingOptions>(
    builder.Configuration.GetSection("Processing"));

// register services
builder.Services.AddSqlServer(builder.Configuration);
builder.Services.AddSingleton<ConfigurationRepository>();
builder.Services.AddSingleton<FileStabilityChecker>();
builder.Services.AddSingleton<FileDiscoveryService>();
builder.Services.AddSingleton<FileDataReaderFactory>();
builder.Services.AddSingleton<BulkDataWriter>();
builder.Services.AddSingleton<FileProcessingService>();
builder.Services.AddSingleton<FileJobRepository>();
builder.Services.AddSingleton<FileJobService>();

builder.Services.AddHostedService<FileDiscoveryWorker>();
builder.Services.AddHostedService<FileProcessingWorker>();

var host = builder.Build();
host.Run();
