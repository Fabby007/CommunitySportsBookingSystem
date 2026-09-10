/* ============================================================================
   05-testing-queries.sql
   Community Sports Facilities Booking System — Database Testing Queries
   Target: Microsoft SQL Server

   Verification queries for T25 (foreign keys / constraints) in docs/testing.md.
   Section 2 contains statements that are EXPECTED TO FAIL — each is commented
   with the constraint it proves is enforced. Run them individually (not as one
   batch) so an expected failure doesn't stop the rest of the script.
   ============================================================================ */

USE CommunitySportsBookingSystemSql;
GO

/* ---------- Section 1: row counts and orphan-FK checks (all expected to pass) ---------- */

-- Row counts per table
-- (RowsFound, not RowCount, since ROWCOUNT is a reserved T-SQL keyword)
SELECT 'Members' AS TableName, COUNT(*) AS RowsFound FROM dbo.Members
UNION ALL SELECT 'Sports', COUNT(*) FROM dbo.Sports
UNION ALL SELECT 'MemberSports', COUNT(*) FROM dbo.MemberSports
UNION ALL SELECT 'Facilities', COUNT(*) FROM dbo.Facilities
UNION ALL SELECT 'FacilitySports', COUNT(*) FROM dbo.FacilitySports
UNION ALL SELECT 'Bookings', COUNT(*) FROM dbo.Bookings
UNION ALL SELECT 'Reviews', COUNT(*) FROM dbo.Reviews
UNION ALL SELECT 'Inquiries', COUNT(*) FROM dbo.Inquiries;
GO

-- Orphan check: every Booking must reference an existing Member and Facility
SELECT b.BookingId FROM dbo.Bookings b
WHERE NOT EXISTS (SELECT 1 FROM dbo.Members m WHERE m.MemberId = b.MemberId)
   OR NOT EXISTS (SELECT 1 FROM dbo.Facilities f WHERE f.FacilityId = b.FacilityId);
-- Expected: 0 rows (foreign keys make this structurally impossible; this query
-- documents the invariant even though the FK constraints already guarantee it).
GO

-- Orphan check: every Review must reference exactly one Booking, and that
-- Booking must belong to the same Member and Facility as the Review.
SELECT r.ReviewId
FROM dbo.Reviews r
JOIN dbo.Bookings b ON r.BookingId = b.BookingId
WHERE r.MemberId <> b.MemberId OR r.FacilityId <> b.FacilityId;
-- Expected: 0 rows — a review's member/facility must match its booking's.
GO

/* ---------- Section 2: constraint-violation attempts (EACH EXPECTED TO FAIL) ---------- */

-- 2a. Duplicate email — expected to fail: violates UQ_Members_Email
INSERT INTO dbo.Members (FirstName, LastName, Email, PasswordHash, Phone, AddressLine, City, Postcode)
VALUES ('Dupe', 'Test', 'demo.member@example.com', 'x', '000', 'x', 'x', 'x');
GO

-- 2b. Rating out of range — expected to fail: violates CK_Reviews_Rating
INSERT INTO dbo.Reviews (MemberId, FacilityId, BookingId, Rating, Comment)
VALUES (1, 1, 2, 6, 'Rating too high, should be rejected');
GO

-- 2c. End time not after start time — expected to fail: violates CK_Bookings_EndAfterStart
INSERT INTO dbo.Bookings (MemberId, FacilityId, BookingDate, StartTime, EndTime, Status)
VALUES (1, 1, '2026-12-01', '11:00', '10:00', 'Confirmed');
GO

-- 2d. Booking referencing a non-existent facility — expected to fail: violates FK_Bookings_Facilities
INSERT INTO dbo.Bookings (MemberId, FacilityId, BookingDate, StartTime, EndTime, Status)
VALUES (1, 9999, '2026-12-01', '10:00', '11:00', 'Confirmed');
GO

-- 2e. Second review on the same booking — expected to fail: violates UQ_Reviews_BookingId
INSERT INTO dbo.Reviews (MemberId, FacilityId, BookingId, Rating, Comment)
VALUES (1, 1, 1, 3, 'Second review on the same booking, should be rejected');
GO
