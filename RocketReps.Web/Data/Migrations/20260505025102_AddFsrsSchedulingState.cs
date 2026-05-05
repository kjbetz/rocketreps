using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RocketReps.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFsrsSchedulingState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FsrsState",
                table: "StudentCardProgress",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Learning");

            migrationBuilder.AddColumn<int>(
                name: "FsrsStep",
                table: "StudentCardProgress",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FsrsState",
                table: "StudentCardProgress");

            migrationBuilder.DropColumn(
                name: "FsrsStep",
                table: "StudentCardProgress");
        }
    }
}
