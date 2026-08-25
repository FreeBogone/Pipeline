# Pipeline

## Schema
```sql
```
Dataset
  String Name 
  String SourcePath
  String FilePattern

Job
  String Dataset
  String SourcePath
  String Status       // Pending, Running, Completed, Failed

JobHistory
  String Dataset
  String SourcePath
  Datetime CompletedDateTime
  String Result       // Success, Failure
```
```

