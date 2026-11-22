using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobRadar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sources",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    base_url = table.Column<string>(type: "text", nullable: false),
                    attribution_url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "postings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    external_id = table.Column<string>(type: "text", nullable: false),
                    content_hash = table.Column<string>(type: "text", nullable: false),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    company = table.Column<string>(type: "text", nullable: true),
                    description_html = table.Column<string>(type: "text", nullable: false),
                    description_text = table.Column<string>(type: "text", nullable: false),
                    location_text = table.Column<string>(type: "text", nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    raw_payload = table.Column<string>(type: "jsonb", nullable: false),
                    remote_flag = table.Column<bool>(type: "boolean", nullable: true),
                    tags = table.Column<string[]>(type: "text[]", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    extraction_attempts = table.Column<int>(type: "integer", nullable: false),
                    last_extraction_error = table.Column<string>(type: "text", nullable: true),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    extraction_status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_postings", x => x.id);
                    table.ForeignKey(
                        name: "fk_postings_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "posting_extractions",
                columns: table => new
                {
                    posting_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prompt_version = table.Column<string>(type: "text", nullable: false),
                    model_name = table.Column<string>(type: "text", nullable: false),
                    extracted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    raw_model_output = table.Column<string>(type: "jsonb", nullable: false),
                    citizenship_or_clearance_required = table.Column<bool>(type: "boolean", nullable: true),
                    city = table.Column<string>(type: "text", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    remote_policy = table.Column<string>(type: "text", nullable: true),
                    seniority = table.Column<string>(type: "text", nullable: true),
                    required_spoken_languages = table.Column<string>(type: "jsonb", nullable: false),
                    visa_sponsorship = table.Column<bool>(type: "boolean", nullable: true),
                    salary_currency = table.Column<string>(type: "text", nullable: true),
                    salary_max = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    salary_min = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    salary_period = table.Column<string>(type: "text", nullable: true),
                    cloud_platforms = table.Column<string[]>(type: "text[]", nullable: false),
                    frameworks = table.Column<string[]>(type: "text[]", nullable: false),
                    other_skills = table.Column<string[]>(type: "text[]", nullable: false),
                    primary_languages = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posting_extractions", x => new { x.posting_id, x.prompt_version });
                    table.ForeignKey(
                        name: "fk_posting_extractions_postings_posting_id",
                        column: x => x.posting_id,
                        principalTable: "postings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "sources",
                columns: new[] { "id", "attribution_url", "base_url", "name" },
                values: new object[] { 1, "https://www.arbeitnow.com/", "https://www.arbeitnow.com/api/job-board-api", "Arbeitnow" });

            migrationBuilder.CreateIndex(
                name: "ix_postings_source_id_external_id",
                table: "postings",
                columns: new[] { "source_id", "external_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "posting_extractions");

            migrationBuilder.DropTable(
                name: "postings");

            migrationBuilder.DropTable(
                name: "sources");
        }
    }
}
