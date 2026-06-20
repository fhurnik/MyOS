CREATE OR ALTER VIEW [fitness].[v_exercise_targets]
AS
SELECT
    id,
    exercise_id,
    user_id,
    value,
    created_at_utc,
    updated_at_utc
FROM [fitness].[exercise_targets];
