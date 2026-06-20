-- Training volume: set count per (user, exercise, ISO week). Cardio has no sets, so only
-- strength entries contribute. Total weekly volume = SUM(set_count) across exercises client-side.
CREATE OR ALTER VIEW [fitness].[v_weekly_sets]
AS
SELECT
    w.user_id,
    we.exercise_id,
    e.name AS exercise_name,
    CASE
        WHEN DATEPART(ISO_WEEK, w.date) >= 52 AND DATEPART(MONTH, w.date) = 1 THEN DATEPART(YEAR, w.date) - 1
        WHEN DATEPART(ISO_WEEK, w.date) = 1 AND DATEPART(MONTH, w.date) = 12 THEN DATEPART(YEAR, w.date) + 1
        ELSE DATEPART(YEAR, w.date)
    END AS iso_year,
    DATEPART(ISO_WEEK, w.date) AS iso_week,
    COUNT(s.id) AS set_count
FROM [fitness].[exercise_sets] s
INNER JOIN [fitness].[workout_exercises] we
    ON we.id = s.workout_exercise_id AND we.deleted_at_utc IS NULL
INNER JOIN [fitness].[workouts] w
    ON w.id = we.workout_id AND w.deleted_at_utc IS NULL
INNER JOIN [fitness].[exercises] e
    ON e.id = we.exercise_id
WHERE s.deleted_at_utc IS NULL
GROUP BY
    w.user_id,
    we.exercise_id,
    e.name,
    CASE
        WHEN DATEPART(ISO_WEEK, w.date) >= 52 AND DATEPART(MONTH, w.date) = 1 THEN DATEPART(YEAR, w.date) - 1
        WHEN DATEPART(ISO_WEEK, w.date) = 1 AND DATEPART(MONTH, w.date) = 12 THEN DATEPART(YEAR, w.date) + 1
        ELSE DATEPART(YEAR, w.date)
    END,
    DATEPART(ISO_WEEK, w.date);
