using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloraAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConditionsDictionary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ConditionName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Treatment = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CareInstructions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionsDictionary", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlantLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommonName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DefaultImage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPlants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PlantType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CurrentStatus = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SavedTreatment = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SavedCareInstructions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPlants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScanHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserPlantId = table.Column<int>(type: "int", nullable: false),
                    ConditionsDictionaryId = table.Column<int>(type: "int", nullable: false),
                    ConditionFound = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ScanDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScanHistories_ConditionsDictionary_ConditionsDictionaryId",
                        column: x => x.ConditionsDictionaryId,
                        principalTable: "ConditionsDictionary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScanHistories_UserPlants_UserPlantId",
                        column: x => x.UserPlantId,
                        principalTable: "UserPlants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConditionsDictionary_PlantType_ConditionName",
                table: "ConditionsDictionary",
                columns: new[] { "PlantType", "ConditionName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlantLookups_CommonName",
                table: "PlantLookups",
                column: "CommonName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScanHistories_ConditionsDictionaryId",
                table: "ScanHistories",
                column: "ConditionsDictionaryId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanHistories_UserPlantId",
                table: "ScanHistories",
                column: "UserPlantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPlants_UserId",
                table: "UserPlants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlantLookups");

            migrationBuilder.DropTable(
                name: "ScanHistories");

            migrationBuilder.DropTable(
                name: "ConditionsDictionary");

            migrationBuilder.DropTable(
                name: "UserPlants");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
