using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RocketReps.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEarlyAccessSignups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EarlyAccessSignups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Role = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SchoolOrOrganization = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PlanInterest = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Source = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, defaultValue: "Open House"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarlyAccessSignups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EarlyAccessSignups_CreatedAt",
                table: "EarlyAccessSignups",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EarlyAccessSignups_Email",
                table: "EarlyAccessSignups",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EarlyAccessSignups");
        }
    }
}
