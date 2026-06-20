CREATE OR ALTER VIEW [fitness].[v_workout_exercises]
AS
SELECT
    we.id,
    we.workout_id,
    w.user_id,
    we.exercise_id,
    e.name AS exercise_name,
    e.activity_type,
    e.strength_category,
    we.position,
    we.duration,
    we.created_at_utc
FROM [fitness].[workout_exercises] we
INNER JOIN [fitness].[workouts] w
    ON w.id = we.workout_id AND w.deleted_at_utc IS NULL
-- exercises are NOT filtered on deleted_at_utc: a soft-deleted exercise must still expose its
-- name/type for historical workout entries that reference it.
INNER JOIN [fitness].[exercises] e
    ON e.id = we.exercise_id
WHERE we.deleted_at_utc IS NULL;
