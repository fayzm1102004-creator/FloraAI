using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloraAI.API.Migrations
{
    /// <inheritdoc />
    public partial class StructuredCareAdvice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SavedCareInstructions",
                table: "UserPlants",
                newName: "SavedWateringAdvice");

            migrationBuilder.RenameColumn(
                name: "CareInstructions",
                table: "ConditionsDictionary",
                newName: "WateringAdvice");

            migrationBuilder.AddColumn<string>(
                name: "SavedFertilizingAdvice",
                table: "UserPlants",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SavedHumidityAdvice",
                table: "UserPlants",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SavedLightAdvice",
                table: "UserPlants",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SavedSoilAdvice",
                table: "UserPlants",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FertilizingAdvice",
                table: "ConditionsDictionary",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HumidityAdvice",
                table: "ConditionsDictionary",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LightAdvice",
                table: "ConditionsDictionary",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoilAdvice",
                table: "ConditionsDictionary",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SavedFertilizingAdvice",
                table: "UserPlants");

            migrationBuilder.DropColumn(
                name: "SavedHumidityAdvice",
                table: "UserPlants");

            migrationBuilder.DropColumn(
                name: "SavedLightAdvice",
                table: "UserPlants");

            migrationBuilder.DropColumn(
                name: "SavedSoilAdvice",
                table: "UserPlants");

            migrationBuilder.DropColumn(
                name: "FertilizingAdvice",
                table: "ConditionsDictionary");

            migrationBuilder.DropColumn(
                name: "HumidityAdvice",
                table: "ConditionsDictionary");

            migrationBuilder.DropColumn(
                name: "LightAdvice",
                table: "ConditionsDictionary");

            migrationBuilder.DropColumn(
                name: "SoilAdvice",
                table: "ConditionsDictionary");

            migrationBuilder.RenameColumn(
                name: "SavedWateringAdvice",
                table: "UserPlants",
                newName: "SavedCareInstructions");

            migrationBuilder.RenameColumn(
                name: "WateringAdvice",
                table: "ConditionsDictionary",
                newName: "CareInstructions");
        }
    }
}
