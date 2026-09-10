/* ============================================================================
   04-select-queries.sql
   Community Sports Facilities Booking System — Example SELECT Queries
   Target: Microsoft SQL Server

   Run against CommunitySportsBookingSystemSql after 01-03 have been executed.
   Each query is numbered and commented to match docs/testing.md T24 evidence.
   ============================================================================ */

USE CommunitySportsBookingSystemSql;
GO

-- 1. Display all facilities
SELECT * FROM dbo.Facilities;
GO

-- 2. Display active facilities only
SELECT FacilityId, Name, FacilityType, Location, IsActive
FROM dbo.Facilities
WHERE IsActive = 1;
GO

-- 3. Search facilities by type
SELECT FacilityId, Name, FacilityType, Location
FROM dbo.Facilities
WHERE FacilityType LIKE '%Tennis%';
GO

-- 4. Search facilities by location
SELECT FacilityId, Name, FacilityType, Location
FROM dbo.Facilities
WHERE Location LIKE '%Park%';
GO

-- 5. Display bookings with member and facility information (JOIN)
SELECT b.BookingId, m.FirstName, m.LastName, f.Name AS FacilityName,
       b.BookingDate, b.StartTime, b.EndTime, b.Status
FROM dbo.Bookings b
JOIN dbo.Members m ON b.MemberId = m.MemberId
JOIN dbo.Facilities f ON b.FacilityId = f.FacilityId
ORDER BY b.BookingDate DESC;
GO

-- 6. Display bookings for one member
SELECT b.BookingId, f.Name AS FacilityName, b.BookingDate, b.StartTime, b.EndTime, b.Status
FROM dbo.Bookings b
JOIN dbo.Facilities f ON b.FacilityId = f.FacilityId
WHERE b.MemberId = 1
ORDER BY b.BookingDate DESC;
GO

-- 7. Display bookings for a particular date
SELECT b.BookingId, m.FirstName, m.LastName, f.Name AS FacilityName, b.StartTime, b.EndTime, b.Status
FROM dbo.Bookings b
JOIN dbo.Members m ON b.MemberId = m.MemberId
JOIN dbo.Facilities f ON b.FacilityId = f.FacilityId
WHERE b.BookingDate = DATEADD(DAY, 7, CAST(GETDATE() AS DATE));
GO

-- 8. Display reviews for a facility
SELECT r.ReviewId, m.FirstName, m.LastName, r.Rating, r.Comment, r.SubmittedAt
FROM dbo.Reviews r
JOIN dbo.Members m ON r.MemberId = m.MemberId
WHERE r.FacilityId = 1
ORDER BY r.SubmittedAt DESC;
GO

-- 9. Display average rating by facility (GROUP BY + aggregation)
SELECT f.FacilityId, f.Name, AVG(CAST(r.Rating AS DECIMAL(3,2))) AS AverageRating, COUNT(*) AS ReviewCount
FROM dbo.Reviews r
JOIN dbo.Facilities f ON r.FacilityId = f.FacilityId
GROUP BY f.FacilityId, f.Name
ORDER BY AverageRating DESC;
GO

-- 10. Find preferred sports for members
SELECT m.FirstName, m.LastName, s.Name AS PreferredSport
FROM dbo.MemberSports ms
JOIN dbo.Members m ON ms.MemberId = m.MemberId
JOIN dbo.Sports s ON ms.SportId = s.SportId
ORDER BY m.LastName, s.Name;
GO

-- 11. Find facilities with no booking at a selected date/time
DECLARE @CheckDate DATE = DATEADD(DAY, 7, CAST(GETDATE() AS DATE));
DECLARE @CheckStart TIME = '14:00';
DECLARE @CheckEnd TIME = '15:00';

SELECT f.FacilityId, f.Name, f.FacilityType, f.Location
FROM dbo.Facilities f
WHERE f.IsActive = 1
  AND f.OpeningTime <= @CheckStart AND f.ClosingTime >= @CheckEnd
  AND NOT EXISTS (
        SELECT 1 FROM dbo.Bookings b
        WHERE b.FacilityId = f.FacilityId
          AND b.BookingDate = @CheckDate
          AND b.Status IN ('Pending', 'Confirmed')
          AND b.StartTime < @CheckEnd AND @CheckStart < b.EndTime
  );
GO

-- 12. Display inquiries
SELECT InquiryId, Name, Email, Subject, Status, SubmittedAt
FROM dbo.Inquiries
ORDER BY SubmittedAt DESC;
GO

-- 13. Demonstrate JOIN usage: every facility with the sports it supports
SELECT f.Name AS FacilityName, s.Name AS SportName
FROM dbo.Facilities f
JOIN dbo.FacilitySports fs ON f.FacilityId = fs.FacilityId
JOIN dbo.Sports s ON fs.SportId = s.SportId
ORDER BY f.Name;
GO

-- 14. Demonstrate filtering using WHERE: reviews rated 4 or higher
SELECT r.ReviewId, r.Rating, r.Comment
FROM dbo.Reviews r
WHERE r.Rating >= 4;
GO

-- 15. Demonstrate ORDER BY: facilities alphabetically by name
SELECT Name, FacilityType, Location
FROM dbo.Facilities
ORDER BY Name ASC;
GO

-- 16. Demonstrate GROUP BY/aggregation: number of bookings per facility
SELECT f.Name AS FacilityName, COUNT(b.BookingId) AS TotalBookings
FROM dbo.Facilities f
LEFT JOIN dbo.Bookings b ON f.FacilityId = b.FacilityId
GROUP BY f.Name
ORDER BY TotalBookings DESC;
GO
