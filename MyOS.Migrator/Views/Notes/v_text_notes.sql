CREATE OR ALTER VIEW [notes].[v_text_notes]
AS
SELECT
    id,
    user_id,
    title,
    text,
    created_at_utc,
    updated_at_utc
FROM [notes].[text_notes]
WHERE deleted_at_utc IS NULL;
