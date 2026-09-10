/* ============================================================================
   06-aws-deployment-schema.sql
   Community Sports Facilities Booking System — AWS Deployment Schema
   Target: Microsoft SQL Server (Amazon RDS for SQL Server)

   This is the REAL schema the running ASP.NET Core application uses (it maps
   1:1 onto ApplicationDbContext and the EF Core migration
   Migrations/20260902174612_InitialCreate.cs). Run this manually, once,
   against your AWS RDS SQL Server database before starting the application.

   The application no longer calls Database.Migrate() or seeds data at
   startup (see Program.cs) — it only ever reads/writes tables that already
   exist, so this script is the single source of truth for schema creation.

   Do NOT confuse this with 01-create-database.sql / 02-create-tables.sql /
   03-seed-data.sql in this same folder — those create a separate, simplified
   "CommunitySportsBookingSystemSql" database kept purely as standalone SQL
   coursework evidence (see docs/database-implementation.md) and are unrelated
   to what the deployed application actually connects to.

   Usage:
     1. Create the target database on your RDS instance (if it does not
        already exist) and connect to it, e.g.:
           CREATE DATABASE CommunitySportsBookingSystem;
           GO
           USE CommunitySportsBookingSystem;
           GO
     2. Run this entire script against that database.
   ============================================================================ */

SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* ---------------------------------------------------------------------------
   1. Tables
   ------------------------------------------------------------------------- */

