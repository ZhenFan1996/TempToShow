using Microsoft.EntityFrameworkCore.Migrations;

namespace PlattformChallenge.Migrations
{
    public partial class ProblemFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowUpdate",
                table: "Solutions");

            migrationBuilder.AddColumn<bool>(
                name: "AllowOpen",
                table: "Challenges",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowOpen",
                table: "Challenges");

            migrationBuilder.AddColumn<bool>(
                name: "AllowUpdate",
                table: "Solutions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
