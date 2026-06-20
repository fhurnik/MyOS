-- One summary row per user (only users who have at least one workout). "This week" uses
-- ISO_WEEK + calendar YEAR (minor imprecision at the year boundary, acceptable for a dashboard).
CREATE OR ALTER VIEW [fitness].[v_user_fitness_dashboard]
AS
SELECT
    uw.user_id,
    uw.last_workout_date,
    DATEDIFF(DAY, uw.last_workout_date, CAST(SYSUTCDATETIME() AS DATE)) AS days_since_last_workout,
    (
        SELECT COUNT(*)
        FROM [fitness].[workouts] w2
        WHERE w2.user_id = uw.user_id
          AND w2.deleted_at_utc IS NULL
          AND DATEPART(ISO_WEEK, w2.date) = DATEPART(ISO_WEEK, CAST(SYSUTCDATETIME() AS DATE))
          AND DATEPART(YEAR, w2.date) = DATEPART(YEAR, CAST(SYSUTCDATETIME() AS DATE))
    ) AS workouts_this_week,
    (
        SELECT COUNT(s.id)
        FROM [fitness].[exercise_sets] s
        INNER JOIN [fitness].[workout_exercises] we ON we.id = s.workout_exercise_id AND we.deleted_at_utc IS NULL
        INNER JOIN [fitness].[workouts] w3 ON w3.id = we.workout_id AND w3.deleted_at_utc IS NULL
        WHERE w3.user_id = uw.user_id
          AND s.deleted_at_utc IS NULL
          AND DATEPART(ISO_WEEK, w3.date) = DATEPART(ISO_WEEK, CAST(SYSUTCDATETIME() AS DATE))
          AND DATEPART(YEAR, w3.date) = DATEPART(YEAR, CAST(SYSUTCDATETIME() AS DATE))
    ) AS sets_this_week
FROM (
    SELECT user_id, MAX(date) AS last_workout_date
    FROM [fitness].[workouts]
    WHERE deleted_at_utc IS NULL
    GROUP BY user_id
) uw;
