CREATE OR ALTER VIEW [storage].[v_allowed_file_types]
AS
SELECT
    id,
    extension,
    content_type,
    category
FROM [storage].[allowed_file_types]
WHERE is_active = 1;
