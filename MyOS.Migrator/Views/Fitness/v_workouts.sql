CREATE OR ALTER VIEW [fitness].[v_workouts]
AS
SELECT
    id,
    user_id,
    date,
    notes,
    created_at_utc,
    updated_at_utc
FROM [fitness].[workouts]
WHERE deleted_at_utc IS NULL;
