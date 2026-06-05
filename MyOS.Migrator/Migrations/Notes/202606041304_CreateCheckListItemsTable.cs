using FluentMigrator;

namespace MyOS.Migrator.Migrations.Notes
{
    [Migration(202606041304)]
    public sealed class CreateCheckListItemsTable : Migration
    {
        public override void Up()
        {
            Create.Table("check_list_items")
                .InSchema("notes")

                .WithColumn("id")
                    .AsGuid()
                    .PrimaryKey()

                .WithColumn("check_list_id")
                    .AsGuid()
                    .NotNullable()

                .WithColumn("text")
                    .AsString(2000)
                    .NotNullable()

                .WithColumn("is_checked")
                    .AsBoolean()
                    .NotNullable()
                    .WithDefaultValue(false)

                .WithColumn("order")
                    .AsInt32()
                    .NotNullable()

                .WithColumn("created_at_utc")
                    .AsDateTime2()
                    .NotNullable()

                .WithColumn("updated_at_utc")
                    .AsDateTime2()
                    .Nullable()

                .WithColumn("deleted_at_utc")
                    .AsDateTime2()
                    .Nullable();

            Create.ForeignKey("fk_check_list_items_check_lists")
                .FromTable("check_list_items").InSchema("notes").ForeignColumn("check_list_id")
                .ToTable("check_lists").InSchema("notes").PrimaryColumn("id");

            Create.Index("ix_check_list_items_check_list_id_order")
                .OnTable("check_list_items").InSchema("notes")
                .OnColumn("check_list_id").Ascending()
                .OnColumn("order").Ascending();
        }

        public override void Down()
        {
            Delete.Table("check_list_items").InSchema("notes");
        }
    }
}
