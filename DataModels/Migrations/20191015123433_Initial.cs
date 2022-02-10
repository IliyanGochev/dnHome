using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DataModels.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Boiler",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(nullable: false),
                    GreykoTimestamp = table.Column<DateTime>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    Errors = table.Column<int>(nullable: false),
                    SetTemperature = table.Column<short>(nullable: false),
                    CurrentTemperature = table.Column<short>(nullable: false),
                    Flame = table.Column<short>(nullable: false),
                    Heather = table.Column<bool>(nullable: false),
                    DHW = table.Column<short>(nullable: false),
                    CHPump = table.Column<bool>(nullable: false),
                    BF = table.Column<bool>(nullable: false),
                    FF = table.Column<bool>(nullable: false),
                    Fan = table.Column<short>(nullable: false),
                    Power = table.Column<int>(nullable: false),
                    ThermostatStop = table.Column<bool>(nullable: false),
                    FFWorkTime = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boiler", x => x.Timestamp);
                });

            migrationBuilder.CreateTable(
                name: "DHW",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Temperature = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DHW", x => x.Timestamp);
                });

            migrationBuilder.CreateTable(
                name: "LatestBoiler",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    GreykoTimestamp = table.Column<DateTime>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    Errors = table.Column<int>(nullable: false),
                    SetTemperature = table.Column<short>(nullable: false),
                    CurrentTemperature = table.Column<short>(nullable: false),
                    Flame = table.Column<short>(nullable: false),
                    Heather = table.Column<bool>(nullable: false),
                    DHW = table.Column<short>(nullable: false),
                    CHPump = table.Column<bool>(nullable: false),
                    BF = table.Column<bool>(nullable: false),
                    FF = table.Column<bool>(nullable: false),
                    Fan = table.Column<short>(nullable: false),
                    Power = table.Column<int>(nullable: false),
                    ThermostatStop = table.Column<bool>(nullable: false),
                    FFWorkTime = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LatestBoiler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LatestDHW",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Temperature = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LatestDHW", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LatestLuftdaten",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Temperature = table.Column<decimal>(nullable: false),
                    Humidity = table.Column<decimal>(nullable: false),
                    Pressure = table.Column<decimal>(nullable: false),
                    PM_2_5 = table.Column<int>(nullable: false),
                    PM_10 = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LatestLuftdaten", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LuftdatenStation",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Temperature = table.Column<decimal>(nullable: false),
                    Humidity = table.Column<decimal>(nullable: false),
                    Pressure = table.Column<decimal>(nullable: false),
                    PM_2_5 = table.Column<int>(nullable: false),
                    PM_10 = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LuftdatenStation", x => x.Timestamp);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Boiler");

            migrationBuilder.DropTable(
                name: "DHW");

            migrationBuilder.DropTable(
                name: "LatestBoiler");

            migrationBuilder.DropTable(
                name: "LatestDHW");

            migrationBuilder.DropTable(
                name: "LatestLuftdaten");

            migrationBuilder.DropTable(
                name: "LuftdatenStation");
        }
    }
}
