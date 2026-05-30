using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComoGastoMinhaGrana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reports",
                schema: "cgmg",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportStatements",
                schema: "cgmg",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatementId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportStatements", x => new { x.ReportId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_ReportStatements_FinancialStatements_StatementId",
                        column: x => x.StatementId,
                        principalSchema: "cgmg",
                        principalTable: "FinancialStatements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportStatements_Reports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "cgmg",
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId",
                schema: "cgmg",
                table: "Reports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportStatements_StatementId",
                schema: "cgmg",
                table: "ReportStatements",
                column: "StatementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportStatements",
                schema: "cgmg");

            migrationBuilder.DropTable(
                name: "Reports",
                schema: "cgmg");
        }
    }
}
