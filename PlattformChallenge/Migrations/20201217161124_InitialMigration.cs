using Microsoft.EntityFrameworkCore.Migrations;

namespace PlattformChallenge.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participations_Solutions_S_Id",
                table: "Participations");

            migrationBuilder.DropIndex(
                name: "IX_Participations_S_Id",
                table: "Participations");

            migrationBuilder.DropColumn(
                name: "S_Id",
                table: "Participations");

            migrationBuilder.AddColumn<int>(
                name: "C_ID",
                table: "Solutions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "P_ID",
                table: "Solutions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_P_ID_C_ID",
                table: "Solutions",
                columns: new[] { "P_ID", "C_ID" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Solutions_Participations_P_ID_C_ID",
                table: "Solutions",
                columns: new[] { "P_ID", "C_ID" },
                principalTable: "Participations",
                principalColumns: new[] { "C_Id", "P_Id" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_Participations_P_ID_C_ID",
                table: "Solutions");

            migrationBuilder.DropIndex(
                name: "IX_Solutions_P_ID_C_ID",
                table: "Solutions");

            migrationBuilder.DropColumn(
                name: "C_ID",
                table: "Solutions");

            migrationBuilder.DropColumn(
                name: "P_ID",
                table: "Solutions");

            migrationBuilder.AddColumn<int>(
                name: "S_Id",
                table: "Participations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Participations_S_Id",
                table: "Participations",
                column: "S_Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Participations_Solutions_S_Id",
                table: "Participations",
                column: "S_Id",
                principalTable: "Solutions",
                principalColumn: "S_Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
