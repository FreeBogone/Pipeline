CREATE TABLE Dataset
(
	Id BIGINT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Dataset PRIMARY KEY,
	DatasetKey nvarchar(50) NOT NULL,
	DisplayName nvarchar(50) NOT NULL,
	SourcePath nvarchar(2048) NOT NULL,
	FilePattern nvarchar(50) NOT NULL,
	DestinationTableName nvarchar(100) NOT NULL,
	StabilityCheckSeconds int NOT NULL,
	IsEnabled bit NOT NULL DEFAULT 1,
	CreatedAt DATETIME2 CONSTRAINT DF_Dataset_CreatedAt DEFAULT SYSUTCDATETIME(),
	UpdatedAt DATETIME2
)

insert into Dataset (
	DatasetKey, 
	DisplayName, 
	SourcePath, 
	FilePattern, 
	DestinationTableName,
	StabilityCheckSeconds
) VALUES (
	'mock-data',
	'Mock Data',
	'C:\\Temp\\TestData\\mock_data',
	'*.csv',
	'MockData',
	0
);

CREATE TABLE DatasetColumnMapping
(
	Id BIGINT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_DatasetColMap PRIMARY KEY,
	DatasetId BIGINT FOREIGN KEY REFERENCES Dataset(Id),
	SourceColumnIndex int NOT NULL,
	DestinationColumnName nvarchar(100) NOT NULL,
	TargetType nvarchar(50),
	IsRequired bit NOT NULL DEFAULT 1
)

CREATE TABLE MockData
(
	FirstName nvarchar(50),
	LastName nvarchar(50),
	Email nvarchar(50)
);

insert into DatasetColumnMapping
(
	DatasetId,
	SourceColumnIndex,
	DestinationColumnName,
	TargetType,
	IsRequired
) Values (
	2,
	1,
	'FirstName',
	'string',
	1
);

insert into DatasetColumnMapping
(
	DatasetId,
	SourceColumnIndex,
	DestinationColumnName,
	TargetType,
	IsRequired
) Values (
	2,
	2,
	'LastName',
	'string',
	1
);

insert into DatasetColumnMapping
(
	DatasetId,
	SourceColumnIndex,
	DestinationColumnName,
	TargetType,
	IsRequired
) Values (
	2,
	3,
	'Email',
	'string',
	1
);

CREATE TABLE FileJob
(
	Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FileJob PRIMARY KEY,
	DatasetId BIGINT FOREIGN KEY REFERENCES Dataset(Id),
	FilePath nvarchar(2048) NOT NULL,
	Status nvarchar(50),
	AttemptCount int NOT NULL DEFAULT 0,
	DiscoveredAt DATETIME2(7) CONSTRAINT DF_FileJob_DiscoveredAt DEFAULT SYSUTCDATETIME(),
	StartedAt DATETIME2(7),
	CompletedAt DATETIME2(7),
	ErrorMessage nvarchar(2048)
);