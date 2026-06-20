CREATE OR ALTER VIEW [fitness].[v_exercise_sets]
AS
SELECT
    s.id,
    s.workout_exercise_id,
    we.workout_id,
    w.user_id,
    s.position,
    s.reps,
    s.weight,
    s.added_weight,
    s.negatives,
    s.rir,
    s.created_at_utc
FROM [fitness].[exercise_sets] s
INNER JOIN [fitness].[workout_exercises] we
    ON we.id = s.workout_exercise_id AND we.deleted_at_utc IS NULL
INNER JOIN [fitness].[workouts] w
    ON w.id = we.workout_id AND w.deleted_at_utc IS NULL
WHERE s.deleted_at_utc IS NULL;
