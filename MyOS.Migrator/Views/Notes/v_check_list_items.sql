CREATE OR ALTER VIEW [notes].[v_check_list_items]
AS
SELECT
    cli.id,
    cli.check_list_id,
    cl.user_id,
    cli.text,
    cli.is_checked,
    cli.[order],
    cli.created_at_utc,
    cli.updated_at_utc
FROM [notes].[check_list_items] cli
INNER JOIN [notes].[check_lists] cl ON cl.id = cli.check_list_id
WHERE cli.deleted_at_utc IS NULL;
