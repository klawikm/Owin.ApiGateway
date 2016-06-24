CREATE TABLE [dbo].WebServiceLog
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [DateTime] DATETIME NOT NULL, 
    [RequestedUrl] VARCHAR(1000) NOT NULL, 
    [SoapAction] VARCHAR(200) NULL, 
    [RequestString] VARCHAR(MAX) NULL, 
    [RequestHeaders] VARCHAR(MAX) NULL, 
    [ResponseString] VARCHAR(MAX) NULL, 
    [ResponseHeaders] VARCHAR(MAX) NULL, 
    [IsFromCache] BIT NULL, 
    [ResponseTimeInMS] INT NOT NULL
)
