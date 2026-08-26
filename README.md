# Pipeline

## SQL Server setup

The worker uses `Microsoft.Data.SqlClient`. Configure the `SqlServer` connection
string with .NET user-secrets so credentials are not committed to the repository:

```powershell
dotnet user-secrets set --project Pipeline.Worker "ConnectionStrings:SqlServer" "Server=localhost;Database=Pipeline;Integrated Security=True;TrustServerCertificate=True;"
```

For deployed environments, set the equivalent environment variable:

```text
ConnectionStrings__SqlServer=Server=...;Database=...;User ID=...;Password=...;
```

Database services should depend on `ISqlConnectionFactory` and open a connection
for each unit of work. Connections are pooled by the client and must be disposed:

```csharp
await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
await using var command = connection.CreateCommand();
command.CommandText = "SELECT 1";
var result = await command.ExecuteScalarAsync(cancellationToken);
```

## Data Model

```
DatasetOptions
    String Name
    String SourcePath
	String FilePattern
    String DestinationTable
	int StabilityCheckSeconds
    Bool enabled

DiscoveredFile
    string DatasetName
    string FilePath
    string FileName

FileJob
	long Id
	string DatasetName
    string FilePath
    string FileName
    string Status			-- "Pending" or "Processing" or "Complete" or "Failed"
    string DiscoveredAt
    DateTime? StartedAt
    DateTime? CompletedAt
    string? ErrorMessage

-- FUTURE:
DatasetColumnMappings
	String DatasetName
    String SourceColumn
    String DestinationColumn
    String DataType
    Bool Required

```

## Architecture

**Worker.cs**: Based on DatasetOptions, we check SourcePaths and collect a list of DiscoveredFiles. Then, for each DiscoveredFile, we check existing FileJobs / FileJobHistory records to prevent duplicate files. If DiscoveredFile is valid, we create a new FileJob for the file (insert record to FileJobQueue table). For each record in the FileJobQueue, spawn a new worker (respecting the Max_Workers configuration) and process the FileJobs concurrently. When each Worker finishes, insert a record into FileJobHistory with success/error message and metadata.
