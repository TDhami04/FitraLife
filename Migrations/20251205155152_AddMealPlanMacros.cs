using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitraLife.Migrations
{
    /// <inheritdoc />
    public partial class AddMealPlanMacros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Carbs",
                table: "SavedMealPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Fats",
                table: "SavedMealPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Protein",
                table: "SavedMealPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalCalories",
                table: "SavedMealPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Carbs",
                table: "SavedMealPlans");

            migrationBuilder.DropColumn(
                name: "Fats",
                table: "SavedMealPlans");

            migrationBuilder.DropColumn(
                name: "Protein",
                table: "SavedMealPlans");

            migrationBuilder.DropColumn(
                name: "TotalCalories",
                table: "SavedMealPlans");
        }
    }
}
