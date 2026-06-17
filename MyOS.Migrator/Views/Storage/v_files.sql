CREATE OR ALTER VIEW [storage].[v_files]
AS
SELECT
    id,
    user_id,
    storage_file_name,
    original_name,
    extension,
    content_type,
    size_bytes,
    created_at_utc,
    updated_at_utc
FROM [storage].[files]
WHERE deleted_at_utc IS NULL;
