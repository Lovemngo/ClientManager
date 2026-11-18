IF DB_ID(N'ClientManagerDb') IS NULL
    CREATE DATABASE ClientManagerDb;
GO
USE ClientManagerDb;
GO

CREATE TABLE dbo.Gender (
    GenderId INT          NOT NULL CONSTRAINT PK_Gender PRIMARY KEY,
    Name     NVARCHAR(30) NOT NULL CONSTRAINT UQ_Gender_Name UNIQUE
);

CREATE TABLE dbo.AddressType (
    AddressTypeId INT          NOT NULL CONSTRAINT PK_AddressType PRIMARY KEY,
    Name          NVARCHAR(50) NOT NULL CONSTRAINT UQ_AddressType_Name UNIQUE
);

CREATE TABLE dbo.ContactType (
    ContactTypeId INT          NOT NULL CONSTRAINT PK_ContactType PRIMARY KEY,
    Name          NVARCHAR(50) NOT NULL CONSTRAINT UQ_ContactType_Name UNIQUE
);

CREATE TABLE dbo.Client (
    ClientId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Client PRIMARY KEY,
    FirstName   NVARCHAR(100) NOT NULL,
    LastName    NVARCHAR(100) NOT NULL,
    GenderId    INT           NOT NULL CONSTRAINT FK_Client_Gender REFERENCES dbo.Gender(GenderId),
    DateOfBirth DATE          NULL,
    IdNumber    NVARCHAR(20)  NULL,
    CreatedOn   DATETIME2(0)  NOT NULL CONSTRAINT DF_Client_CreatedOn  DEFAULT SYSUTCDATETIME(),
    ModifiedOn  DATETIME2(0)  NOT NULL CONSTRAINT DF_Client_ModifiedOn DEFAULT SYSUTCDATETIME()
);

CREATE INDEX IX_Client_Name ON dbo.Client (LastName, FirstName) INCLUDE (GenderId, DateOfBirth, IdNumber);
CREATE UNIQUE INDEX UX_Client_IdNumber ON dbo.Client (IdNumber) WHERE IdNumber IS NOT NULL;

CREATE TABLE dbo.ClientAddress (
    AddressId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ClientAddress PRIMARY KEY,
    ClientId      INT           NOT NULL CONSTRAINT FK_ClientAddress_Client
                                  REFERENCES dbo.Client(ClientId) ON DELETE CASCADE,
    AddressTypeId INT           NOT NULL CONSTRAINT FK_ClientAddress_Type
                                  REFERENCES dbo.AddressType(AddressTypeId),
    Line1         NVARCHAR(150) NOT NULL,
    Line2         NVARCHAR(150) NULL,
    Suburb        NVARCHAR(100) NULL,
    City          NVARCHAR(100) NULL,
    PostalCode    NVARCHAR(20)  NULL,
    Country       NVARCHAR(100) NULL
);
CREATE INDEX IX_ClientAddress_ClientId ON dbo.ClientAddress (ClientId);

CREATE TABLE dbo.ClientContact (
    ContactId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ClientContact PRIMARY KEY,
    ClientId      INT           NOT NULL CONSTRAINT FK_ClientContact_Client
                                  REFERENCES dbo.Client(ClientId) ON DELETE CASCADE,
    ContactTypeId INT           NOT NULL CONSTRAINT FK_ClientContact_Type
                                  REFERENCES dbo.ContactType(ContactTypeId),
    Value         NVARCHAR(150) NOT NULL
);
CREATE INDEX IX_ClientContact_ClientId ON dbo.ClientContact (ClientId);
GO

INSERT INTO dbo.Gender (GenderId, Name) VALUES
    (1, N'Male'), (2, N'Female'), (3, N'Other'), (4, N'Unspecified');

INSERT INTO dbo.AddressType (AddressTypeId, Name) VALUES
    (1, N'Residential Address'), (2, N'Work Address'), (3, N'Postal Address'), (4, N'Billing Address');

INSERT INTO dbo.ContactType (ContactTypeId, Name) VALUES
    (1, N'Cell Number'), (2, N'Work Number'), (3, N'Home Number'), (4, N'Fax Number'), (5, N'Email Address');
GO
