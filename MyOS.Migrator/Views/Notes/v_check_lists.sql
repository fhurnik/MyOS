CREATE OR ALTER VIEW [notes].[v_check_lists]
AS
SELECT
    id,
    user_id,
    title,
    created_at_utc,
    updated_at_utc
FROM [notes].[check_lists]
WHERE deleted_at_utc IS NULL;
