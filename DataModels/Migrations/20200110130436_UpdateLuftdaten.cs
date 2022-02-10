using Microsoft.EntityFrameworkCore.Migrations;

namespace DataModels.Migrations
{
    public partial class UpdateLuftdaten : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "PM_2_5",
                table: "LuftdatenStation",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<float>(
                name: "PM_10",
                table: "LuftdatenStation",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<float>(
                name: "PM_2_5",
                table: "LatestLuftdaten",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<float>(
                name: "PM_10",
                table: "LatestLuftdaten",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PM_2_5",
                table: "LuftdatenStation",
                type: "integer",
                nullable: false,
                oldClrType: typeof(float));

            migrationBuilder.AlterColumn<int>(
                name: "PM_10",
                table: "LuftdatenStation",
                type: "integer",
                nullable: false,
                oldClrType: typeof(float));

            migrationBuilder.AlterColumn<int>(
                name: "PM_2_5",
                table: "LatestLuftdaten",
                type: "integer",
                nullable: false,
                oldClrType: typeof(float));

            migrationBuilder.AlterColumn<int>(
                name: "PM_10",
                table: "LatestLuftdaten",
                type: "integer",
                nullable: false,
                oldClrType: typeof(float));
        }
    }
}
