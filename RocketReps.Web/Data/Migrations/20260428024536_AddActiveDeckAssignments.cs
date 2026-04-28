using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RocketReps.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveDeckAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DeckAssignments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DeckAssignments");
        }
    }
}
