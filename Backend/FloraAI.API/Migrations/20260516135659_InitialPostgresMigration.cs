using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FloraAI.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConditionsDictionary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlantType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ConditionName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Treatment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    WateringAdvice = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LightAdvice = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    FertilizingAdvice = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SoilAdvice = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    HumidityAdvice = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionsDictionary", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlantLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommonName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DefaultImage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPlants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Nickname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PlantType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CurrentStatus = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SavedTreatment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SavedWateringAdvice = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SavedLightAdvice = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SavedFertilizingAdvice = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SavedSoilAdvice = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SavedHumidityAdvice = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserPlantId = table.Column<int>(type: "integer", nullable: false),
                    ConditionsDictionaryId = table.Column<int>(type: "integer", nullable: false),
                    ConditionFound = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ScanDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
