-- Per-session progression value for an exercise. One row per (non-deleted) workout entry.
-- value is the metric matching the target: weighted = max weight, bodyweight = max reps,
-- cardio = duration (seconds). activity_type/strength_category convey the unit.
-- Enum byte values: activity_type Cardio=0 / Strength=1; strength_category Weighted=0 / Bodyweight=1.
CREATE OR ALTER VIEW [fitness].[v_exercise_progression]
AS
SELECT
    we.exercise_id,
    we.id AS workout_exercise_id,
    we.workout_id,
    w.user_id,
    w.date,
    e.activity_type,
    e.strength_category,
    CAST(
        CASE
            WHEN e.activity_type = 0 THEN we.duration
            WHEN e.strength_category = 0 THEN sets_agg.max_weight
            WHEN e.strength_category = 1 THEN sets_agg.max_reps
        END AS DECIMAL(9, 2)) AS value
FROM [fitness].[workout_exercises] we
INNER JOIN [fitness].[workouts] w
    ON w.id = we.workout_id AND w.deleted_at_utc IS NULL
INNER JOIN [fitness].[exercises] e
    ON e.id = we.exercise_id
OUTER APPLY (
    SELECT MAX(s.weight) AS max_weight, MAX(s.reps) AS max_reps
    FROM [fitness].[exercise_sets] s
    WHERE s.workout_exercise_id = we.id AND s.deleted_at_utc IS NULL
) sets_agg
WHERE we.deleted_at_utc IS NULL;
