/* ============================================================================
   03-seed-data.sql
   Community Sports Facilities Booking System — Sample Data
   Target: Microsoft SQL Server

   Realistic but entirely fictional data, inserted in foreign-key-safe order.
   PasswordHash values below are placeholder strings representing where a real
   PBKDF2 hash would go — never plain text (the running application computes
   real hashes via ASP.NET Core Identity; see docs/database-implementation.md).
   ============================================================================ */

USE CommunitySportsBookingSystemSql;
GO

-- 1. Sports
INSERT INTO dbo.Sports (Name) VALUES
    ('Tennis'), ('Football'), ('Basketball'), ('Badminton'), ('Cricket');
GO

-- 2. Members (fictional demo accounts)
INSERT INTO dbo.Members (FirstName, LastName, Email, PasswordHash, Phone, AddressLine, City, Postcode) VALUES
    ('Dana', 'Morgan', 'demo.member@example.com', 'PLACEHOLDER_PBKDF2_HASH_1', '555-0100', '12 Willow Street', 'Springfield', 'SP1 2AB'),
    ('Priya', 'Nair', 'priya.nair@example.com', 'PLACEHOLDER_PBKDF2_HASH_2', '555-0101', '48 Maple Avenue', 'Springfield', 'SP2 4CD'),
    ('Tom', 'Okafor', 'tom.okafor@example.com', 'PLACEHOLDER_PBKDF2_HASH_3', '555-0102', '7 Birch Close', 'Springfield', 'SP3 6EF');
GO

-- 3. Member preferred sports
INSERT INTO dbo.MemberSports (MemberId, SportId) VALUES
    (1, 1),                 -- Dana: Tennis
    (2, 3), (2, 4),         -- Priya: Basketball, Badminton
    (3, 2), (3, 5);         -- Tom: Football, Cricket
GO

-- 4. Facilities
INSERT INTO dbo.Facilities (Name, FacilityType, Location, Description, Amenities, Capacity, OpeningTime, ClosingTime) VALUES
    ('Riverside Tennis Courts', 'Outdoor Tennis Court', 'Riverside Park, Community Way', 'Two floodlit hard-surface tennis courts beside the river.', 'Floodlights, seating, water fountain', 4, '08:00', '21:00'),
    ('Central Football Pitch', 'Outdoor Football Pitch', 'Central Sports Ground, High Street', 'Full-size grass football pitch with changing rooms.', 'Changing rooms, floodlights, parking', 22, '09:00', '22:00'),
    ('Oakwood Sports Hall', 'Indoor Multi-Purpose Hall', 'Oakwood Community Centre, Elm Road', 'Indoor hall marked for basketball and badminton.', 'Indoor courts, scoreboards, seating', 30, '07:00', '22:00'),
    ('Greenfield Cricket Ground', 'Outdoor Cricket Ground', 'Greenfield Recreation Area, Meadow Lane', 'Full-size cricket ground with practice nets.', 'Practice nets, pavilion, parking', 22, '08:00', '20:00'),
    ('Elmwood Basketball Court', 'Outdoor Basketball Court', 'Elmwood Park, Park Avenue', 'Outdoor half-court, popular with local youth leagues.', 'Floodlights, seating', 10, '08:00', '21:00');
GO

-- 5. Facility <-> Sport links (independent of FacilityType — a hall can support two sports)
INSERT INTO dbo.FacilitySports (FacilityId, SportId) VALUES
    (1, 1),                 -- Riverside Tennis Courts: Tennis
    (2, 2),                 -- Central Football Pitch: Football
    (3, 3), (3, 4),         -- Oakwood Sports Hall: Basketball, Badminton
    (4, 5),                 -- Greenfield Cricket Ground: Cricket
    (5, 3);                 -- Elmwood Basketball Court: Basketball
GO

-- 6. Bookings (mix of Completed, Confirmed, Cancelled to exercise every status)
INSERT INTO dbo.Bookings (MemberId, FacilityId, BookingDate, StartTime, EndTime, Status) VALUES
    (1, 1, DATEADD(DAY, -7, CAST(GETDATE() AS DATE)), '10:00', '11:00', 'Completed'),
    (1, 1, DATEADD(DAY, 7, CAST(GETDATE() AS DATE)), '14:00', '15:00', 'Confirmed'),
    (2, 3, DATEADD(DAY, -3, CAST(GETDATE() AS DATE)), '18:00', '19:00', 'Completed'),
    (3, 4, DATEADD(DAY, 10, CAST(GETDATE() AS DATE)), '09:00', '11:00', 'Confirmed'),
    (2, 3, DATEADD(DAY, 2, CAST(GETDATE() AS DATE)), '17:00', '18:00', 'Cancelled');
GO

-- 7. Reviews (one per Completed booking demonstrated; BookingId is UNIQUE)
INSERT INTO dbo.Reviews (MemberId, FacilityId, BookingId, Rating, Comment) VALUES
    (1, 1, 1, 5, 'Great court, well maintained and the floodlights are excellent in the evening.'),
    (2, 3, 3, 4, 'Good indoor hall, gets a little crowded on weekday evenings.');
GO

-- 8. Inquiries
INSERT INTO dbo.Inquiries (Name, Email, Subject, Message, Status) VALUES
    ('Jamie Guest', 'jamie.guest@example.com', 'Question about hall hire', 'Can I hire the sports hall for a private event?', 'New'),
    ('Sam Carter', 'sam.carter@example.com', 'Cricket nets availability', 'Are the practice nets bookable separately from the main ground?', 'Reviewed');
GO
