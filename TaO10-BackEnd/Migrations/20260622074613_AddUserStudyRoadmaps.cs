using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaO10_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStudyRoadmaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_study_roadmaps",
                columns: table => new
                {
                    user_study_roadmap_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_exam_attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: false),
                    strengths = table.Column<string>(type: "jsonb", nullable: false),
                    weaknesses = table.Column<string>(type: "jsonb", nullable: false),
                    weeks = table.Column<string>(type: "jsonb", nullable: false),
                    daily_time = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    next_action = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_study_roadmaps_pkey", x => x.user_study_roadmap_id);
                    table.ForeignKey(
                        name: "user_study_roadmaps_user_exam_attempt_id_fkey",
                        column: x => x.user_exam_attempt_id,
                        principalTable: "user_exam_attempts",
                        principalColumn: "user_exam_attempt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_study_roadmaps_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_study_roadmaps_user_exam_attempt_id",
                table: "user_study_roadmaps",
                column: "user_exam_attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_study_roadmaps_user_id",
                table: "user_study_roadmaps",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_study_roadmaps");
        }
    }
}
