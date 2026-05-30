using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComoGastoMinhaGrana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "cgmg",
                table: "Categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UserId_Name",
                schema: "cgmg",
                table: "Categories",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_AspNetUsers_UserId",
                schema: "cgmg",
                table: "Categories",
                column: "UserId",
                principalSchema: "cgmg",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_AspNetUsers_UserId",
                schema: "cgmg",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_UserId_Name",
                schema: "cgmg",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "cgmg",
                table: "Categories");
        }
    }
}
