drop table if exists FileJob;

CREATE TABLE FileJob
(
	Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FileJob PRIMARY KEY,
	DatasetName nvarchar(200) NOT NULL,
	FilePath nvarchar(2048) NOT NULL,
	FileName nvarchar(200),
	Status nvarchar(50),
	DiscoveredAt DATETIME2(7) CONSTRAINT DF_FileJob_DiscoveredAt DEFAULT SYSUTCDATETIME(),
	StartedAt DATETIME2(7),
	CompletedAt DATETIME2(7),
	ErrorMessage nvarchar(2048)
);



select * from FileJob;


delete from FileJob;