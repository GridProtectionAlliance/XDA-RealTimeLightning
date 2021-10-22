CREATE TABLE RTLightningStrike
(
    UALFVersion TINYINT NOT NULL,
    StrikeTime DATETIME2 NOT NULL,
    Nanoseconds TINYINT NOT NULL,
    Latitude DECIMAL(6, 4),
    Longitude DECIMAL(7, 4),
    PeakCurrent SMALLINT NOT NULL,
    FlashMultiplicity TINYINT NOT NULL,
    ParticipatingSensors TINYINT NOT NULL,
    DegreesOfFreedom TINYINT NOT NULL,
    EllipseAngle FLOAT NOT NULL,
    SemiMajorAxisLength FLOAT NOT NULL,
    SemiMinorAxisLength FLOAT NOT NULL,
    ChiSquared FLOAT NOT NULL,
    Risetime FLOAT NOT NULL,
    PeakToZeroTime FLOAT NOT NULL,
    MaximumRateOfRise FLOAT NOT NULL,
    CloudIndicator BIT NOT NULL,
    AngleIndicator BIT NOT NULL,
    SignalIndicator BIT NOT NULL,
    TimingIndicator BIT NOT NULL,
    CONSTRAINT PK_LightningStrike PRIMARY KEY
    (
        StrikeTime,
        Nanoseconds,
        Latitude,
        Longitude
    )
)
GO