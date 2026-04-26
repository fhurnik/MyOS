CREATE OR ALTER VIEW [Identity].[v_users]
AS
SELECT
    id,
    email,
    created_at_utc,
    deleted_at_utc
FROM [identity].[users]
WHERE deleted_at_utc IS NULL;