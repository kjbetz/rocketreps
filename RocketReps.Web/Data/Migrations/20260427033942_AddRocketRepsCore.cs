using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RocketReps.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRocketRepsCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mascot = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classrooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByTeacherId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    JoinCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RequiresTeacherApproval = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classrooms_AspNetUsers_CreatedByTeacherId",
                        column: x => x.CreatedByTeacherId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Classrooms_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Decks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerTeacherId = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    Subject = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    GradeBand = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsGlobalStock = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Decks_AspNetUsers_OwnerTeacherId",
                        column: x => x.OwnerTeacherId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Decks_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassroomMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByTeacherId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassroomMemberships_AspNetUsers_ApprovedByTeacherId",
                        column: x => x.ApprovedByTeacherId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ClassroomMemberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassroomMemberships_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeckId = table.Column<Guid>(type: "uuid", nullable: false),
                    Front = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Back = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CardType = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ChoicesJson = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CorrectAnswer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeckAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeckId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedByTeacherId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsOpenStudyAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeckAssignments_AspNetUsers_AssignedByTeacherId",
                        column: x => x.AssignedByTeacherId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeckAssignments_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeckAssignments_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentCardProgress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CardId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    LapseCount = table.Column<int>(type: "integer", nullable: false),
                    Stability = table.Column<double>(type: "double precision", nullable: true),
                    Difficulty = table.Column<double>(type: "double precision", nullable: true),
                    LastRating = table.Column<int>(type: "integer", nullable: true),
                    LastReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCardProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentCardProgress_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentCardProgress_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CardId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeckAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    WasCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    ScheduledInterval = table.Column<TimeSpan>(type: "interval", nullable: true),
                    NextDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewLogs_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewLogs_DeckAssignments_DeckAssignmentId",
                        column: x => x.DeckAssignmentId,
                        principalTable: "DeckAssignments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SchoolId",
                table: "AspNetUsers",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_DeckId_SortOrder",
                table: "Cards",
                columns: new[] { "DeckId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomMemberships_ApprovedByTeacherId",
                table: "ClassroomMemberships",
                column: "ApprovedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomMemberships_ClassroomId_UserId",
                table: "ClassroomMemberships",
                columns: new[] { "ClassroomId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomMemberships_UserId",
                table: "ClassroomMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_CreatedByTeacherId",
                table: "Classrooms",
                column: "CreatedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_JoinCode",
                table: "Classrooms",
                column: "JoinCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_SchoolId_Name",
                table: "Classrooms",
                columns: new[] { "SchoolId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_DeckAssignments_AssignedByTeacherId",
                table: "DeckAssignments",
                column: "AssignedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckAssignments_ClassroomId_DeckId",
                table: "DeckAssignments",
                columns: new[] { "ClassroomId", "DeckId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeckAssignments_DeckId",
                table: "DeckAssignments",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_IsGlobalStock_Subject",
                table: "Decks",
                columns: new[] { "IsGlobalStock", "Subject" });

            migrationBuilder.CreateIndex(
                name: "IX_Decks_OwnerTeacherId",
                table: "Decks",
                column: "OwnerTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_SchoolId",
                table: "Decks",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLogs_CardId",
                table: "ReviewLogs",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLogs_DeckAssignmentId",
                table: "ReviewLogs",
                column: "DeckAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLogs_UserId_ReviewedAt",
                table: "ReviewLogs",
                columns: new[] { "UserId", "ReviewedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Schools_Name",
                table: "Schools",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentCardProgress_CardId",
                table: "StudentCardProgress",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCardProgress_DueAt",
                table: "StudentCardProgress",
                column: "DueAt");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCardProgress_UserId_CardId",
                table: "StudentCardProgress",
                columns: new[] { "UserId", "CardId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Schools_SchoolId",
                table: "AspNetUsers",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Schools_SchoolId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ClassroomMemberships");

            migrationBuilder.DropTable(
                name: "ReviewLogs");

            migrationBuilder.DropTable(
                name: "StudentCardProgress");

            migrationBuilder.DropTable(
                name: "DeckAssignments");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Classrooms");

            migrationBuilder.DropTable(
                name: "Decks");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SchoolId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "AspNetUsers");
        }
    }
}
