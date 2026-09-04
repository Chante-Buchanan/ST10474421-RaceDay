

    CREATE DATABASE RaceDayDB;


IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID('dbo.Enrolments', 'U') IS NOT NULL DROP TABLE dbo.Enrolments;
IF OBJECT_ID('dbo.Routes', 'U') IS NOT NULL DROP TABLE dbo.Routes;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO


CREATE TABLE dbo.Users (
    user_id         INT IDENTITY(1,1) PRIMARY KEY,
    full_name       NVARCHAR(100)   NOT NULL,
    email           NVARCHAR(150)   NOT NULL UNIQUE,
    password_hash   NVARCHAR(255)   NOT NULL,
    role            VARCHAR(20)     NOT NULL
                        CONSTRAINT CK_Users_Role CHECK (role IN ('Organiser', 'Participant')),
    phone_number    VARCHAR(20)     NULL,
    created_at      DATETIME        NOT NULL DEFAULT GETDATE()
);
GO


CREATE TABLE dbo.Events (
    event_id        INT IDENTITY(1,1) PRIMARY KEY,
    organiser_id    INT             NOT NULL,
    event_name      NVARCHAR(150)   NOT NULL,
    description     NVARCHAR(MAX)   NULL,
    event_date      DATE            NOT NULL,
    location        NVARCHAR(150)   NULL,
    city            NVARCHAR(100)   NULL,
    province        NVARCHAR(100)   NULL,
    status          VARCHAR(20)     NOT NULL DEFAULT 'Draft'
                        CONSTRAINT CK_Events_Status CHECK (status IN ('Draft', 'Published', 'Cancelled', 'Completed')),
    created_at      DATETIME        NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Events_Organiser FOREIGN KEY (organiser_id) REFERENCES dbo.Users(user_id)
);
GO


CREATE TABLE dbo.Categories (
    category_id     INT IDENTITY(1,1) PRIMARY KEY,
    event_id        INT             NOT NULL,
    category_name   NVARCHAR(100)   NOT NULL,
    distance_km     DECIMAL(6,2)    NOT NULL,
    start_time      TIME            NULL,
    max_participants INT            NULL,
    entry_fee       DECIMAL(8,2)    NOT NULL DEFAULT 0,
    CONSTRAINT FK_Categories_Event FOREIGN KEY (event_id) REFERENCES dbo.Events(event_id)
);
GO


CREATE TABLE dbo.Routes (
    route_id            INT IDENTITY(1,1) PRIMARY KEY,
    category_id         INT             NOT NULL UNIQUE,
    route_name          NVARCHAR(150)   NULL,
    distance_km         DECIMAL(6,2)    NULL,
    elevation_gain_m    INT             NULL,
    gpx_url             NVARCHAR(255)   NULL,
    description         NVARCHAR(MAX)   NULL,
    CONSTRAINT FK_Routes_Category FOREIGN KEY (category_id) REFERENCES dbo.Categories(category_id)
);
GO


CREATE TABLE dbo.Enrolments (
    enrolment_id    INT IDENTITY(1,1) PRIMARY KEY,
    category_id     INT             NOT NULL,
    participant_id  INT             NOT NULL,
    bib_number      VARCHAR(10)     NULL,
    enrolment_date  DATETIME        NOT NULL DEFAULT GETDATE(),
    status          VARCHAR(20)     NOT NULL DEFAULT 'Pending'
                        CONSTRAINT CK_Enrolments_Status CHECK (status IN ('Pending', 'Confirmed', 'Cancelled')),
    payment_status  VARCHAR(20)     NOT NULL DEFAULT 'Unpaid'
                        CONSTRAINT CK_Enrolments_PaymentStatus CHECK (payment_status IN ('Unpaid', 'Paid', 'Refunded')),
    CONSTRAINT FK_Enrolments_Category FOREIGN KEY (category_id) REFERENCES dbo.Categories(category_id),
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (participant_id) REFERENCES dbo.Users(user_id),
    CONSTRAINT UQ_Enrolments_ParticipantCategory UNIQUE (category_id, participant_id)
);
GO


