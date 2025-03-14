using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGoldenFood.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseTranslatedValueLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TranslatedValue",
                table: "Translations",
                type: "NVARCHAR(MAX)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TranslatedValue",
                table: "Translations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(MAX)",
                oldMaxLength: 255);
        }
    }
}
