/* ============================================================================
   02-create-tables.sql
   Community Sports Facilities Booking System — Table Creation
   Target: Microsoft SQL Server

   NOTE on Members: the running ASP.NET Core application stores member accounts
   in ASP.NET Core Identity's AspNetUsers table (string primary key, extended
   with the same profile fields as below) so it gets proven password-hashing
   and sign-in plumbing for free. This standalone script instead creates the
   simpler, brief-literal "Members" table (INT IDENTITY) as independent SQL
   Server design/implementation evidence — same attributes and business rules,
   simpler natural key. See docs/data-dictionary.md for the full mapping note.
   ============================================================================ */

USE CommunitySportsBookingSystemSql;
GO

IF OBJECT_ID('dbo.Reviews', 'U') IS NOT NULL DROP TABLE dbo.Reviews;
IF OBJECT_ID('dbo.Bookings', 'U') IS NOT NULL DROP TABLE dbo.Bookings;
IF OBJECT_ID('dbo.FacilitySports', 'U') IS NOT NULL DROP TABLE dbo.FacilitySports;
IF OBJECT_ID('dbo.MemberSports', 'U') IS NOT NULL DROP TABLE dbo.MemberSports;
IF OBJECT_ID('dbo.Facilities', 'U') IS NOT NULL DROP TABLE dbo.Facilities;
IF OBJECT_ID('dbo.Sports', 'U') IS NOT NULL DROP TABLE dbo.Sports;
IF OBJECT_ID('dbo.Members', 'U') IS NOT NULL DROP TABLE dbo.Members;
IF OBJECT_ID('dbo.Inquiries', 'U') IS NOT NULL DROP TABLE dbo.Inquiries;
GO

CREATE TABLE dbo.Members
(
    MemberId        INT IDENTITY(1,1)  NOT NULL,
    FirstName       NVARCHAR(50)       NOT NULL,
    LastName        NVARCHAR(50)       NOT NULL,
    Email           NVARCHAR(256)      NOT NULL,
    PasswordHash    NVARCHAR(MAX)      NOT NULL,
    Phone           NVARCHAR(20)       NOT NULL,
    AddressLine     NVARCHAR(200)      NOT NULL,
    City            NVARCHAR(100)      NOT NULL,
    Postcode        NVARCHAR(20)       NOT NULL,
    CreatedAt       DATETIME2          NOT NULL CONSTRAINT DF_Members_CreatedAt DEFAULT SYSDATETIME(),
    IsActive        BIT                NOT NULL CONSTRAINT DF_Members_IsActive DEFAULT 1,
    CONSTRAINT PK_Members PRIMARY KEY (MemberId),
    CONSTRAINT UQ_Members_Email UNIQUE (Email)
);
GO

CREATE TABLE dbo.Sports
(
    SportId   INT IDENTITY(1,1) NOT NULL,
    Name      NVARCHAR(50)      NOT NULL,
    CONSTRAINT PK_Sports PRIMARY KEY (SportId),
    CONSTRAINT UQ_Sports_Name UNIQUE (Name)
);
GO

