using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitraLife.Migrations
{
    /// <inheritdoc />
    public partial class AddUserGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StepGoal",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkoutMinutesGoal",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StepGoal",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WorkoutMinutesGoal",
                table: "AspNetUsers");
        }
    }
}
