using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComoGastoMinhaGrana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorityToCategoryRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "cgmg",
                table: "CategoryRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "cgmg",
                table: "CategoryRules");
        }
    }
}
