IF DB_ID(N'$(DB_NAME)') IS NULL
BEGIN
    PRINT 'Creating database $(DB_NAME)';
    EXEC('CREATE DATABASE [$(DB_NAME)]');
END
ELSE
BEGIN
    PRINT 'Database $(DB_NAME) already exists';
END
GO