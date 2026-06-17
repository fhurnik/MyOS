CREATE OR ALTER VIEW [storage].[v_folders]
AS
SELECT
    id,
    user_id,
    parent_id,
    name,
    created_at_utc,
    updated_at_utc
FROM [storage].[folders]
WHERE deleted_at_utc IS NULL;