CREATE TABLE dbo.MemberSports
(
    MemberId  INT NOT NULL,
    SportId   INT NOT NULL,
    CONSTRAINT PK_MemberSports PRIMARY KEY (MemberId, SportId),
    CONSTRAINT FK_MemberSports_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members(MemberId) ON DELETE CASCADE,
    CONSTRAINT FK_MemberSports_Sports FOREIGN KEY (SportId) REFERENCES dbo.Sports(SportId) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Facilities
(
    FacilityId    INT IDENTITY(1,1)  NOT NULL,
    Name          NVARCHAR(150)      NOT NULL,
    FacilityType  NVARCHAR(100)      NOT NULL,
    Location      NVARCHAR(200)      NOT NULL,
    Description   NVARCHAR(1000)     NULL,
    Amenities     NVARCHAR(500)      NULL,
    Capacity      INT                NULL,
    OpeningTime   TIME(0)            NOT NULL,
    ClosingTime   TIME(0)            NOT NULL,
    IsActive      BIT                NOT NULL CONSTRAINT DF_Facilities_IsActive DEFAULT 1,
    CreatedAt     DATETIME2          NOT NULL CONSTRAINT DF_Facilities_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Facilities PRIMARY KEY (FacilityId),
    CONSTRAINT CK_Facilities_ClosingAfterOpening CHECK (ClosingTime > OpeningTime),
    CONSTRAINT CK_Facilities_Capacity CHECK (Capacity IS NULL OR Capacity > 0)
);
GO

CREATE TABLE dbo.FacilitySports
(
    FacilityId  INT NOT NULL,
    SportId     INT NOT NULL,
    CONSTRAINT PK_FacilitySports PRIMARY KEY (FacilityId, SportId),
    CONSTRAINT FK_FacilitySports_Facilities FOREIGN KEY (FacilityId) REFERENCES dbo.Facilities(FacilityId) ON DELETE CASCADE,
    CONSTRAINT FK_FacilitySports_Sports FOREIGN KEY (SportId) REFERENCES dbo.Sports(SportId) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Bookings
(
    BookingId     INT IDENTITY(1,1) NOT NULL,
    MemberId      INT               NOT NULL,
    FacilityId    INT               NOT NULL,
    BookingDate   DATE              NOT NULL,
    StartTime     TIME(0)           NOT NULL,
    EndTime       TIME(0)           NOT NULL,
    Status        NVARCHAR(20)      NOT NULL CONSTRAINT DF_Bookings_Status DEFAULT 'Confirmed',
    CreatedAt     DATETIME2         NOT NULL CONSTRAINT DF_Bookings_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Bookings PRIMARY KEY (BookingId),
    CONSTRAINT FK_Bookings_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members(MemberId),
    CONSTRAINT FK_Bookings_Facilities FOREIGN KEY (FacilityId) REFERENCES dbo.Facilities(FacilityId),
    CONSTRAINT CK_Bookings_EndAfterStart CHECK (EndTime > StartTime),
    CONSTRAINT CK_Bookings_Status CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled', 'Completed'))
);
GO

CREATE INDEX IX_Bookings_FacilityId_BookingDate ON dbo.Bookings (FacilityId, BookingDate);
GO

CREATE TABLE dbo.Reviews
(
    ReviewId      INT IDENTITY(1,1) NOT NULL,
    MemberId      INT               NOT NULL,
    FacilityId    INT               NOT NULL,
    BookingId     INT               NOT NULL,
    Rating        INT               NOT NULL,
    Comment       NVARCHAR(1000)    NOT NULL,
    SubmittedAt   DATETIME2         NOT NULL CONSTRAINT DF_Reviews_SubmittedAt DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Reviews PRIMARY KEY (ReviewId),
    CONSTRAINT FK_Reviews_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members(MemberId),
    CONSTRAINT FK_Reviews_Facilities FOREIGN KEY (FacilityId) REFERENCES dbo.Facilities(FacilityId),
    CONSTRAINT FK_Reviews_Bookings FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(BookingId),
    CONSTRAINT UQ_Reviews_BookingId UNIQUE (BookingId),
    CONSTRAINT CK_Reviews_Rating CHECK (Rating BETWEEN 1 AND 5)
);
GO

CREATE TABLE dbo.Inquiries
(
    InquiryId     INT IDENTITY(1,1) NOT NULL,
    Name          NVARCHAR(100)     NOT NULL,
    Email         NVARCHAR(256)     NOT NULL,
    Subject       NVARCHAR(150)     NOT NULL,
    Message       NVARCHAR(2000)    NOT NULL,
    Status        NVARCHAR(20)      NOT NULL CONSTRAINT DF_Inquiries_Status DEFAULT 'New',
    SubmittedAt   DATETIME2         NOT NULL CONSTRAINT DF_Inquiries_SubmittedAt DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Inquiries PRIMARY KEY (InquiryId),
    CONSTRAINT CK_Inquiries_Status CHECK (Status IN ('New', 'Reviewed', 'Closed'))
);
GO
