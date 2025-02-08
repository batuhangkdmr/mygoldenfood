using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGoldenFood.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslationTablee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "Translations");

            migrationBuilder.RenameColumn(
                name: "FieldName",
                table: "Translations",
                newName: "TableName");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "Translations",
                newName: "ReferenceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TableName",
                table: "Translations",
                newName: "FieldName");

            migrationBuilder.RenameColumn(
                name: "ReferenceId",
                table: "Translations",
                newName: "EntityId");

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "Translations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
