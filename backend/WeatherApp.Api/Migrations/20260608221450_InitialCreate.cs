using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        /// <summary>
        /// Ele contem as instruções para criar tabelas, adicionar colunas, criar índices, etc.
        /// </summary>
        /// <param name="pMigrationBuilder">Objeto builder</param>
        protected override void Up(MigrationBuilder pMigrationBuilder)
        {
            pMigrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            pMigrationBuilder.CreateTable(
                name: "TemperatureRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Temperature = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    FeelsLike = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    Humidity = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemperatureRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemperatureRecords_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            pMigrationBuilder.CreateIndex(
                name: "IX_Cities_Latitude_Longitude",
                table: "Cities",
                columns: new[] { "Latitude", "Longitude" });

            pMigrationBuilder.CreateIndex(
                name: "IX_Cities_Name",
                table: "Cities",
                column: "Name");

            pMigrationBuilder.CreateIndex(
                name: "IX_TemperatureRecords_CityId_RecordedAt",
                table: "TemperatureRecords",
                columns: new[] { "CityId", "RecordedAt" });

            pMigrationBuilder.CreateIndex(
                name: "IX_TemperatureRecords_RecordedAt",
                table: "TemperatureRecords",
                column: "RecordedAt");
        }

        /// <inheritdoc />
        /// <summary>
        /// Usado quando quer voltar o banco para o estado anterior
        /// </summary>
        /// <param name="pMigrationBuilder">Objeto builder</param>
        protected override void Down(MigrationBuilder pMigrationBuilder)
        {
            pMigrationBuilder.DropTable(
                name: "TemperatureRecords");

            pMigrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
