CREATE TABLE [dbo].[WeatherUnits] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [LocationId]    INT           NULL,
    [Time]          NVARCHAR (50) NULL,
    [Interval]      NVARCHAR (50) NULL,
    [Temperature]   NVARCHAR (50) NULL,
    [Windspeed]     NVARCHAR (50) NULL,
    [Winddirection] NVARCHAR (50) NULL,
    [IsDay]         NVARCHAR (50) NULL,
    [Weathercode]   NVARCHAR (50) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([LocationId]) REFERENCES [dbo].[Location] ([Id])
);

