/* ============================================================================
   01-create-database.sql
   Community Sports Facilities Booking System — Database Creation
   Target: Microsoft SQL Server (Express 2022 / LocalDB 2019 or later)

   This script creates the standalone coursework-evidence database. It is
   intentionally independent of the EF Core migrations that create the
   application's runtime database (see docs/database-implementation.md for
   why the two databases exist side by side) — run this against its own
   database name so it never collides with the app's own database.
   ============================================================================ */

IF DB_ID('CommunitySportsBookingSystemSql') IS NULL
BEGIN
    CREATE DATABASE CommunitySportsBookingSystemSql;
END
GO

USE CommunitySportsBookingSystemSql;
GO
