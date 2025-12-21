using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitraLife.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetWeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TargetWeight",
                table: "AspNetUsers",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetWeight",
                table: "AspNetUsers");
        }
    }
}