IF OBJECT_ID('dbo.AspNetRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetRoles
    (
        Id               NVARCHAR(450)  NOT NULL,
        Name             NVARCHAR(256)  NULL,
        NormalizedName   NVARCHAR(256)  NULL,
        ConcurrencyStamp NVARCHAR(MAX)  NULL,
        CONSTRAINT PK_AspNetRoles PRIMARY KEY (Id)
    );
END
GO

IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUsers
    (
        Id                   NVARCHAR(450)      NOT NULL,
        FirstName            NVARCHAR(50)       NOT NULL,
        LastName             NVARCHAR(50)       NOT NULL,
        AddressLine          NVARCHAR(200)      NOT NULL,
        City                 NVARCHAR(100)      NOT NULL,
        Postcode             NVARCHAR(20)       NOT NULL,
        CreatedAt            DATETIME2          NOT NULL,
        IsActive             BIT                NOT NULL,
        UserName             NVARCHAR(256)      NULL,
        NormalizedUserName   NVARCHAR(256)      NULL,
        Email                NVARCHAR(256)      NULL,
        NormalizedEmail      NVARCHAR(256)      NULL,
        EmailConfirmed       BIT                NOT NULL,
        PasswordHash         NVARCHAR(MAX)      NULL,
        SecurityStamp        NVARCHAR(MAX)      NULL,
        ConcurrencyStamp     NVARCHAR(MAX)      NULL,
        PhoneNumber          NVARCHAR(MAX)      NULL,
        PhoneNumberConfirmed BIT                NOT NULL,
        TwoFactorEnabled     BIT                NOT NULL,
        LockoutEnd           DATETIMEOFFSET     NULL,
        LockoutEnabled       BIT                NOT NULL,
        AccessFailedCount    INT                NOT NULL,
        CONSTRAINT PK_AspNetUsers PRIMARY KEY (Id)
    );
END
GO

IF OBJECT_ID('dbo.Facilities', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Facilities
    (
        FacilityId    INT IDENTITY(1,1) NOT NULL,
        Name          NVARCHAR(150)     NOT NULL,
        FacilityType  NVARCHAR(100)     NOT NULL,
        Location      NVARCHAR(200)     NOT NULL,
        Description   NVARCHAR(1000)    NULL,
        Amenities     NVARCHAR(500)     NULL,
        Capacity      INT               NULL,
        OpeningTime   TIME              NOT NULL,
        ClosingTime   TIME              NOT NULL,
        IsActive      BIT               NOT NULL,
        CreatedAt     DATETIME2         NOT NULL,
        CONSTRAINT PK_Facilities PRIMARY KEY (FacilityId),
        CONSTRAINT CK_Facility_Capacity CHECK (Capacity IS NULL OR Capacity > 0),
        CONSTRAINT CK_Facility_ClosingAfterOpening CHECK (ClosingTime > OpeningTime)
    );
END
GO

IF OBJECT_ID('dbo.Inquiries', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inquiries
    (
        InquiryId    INT IDENTITY(1,1) NOT NULL,
        Name         NVARCHAR(100)     NOT NULL,
        Email        NVARCHAR(256)     NOT NULL,
        Subject      NVARCHAR(150)     NOT NULL,
        Message      NVARCHAR(2000)    NOT NULL,
        Status       NVARCHAR(20)      NOT NULL,
        SubmittedAt  DATETIME2         NOT NULL,
        CONSTRAINT PK_Inquiries PRIMARY KEY (InquiryId)
    );
END
GO

IF OBJECT_ID('dbo.Sports', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sports
    (
        SportId INT IDENTITY(1,1) NOT NULL,
        Name    NVARCHAR(50)      NOT NULL,
        CONSTRAINT PK_Sports PRIMARY KEY (SportId)
    );
END
GO

IF OBJECT_ID('dbo.AspNetRoleClaims', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetRoleClaims
    (
        Id         INT IDENTITY(1,1) NOT NULL,
        RoleId     NVARCHAR(450)     NOT NULL,
        ClaimType  NVARCHAR(MAX)     NULL,
        ClaimValue NVARCHAR(MAX)     NULL,
        CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY (Id)
    );
END
GO

IF OBJECT_ID('dbo.AspNetUserClaims', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserClaims
    (
        Id         INT IDENTITY(1,1) NOT NULL,
        UserId     NVARCHAR(450)     NOT NULL,
        ClaimType  NVARCHAR(MAX)     NULL,
        ClaimValue NVARCHAR(MAX)     NULL,
        CONSTRAINT PK_AspNetUserClaims PRIMARY KEY (Id)
    );
END
GO

IF OBJECT_ID('dbo.AspNetUserLogins', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserLogins
    (
        LoginProvider       NVARCHAR(450) NOT NULL,
        ProviderKey         NVARCHAR(450) NOT NULL,
        ProviderDisplayName NVARCHAR(MAX) NULL,
        UserId              NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey)
    );
END
GO

IF OBJECT_ID('dbo.AspNetUserRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserRoles
    (
        UserId NVARCHAR(450) NOT NULL,
        RoleId NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId)
    );
END
GO

IF OBJECT_ID('dbo.AspNetUserTokens', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AspNetUserTokens
    (
        UserId        NVARCHAR(450) NOT NULL,
        LoginProvider NVARCHAR(450) NOT NULL,
        Name          NVARCHAR(450) NOT NULL,
        Value         NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name)
    );
END
GO

IF OBJECT_ID('dbo.Bookings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bookings
    (
        BookingId   INT IDENTITY(1,1) NOT NULL,
        MemberId    NVARCHAR(450)     NOT NULL,
        FacilityId  INT               NOT NULL,
        BookingDate DATE              NOT NULL,
        StartTime   TIME              NOT NULL,
        EndTime     TIME              NOT NULL,
        Status      NVARCHAR(20)      NOT NULL,
        CreatedAt   DATETIME2         NOT NULL,
        CONSTRAINT PK_Bookings PRIMARY KEY (BookingId),
        CONSTRAINT CK_Booking_EndAfterStart CHECK (EndTime > StartTime)
    );
END
GO

IF OBJECT_ID('dbo.FacilitySports', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FacilitySports
    (
        FacilityId INT NOT NULL,
        SportId    INT NOT NULL,
        CONSTRAINT PK_FacilitySports PRIMARY KEY (FacilityId, SportId)
    );
END
GO

IF OBJECT_ID('dbo.MemberSports', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberSports
    (
        MemberId NVARCHAR(450) NOT NULL,
        SportId  INT           NOT NULL,
        CONSTRAINT PK_MemberSports PRIMARY KEY (MemberId, SportId)
    );
END
GO

IF OBJECT_ID('dbo.Reviews', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reviews
    (
        ReviewId    INT IDENTITY(1,1) NOT NULL,
        MemberId    NVARCHAR(450)     NOT NULL,
        FacilityId  INT               NOT NULL,
        BookingId   INT               NOT NULL,
        Rating      INT               NOT NULL,
        Comment     NVARCHAR(1000)    NOT NULL,
        SubmittedAt DATETIME2         NOT NULL,
        CONSTRAINT PK_Reviews PRIMARY KEY (ReviewId),
        CONSTRAINT CK_Review_Rating CHECK (Rating BETWEEN 1 AND 5)
    );
END
GO

/* ---------------------------------------------------------------------------
   2. Foreign keys
   ------------------------------------------------------------------------- */

IF OBJECT_ID('FK_AspNetRoleClaims_AspNetRoles_RoleId', 'F') IS NULL
    ALTER TABLE dbo.AspNetRoleClaims ADD CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId
        FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles (Id) ON DELETE CASCADE;
GO

IF OBJECT_ID('FK_AspNetUserClaims_AspNetUsers_UserId', 'F') IS NULL
    ALTER TABLE dbo.AspNetUserClaims ADD CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE;
GO

IF OBJECT_ID('FK_AspNetUserLogins_AspNetUsers_UserId', 'F') IS NULL
    ALTER TABLE dbo.AspNetUserLogins ADD CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE;
GO

IF OBJECT_ID('FK_AspNetUserRoles_AspNetRoles_RoleId', 'F') IS NULL
    ALTER TABLE dbo.AspNetUserRoles ADD CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId
        FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles (Id) ON DELETE CASCADE;
GO

IF OBJECT_ID('FK_AspNetUserRoles_AspNetUsers_UserId', 'F') IS NULL
    ALTER TABLE dbo.AspNetUserRoles ADD CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE;
GO

IF OBJECT_ID('FK_AspNetUserTokens_AspNetUsers_UserId', 'F') IS NULL
    ALTER TABLE dbo.AspNetUserTokens ADD CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE;
GO

IF OBJECT_ID('FK_Bookings_AspNetUsers_MemberId', 'F') IS NULL
    ALTER TABLE dbo.Bookings ADD CONSTRAINT FK_Bookings_AspNetUsers_MemberId
        FOREIGN KEY (MemberId) REFERENCES dbo.AspNetUsers (Id) ON DELETE NO ACTION;
GO

IF OBJECT_ID('FK_Bookings_Facilities_FacilityId', 'F') IS NULL
    ALTER TABLE dbo.Bookings ADD CONSTRAINT FK_Bookings_Facilities_FacilityId
        FOREIGN KEY (FacilityId) REFERENCES dbo.Facilities (FacilityId) ON DELETE NO ACTION;
GO

IF OBJECT_ID('FK_FacilitySports_Facilities_FacilityId', 'F') IS NULL
    ALTER TABLE dbo.FacilitySports ADD CONSTRAINT FK_FacilitySports_Facilities_FacilityId
        FOREIGN KEY (FacilityId) REFERENCES dbo.Facilities (FacilityId) ON DELETE CASCADE;
GO

IF OBJECT_ID('FK_FacilitySports_Sports_SportId', 'F') IS NULL
    ALTER TABLE dbo.FacilitySports ADD CONSTRAINT FK_FacilitySports_Sports_SportId
        FOREIGN KEY (SportId) REFERENCES dbo.Sports (SportId) ON DELETE CASCADE;
GO

IF OBJECT_ID('FK_MemberSports_AspNetUsers_MemberId', 'F') IS NULL
    ALTER TABLE dbo.MemberSports ADD CONSTRAINT FK_MemberSports_AspNetUsers_MemberId
        FOREIGN KEY (MemberId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE;
GO

IF OBJECT_ID('FK_MemberSports_Sports_SportId', 'F') IS NULL
    ALTER TABLE dbo.MemberSports ADD CONSTRAINT FK_MemberSports_Sports_SportId
        FOREIGN KEY (SportId) REFERENCES dbo.Sports (SportId) ON DELETE CASCADE;
GO

IF OBJECT_ID('FK_Reviews_AspNetUsers_MemberId', 'F') IS NULL
    ALTER TABLE dbo.Reviews ADD CONSTRAINT FK_Reviews_AspNetUsers_MemberId
        FOREIGN KEY (MemberId) REFERENCES dbo.AspNetUsers (Id) ON DELETE NO ACTION;
GO

IF OBJECT_ID('FK_Reviews_Bookings_BookingId', 'F') IS NULL
    ALTER TABLE dbo.Reviews ADD CONSTRAINT FK_Reviews_Bookings_BookingId
        FOREIGN KEY (BookingId) REFERENCES dbo.Bookings (BookingId) ON DELETE NO ACTION;
GO

IF OBJECT_ID('FK_Reviews_Facilities_FacilityId', 'F') IS NULL
    ALTER TABLE dbo.Reviews ADD CONSTRAINT FK_Reviews_Facilities_FacilityId
        FOREIGN KEY (FacilityId) REFERENCES dbo.Facilities (FacilityId) ON DELETE NO ACTION;
GO

/* ---------------------------------------------------------------------------
   3. Indexes
   ------------------------------------------------------------------------- */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetRoleClaims_RoleId')
    CREATE INDEX IX_AspNetRoleClaims_RoleId ON dbo.AspNetRoleClaims (RoleId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'RoleNameIndex')
    CREATE UNIQUE INDEX RoleNameIndex ON dbo.AspNetRoles (NormalizedName) WHERE NormalizedName IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUserClaims_UserId')
    CREATE INDEX IX_AspNetUserClaims_UserId ON dbo.AspNetUserClaims (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUserLogins_UserId')
    CREATE INDEX IX_AspNetUserLogins_UserId ON dbo.AspNetUserLogins (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUserRoles_RoleId')
    CREATE INDEX IX_AspNetUserRoles_RoleId ON dbo.AspNetUserRoles (RoleId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'EmailIndex')
    CREATE INDEX EmailIndex ON dbo.AspNetUsers (NormalizedEmail);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UserNameIndex')
    CREATE UNIQUE INDEX UserNameIndex ON dbo.AspNetUsers (NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bookings_FacilityId_BookingDate')
    CREATE INDEX IX_Bookings_FacilityId_BookingDate ON dbo.Bookings (FacilityId, BookingDate);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bookings_MemberId')
    CREATE INDEX IX_Bookings_MemberId ON dbo.Bookings (MemberId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacilitySports_SportId')
    CREATE INDEX IX_FacilitySports_SportId ON dbo.FacilitySports (SportId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MemberSports_SportId')
    CREATE INDEX IX_MemberSports_SportId ON dbo.MemberSports (SportId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reviews_BookingId')
    CREATE UNIQUE INDEX IX_Reviews_BookingId ON dbo.Reviews (BookingId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reviews_FacilityId')
    CREATE INDEX IX_Reviews_FacilityId ON dbo.Reviews (FacilityId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reviews_MemberId')
    CREATE INDEX IX_Reviews_MemberId ON dbo.Reviews (MemberId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Sports_Name')
    CREATE UNIQUE INDEX IX_Sports_Name ON dbo.Sports (Name);
GO

COMMIT TRANSACTION;
GO

/* ---------------------------------------------------------------------------
   4. Essential reference data (optional)

   The application only ever queries the Sports table for existing rows
   (registration and facility search) — it does not insert into it itself
   now that seeding is disabled. Without at least one row here, those pickers
   are simply empty; the app still runs. This is real reference/lookup data
   for the domain, not test/demo content, so it is included but left as a
   separate, clearly-optional step — comment it out if you plan to manage
   your own sport list.
   ------------------------------------------------------------------------- */

IF NOT EXISTS (SELECT 1 FROM dbo.Sports)
BEGIN
    INSERT INTO dbo.Sports (Name) VALUES
        ('Tennis'), ('Football'), ('Basketball'), ('Badminton'), ('Cricket');
END
GO
