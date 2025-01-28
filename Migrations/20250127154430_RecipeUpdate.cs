using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGoldenFood.Migrations
{
    /// <inheritdoc />
    public partial class RecipeUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "Recipes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "Recipes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
