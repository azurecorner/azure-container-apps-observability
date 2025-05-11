CREATE TABLE [dbo].[Location] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [Department]           NVARCHAR (100) NULL,
    [DepartmentCode]       INT            NULL,
    [City]                 NVARCHAR (100) NULL,
    [PostalCode]           INT            NULL,
    [Latitude]             FLOAT (53)     NULL,
    [Longitude]            FLOAT (53)     NULL,
    [GenerationTimeMs]     FLOAT (53)     NULL,
    [UtcOffsetSeconds]     INT            NULL,
    [Timezone]             NVARCHAR (50)  NULL,
    [TimezoneAbbreviation] NVARCHAR (10)  NULL,
    [Elevation]            FLOAT (53)     NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

