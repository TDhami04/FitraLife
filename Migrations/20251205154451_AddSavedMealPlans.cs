using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitraLife.Migrations
{
    /// <inheritdoc />
    public partial class AddSavedMealPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavedMealPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    DietType = table.Column<string>(type: "TEXT", nullable: false),
                    TargetCalories = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedMealPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedMealItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Calories = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    SavedMealPlanId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedMealItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedMealItems_SavedMealPlans_SavedMealPlanId",
                        column: x => x.SavedMealPlanId,
                        principalTable: "SavedMealPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedMealItems_SavedMealPlanId",
                table: "SavedMealItems",
                column: "SavedMealPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedMealItems");

            migrationBuilder.DropTable(
                name: "SavedMealPlans");
        }
    }
}
