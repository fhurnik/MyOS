CREATE OR ALTER VIEW [fitness].[v_exercises]
AS
SELECT
    id,
    user_id,
    name,
    activity_type,
    strength_category,
    distance,
    created_at_utc,
    updated_at_utc
FROM [fitness].[exercises]
WHERE deleted_at_utc IS NULL;
