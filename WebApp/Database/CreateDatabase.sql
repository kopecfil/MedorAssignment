/* Create DB (idempotent) */
IF DB_ID(N'ExchangeRates') IS NULL
BEGIN
    CREATE DATABASE ExchangeRates;
END
GO

USE ExchangeRates;
GO

IF OBJECT_ID(N'dbo.ExchangeRateEntry', N'U') IS NULL
BEGIN
CREATE TABLE dbo.ExchangeRateEntry
(
    ExchangeRateEntryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExchangeRateEntry PRIMARY KEY,
    TimestampUtc        DATETIME2(0)      NOT NULL,  -- from provider (UTC, seconds precision)
    PriceCzk            DECIMAL(18,8)     NOT NULL,
    BestBidCzk          DECIMAL(18,8)     NOT NULL,
    BestAskCzk          DECIMAL(18,8)     NOT NULL,
    Market              NVARCHAR(32)      NOT NULL,  -- e.g. 'coinbase'
    Instrument          NVARCHAR(32)      NOT NULL,  -- e.g. 'BTC-EUR'
    UserNote            NVARCHAR(400)     NULL,      -- edited on Saved page
    CreatedAtUtc        DATETIME2(0)      NOT NULL CONSTRAINT DF_ExchangeRateEntry_Created DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc        DATETIME2(0)      NOT NULL CONSTRAINT DF_ExchangeRateEntry_Updated DEFAULT (SYSUTCDATETIME())
);

END
GO
