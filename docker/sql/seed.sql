USE [PRN232_LMS];
GO

IF NOT EXISTS (SELECT 1 FROM Semesters)
BEGIN
    INSERT INTO Semesters (SemesterName, StartDate, EndDate)
    VALUES
    ('Spring 2025', '2025-01-01', '2025-05-01'),
    ('Summer 2025', '2025-05-15', '2025-08-15'),
    ('Fall 2025', '2025-09-01', '2025-12-31'),
    ('Spring 2026', '2026-01-01', '2026-05-01'),
    ('Summer 2026', '2026-05-15', '2026-08-15');
END
GO

IF NOT EXISTS (SELECT 1 FROM Subjects)
BEGIN
    INSERT INTO Subjects (SubjectCode, SubjectName, Credit)
    VALUES
    ('PRN232', 'REST API', 3),
    ('SWD392', 'Software Design', 3),
    ('DBI202', 'Database Systems', 3),
    ('MAD101', 'Discrete Math', 3),
    ('PRO192', 'Programming', 3),
    ('NWC203', 'Networking', 3),
    ('OSG202', 'Operating Systems', 3),
    ('WED201', 'Web Design', 3),
    ('SWT301', 'Testing', 3),
    ('PRM392', 'Mobile Programming', 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM Students)
BEGIN
    DECLARE @i INT = 1;

    WHILE @i <= 50
    BEGIN
        INSERT INTO Students (FullName, Email, DateOfBirth)
        VALUES
        (
            CONCAT('Student ', @i),
            CONCAT('student', @i, '@gmail.com'),
            DATEADD(DAY, @i, '2003-01-01')
        );

        SET @i = @i + 1;
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM Courses)
BEGIN
    DECLARE @i INT = 1;

    WHILE @i <= 20
    BEGIN
        INSERT INTO Courses (CourseName, SemesterId)
        VALUES
        (
            CONCAT('Course ', @i),
            ((@i - 1) % 5) + 1
        );

        SET @i = @i + 1;
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM Enrollments)
BEGIN
    DECLARE @i INT = 1;

    WHILE @i <= 500
    BEGIN
        INSERT INTO Enrollments
        (
            StudentId,
            CourseId,
            EnrollDate,
            Status
        )
        VALUES
        (
            ABS(CHECKSUM(NEWID())) % 50 + 1,
            ABS(CHECKSUM(NEWID())) % 20 + 1,
            GETDATE(),
            'Active'
        );

        SET @i = @i + 1;
    END
END
GO

SELECT COUNT(*) AS Semesters FROM Semesters;
SELECT COUNT(*) AS Subjects FROM Subjects;
SELECT COUNT(*) AS Students FROM Students;
SELECT COUNT(*) AS Courses FROM Courses;
SELECT COUNT(*) AS Enrollments FROM Enrollments;
GO
