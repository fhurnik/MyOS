CREATE OR ALTER VIEW [storage].[v_storage_quotas]
AS
SELECT
    id,
    user_id,
    max_bytes,
    used_bytes,
    (max_bytes - used_bytes) AS available_bytes,
    created_at_utc,
    updated_at_utc
FROM [storage].[storage_quotas];
