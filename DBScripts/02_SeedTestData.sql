USE ClientManagerDb;
GO

;WITH n AS (
    SELECT TOP (100000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS i
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO dbo.Client (FirstName, LastName, GenderId, DateOfBirth, IdNumber)
SELECT N'First' + CAST(i AS NVARCHAR(10)),
       N'Last'  + CAST(i % 5000 AS NVARCHAR(10)),
       (i % 3) + 1,
       DATEADD(DAY, -(7000 + (i % 20000)), CAST(GETDATE() AS DATE)),
       N'TEST' + RIGHT(N'000000' + CAST(i AS NVARCHAR(10)), 8)
FROM n;

INSERT INTO dbo.ClientAddress (ClientId, AddressTypeId, Line1, Suburb, City, PostalCode, Country)
SELECT ClientId, 1, CAST(ClientId AS NVARCHAR(10)) + N' Main Road', N'Suburb', N'Johannesburg', N'2000', N'South Africa'
FROM dbo.Client WHERE IdNumber LIKE N'TEST%';

INSERT INTO dbo.ClientContact (ClientId, ContactTypeId, Value)
SELECT ClientId, 1, N'082' + RIGHT(N'0000000' + CAST(ClientId AS NVARCHAR(10)), 7)
FROM dbo.Client WHERE IdNumber LIKE N'TEST%';
GO
