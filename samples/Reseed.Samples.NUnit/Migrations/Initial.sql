CREATE TABLE [dbo].[User] (
	Id int IDENTITY(1, 1) NOT NULL,
	FirstName nvarchar(100) NOT NULL,
	LastName nvarchar(100) NOT NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC)
)