CREATE TABLE dbo.Results (
    result_id       INT IDENTITY(1,1) PRIMARY KEY,
    enrolment_id    INT             NOT NULL UNIQUE,
    finish_time     TIME            NULL,
    position        INT             NULL,
    status          VARCHAR(20)     NOT NULL DEFAULT 'Finished'
                        CONSTRAINT CK_Results_Status CHECK (status IN ('Finished', 'DNF', 'DNS')),
    recorded_at     DATETIME        NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Results_Enrolment FOREIGN KEY (enrolment_id) REFERENCES dbo.Enrolments(enrolment_id)
);
GO



-- Organisers (2)
INSERT INTO dbo.Users (full_name, email, password_hash, role, phone_number) VALUES
('Thandiwe Mokoena', 'thandiwe@raceday-events.co.za', 'hashed_pw_1', 'Organiser', '0821234567'),
('Johan van der Merwe', 'johan@capetownroadrunners.co.za', 'hashed_pw_2', 'Organiser', '0837654321');

-- Participants (2)
INSERT INTO dbo.Users (full_name, email, password_hash, role, phone_number) VALUES
('Lerato Dube', 'lerato.dube@gmail.com', 'hashed_pw_3', 'Participant', '0711112222'),
('Michael Botha', 'michael.botha@gmail.com', 'hashed_pw_4', 'Participant', '0723334444');

-- Events (3)
INSERT INTO dbo.Events (organiser_id, event_name, description, event_date, location, city, province, status) VALUES
(1, 'Pretoria Park Run Challenge', 'A community road running event through the Pretoria CBD and parks.', '2026-10-10', 'Union Buildings Grounds', 'Pretoria', 'Gauteng', 'Published'),
(2, 'Cape Town Coastal Cycle Tour', 'A scenic cycling tour along the Atlantic seaboard.', '2026-11-08', 'Sea Point Promenade', 'Cape Town', 'Western Cape', 'Published'),
(1, 'Soweto Community Walk & Run', 'A family-friendly walk and run supporting local charities.', '2026-09-20', 'Orlando Stadium', 'Soweto', 'Gauteng', 'Draft');

-- Categories (2 per event)
INSERT INTO dbo.Categories (event_id, category_name, distance_km, start_time, max_participants, entry_fee) VALUES
(1, '10km Road Run', 10.00, '07:00:00', 500, 150.00),
(1, '5km Fun Run', 5.00, '07:15:00', 300, 100.00),
(2, '60km Cycle Tour', 60.00, '06:30:00', 800, 350.00),
(2, '30km Cycle Tour', 30.00, '06:45:00', 600, 250.00),
(3, '5km Community Walk', 5.00, '08:00:00', 400, 50.00),
(3, '10km Charity Run', 10.00, '08:00:00', 400, 80.00);

-- Routes (for a couple of categories)
INSERT INTO dbo.Routes (category_id, route_name, distance_km, elevation_gain_m, gpx_url, description) VALUES
(1, 'Pretoria CBD Loop', 10.00, 120, 'https://raceday.example.com/gpx/pretoria-10km.gpx', 'Flat city loop past major landmarks.'),
(3, 'Atlantic Seaboard Route', 60.00, 450, 'https://raceday.example.com/gpx/ct-60km.gpx', 'Coastal route with rolling hills near Camps Bay.');

-- Enrolments (participants entering categories)
INSERT INTO dbo.Enrolments (category_id, participant_id, bib_number, status, payment_status) VALUES
(1, 3, 'A101', 'Confirmed', 'Paid'),
(3, 3, 'B205', 'Confirmed', 'Paid'),
(1, 4, 'A102', 'Confirmed', 'Paid'),
(5, 4, 'C310', 'Pending', 'Unpaid');

-- Results (for completed enrolments)
INSERT INTO dbo.Results (enrolment_id, finish_time, position, status) VALUES
(1, '00:48:32', 12, 'Finished'),
(3, '00:52:10', 25, 'Finished');
GO