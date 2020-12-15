using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlattformChallenge.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Language_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DevelopmentLanguage = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Language_Id);
                });

            migrationBuilder.CreateTable(
                name: "Solutions",
                columns: table => new
                {
                    S_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    URL = table.Column<string>(nullable: false),
                    Status = table.Column<string>(nullable: false),
                    Submit_Date = table.Column<DateTime>(nullable: false),
                    Point = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solutions", x => x.S_Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    User_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(nullable: false),
                    Password = table.Column<string>(nullable: false),
                    AccountTyp = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IsActiv = table.Column<bool>(nullable: true),
                    Logo = table.Column<string>(nullable: true),
                    Firstname = table.Column<string>(nullable: true),
                    Surname = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.User_Id);
                });

            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    C_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Bonus = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    Release_Date = table.Column<DateTime>(nullable: false),
                    Max_Participant = table.Column<int>(nullable: false),
                    Winner = table.Column<int>(nullable: false),
                    Best_Solution = table.Column<int>(nullable: false),
                    Com_ID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.C_Id);
                    table.ForeignKey(
                        name: "FK_Challenges_UserAccounts_Com_ID",
                        column: x => x.Com_ID,
                        principalTable: "UserAccounts",
                        principalColumn: "User_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LanguageChallenge",
                columns: table => new
                {
                    Language_Id = table.Column<int>(nullable: false),
                    C_Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageChallenge", x => new { x.Language_Id, x.C_Id });
                    table.ForeignKey(
                        name: "FK_LanguageChallenge_Challenges_C_Id",
                        column: x => x.C_Id,
                        principalTable: "Challenges",
                        principalColumn: "C_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LanguageChallenge_Languages_Language_Id",
                        column: x => x.Language_Id,
                        principalTable: "Languages",
                        principalColumn: "Language_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Participations",
                columns: table => new
                {
                    C_Id = table.Column<int>(nullable: false),
                    P_Id = table.Column<int>(nullable: false),
                    S_Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participations", x => new { x.C_Id, x.P_Id });
                    table.ForeignKey(
                        name: "FK_Participations_Challenges_C_Id",
                        column: x => x.C_Id,
                        principalTable: "Challenges",
                        principalColumn: "C_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participations_UserAccounts_P_Id",
                        column: x => x.P_Id,
                        principalTable: "UserAccounts",
                        principalColumn: "User_Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Participations_Solutions_S_Id",
                        column: x => x.S_Id,
                        principalTable: "Solutions",
                        principalColumn: "S_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_Com_ID",
                table: "Challenges",
                column: "Com_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageChallenge_C_Id",
                table: "LanguageChallenge",
                column: "C_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Participations_P_Id",
                table: "Participations",
                column: "P_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Participations_S_Id",
                table: "Participations",
                column: "S_Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_Email",
                table: "UserAccounts",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LanguageChallenge");

            migrationBuilder.DropTable(
                name: "Participations");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "Challenges");

            migrationBuilder.DropTable(
                name: "Solutions");

            migrationBuilder.DropTable(
                name: "UserAccounts");
        }
    }
}
