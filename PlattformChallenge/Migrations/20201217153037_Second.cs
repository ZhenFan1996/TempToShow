using Microsoft.EntityFrameworkCore.Migrations;

namespace PlattformChallenge.Migrations
{
    public partial class Second : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Best_Solution",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "Winner",
                table: "Challenges");

            migrationBuilder.AddColumn<int>(
                name: "Best_SolutionS_Id",
                table: "Challenges",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WinnerUser_Id",
                table: "Challenges",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_Best_SolutionS_Id",
                table: "Challenges",
                column: "Best_SolutionS_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_WinnerUser_Id",
                table: "Challenges",
                column: "WinnerUser_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Solutions_Best_SolutionS_Id",
                table: "Challenges",
                column: "Best_SolutionS_Id",
                principalTable: "Solutions",
                principalColumn: "S_Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_UserAccounts_WinnerUser_Id",
                table: "Challenges",
                column: "WinnerUser_Id",
                principalTable: "UserAccounts",
                principalColumn: "User_Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Solutions_Best_SolutionS_Id",
                table: "Challenges");

            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_UserAccounts_WinnerUser_Id",
                table: "Challenges");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_Best_SolutionS_Id",
                table: "Challenges");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_WinnerUser_Id",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "Best_SolutionS_Id",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "WinnerUser_Id",
                table: "Challenges");

            migrationBuilder.AddColumn<int>(
                name: "Best_Solution",
                table: "Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Winner",
                table: "Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
