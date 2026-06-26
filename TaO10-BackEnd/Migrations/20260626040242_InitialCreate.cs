using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaO10_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "forum_categories",
                columns: table => new
                {
                    forum_category_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    threads_count = table.Column<int>(type: "integer", nullable: true),
                    replies_count = table.Column<int>(type: "integer", nullable: true),
                    badge = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("forum_categories_pkey", x => x.forum_category_id);
                });

            migrationBuilder.CreateTable(
                name: "statuses",
                columns: table => new
                {
                    status_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("statuses_pkey", x => x.status_id);
                });

            migrationBuilder.CreateTable(
                name: "blog_posts",
                columns: table => new
                {
                    blog_post_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    meta = table.Column<string>(type: "text", nullable: true),
                    views_count = table.Column<int>(type: "integer", nullable: true),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("blog_posts_pkey", x => x.blog_post_id);
                    table.ForeignKey(
                        name: "blog_posts_status_id_fkey",
                        column: x => x.status_id,
                        principalTable: "statuses",
                        principalColumn: "status_id");
                });

            migrationBuilder.CreateTable(
                name: "exams",
                columns: table => new
                {
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    questions_count = table.Column<int>(type: "integer", nullable: true),
                    duration_time = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    year = table.Column<int>(type: "integer", nullable: true),
                    exam_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    views_count = table.Column<int>(type: "integer", nullable: true),
                    attempts_count = table.Column<int>(type: "integer", nullable: true),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("exams_pkey", x => x.exam_id);
                    table.ForeignKey(
                        name: "exams_status_id_fkey",
                        column: x => x.status_id,
                        principalTable: "statuses",
                        principalColumn: "status_id");
                });

            migrationBuilder.CreateTable(
                name: "packages",
                columns: table => new
                {
                    package_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<int>(type: "integer", nullable: false),
                    duration_time = table.Column<int>(type: "integer", nullable: true),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("packages_pkey", x => x.package_id);
                    table.ForeignKey(
                        name: "packages_status_id_fkey",
                        column: x => x.status_id,
                        principalTable: "statuses",
                        principalColumn: "status_id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    avatar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    total_score = table.Column<int>(type: "integer", nullable: true),
                    total_exams = table.Column<int>(type: "integer", nullable: true),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.user_id);
                    table.ForeignKey(
                        name: "users_status_id_fkey",
                        column: x => x.status_id,
                        principalTable: "statuses",
                        principalColumn: "status_id");
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    question_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: true),
                    question_number = table.Column<int>(type: "integer", nullable: false),
                    section = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    question_text = table.Column<string>(type: "text", nullable: false),
                    option_a = table.Column<string>(type: "text", nullable: true),
                    option_b = table.Column<string>(type: "text", nullable: true),
                    option_c = table.Column<string>(type: "text", nullable: true),
                    option_d = table.Column<string>(type: "text", nullable: true),
                    correct_answer = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    explanation = table.Column<string>(type: "text", nullable: true),
                    points = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("questions_pkey", x => x.question_id);
                    table.ForeignKey(
                        name: "questions_exam_id_fkey",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "exam_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "questions_status_id_fkey",
                        column: x => x.status_id,
                        principalTable: "statuses",
                        principalColumn: "status_id");
                });

            migrationBuilder.CreateTable(
                name: "package_exams",
                columns: table => new
                {
                    package_exam_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    package_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("package_exams_pkey", x => x.package_exam_id);
                    table.ForeignKey(
                        name: "package_exams_exam_id_fkey",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "exam_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "package_exams_package_id_fkey",
                        column: x => x.package_id,
                        principalTable: "packages",
                        principalColumn: "package_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forum_threads",
                columns: table => new
                {
                    forum_thread_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    forum_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    excerpt = table.Column<string>(type: "text", nullable: true),
                    is_pinned = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    is_hot = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    tags = table.Column<string>(type: "jsonb", nullable: true),
                    views_count = table.Column<int>(type: "integer", nullable: true),
                    replies_count = table.Column<int>(type: "integer", nullable: true),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("forum_threads_pkey", x => x.forum_thread_id);
                    table.ForeignKey(
                        name: "forum_threads_forum_category_id_fkey",
                        column: x => x.forum_category_id,
                        principalTable: "forum_categories",
                        principalColumn: "forum_category_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "forum_threads_status_id_fkey",
                        column: x => x.status_id,
                        principalTable: "statuses",
                        principalColumn: "status_id");
                    table.ForeignKey(
                        name: "forum_threads_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("notifications_pkey", x => x.notification_id);
                    table.ForeignKey(
                        name: "notifications_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    reset_token_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    otp_code = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expiry_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("password_reset_tokens_pkey", x => x.reset_token_id);
                    table.ForeignKey(
                        name: "password_reset_tokens_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    package_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expected_amount = table.Column<int>(type: "integer", nullable: false),
                    received_amount = table.Column<int>(type: "integer", nullable: true),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("payments_pkey", x => x.payment_id);
                    table.ForeignKey(
                        name: "payments_package_id_fkey",
                        column: x => x.package_id,
                        principalTable: "packages",
                        principalColumn: "package_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "payments_status_id_fkey",
                        column: x => x.status_id,
                        principalTable: "statuses",
                        principalColumn: "status_id");
                    table.ForeignKey(
                        name: "payments_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    refresh_token_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("refresh_tokens_pkey", x => x.refresh_token_id);
                    table.ForeignKey(
                        name: "refresh_tokens_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_exam_attempts",
                columns: table => new
                {
                    user_exam_attempt_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    correct_answers = table.Column<int>(type: "integer", nullable: true),
                    total_questions = table.Column<int>(type: "integer", nullable: true),
                    time_spent_minutes = table.Column<int>(type: "integer", nullable: true),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_exam_attempts_pkey", x => x.user_exam_attempt_id);
                    table.ForeignKey(
                        name: "user_exam_attempts_exam_id_fkey",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "exam_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "user_exam_attempts_status_id_fkey",
                        column: x => x.status_id,
                        principalTable: "statuses",
                        principalColumn: "status_id");
                    table.ForeignKey(
                        name: "user_exam_attempts_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_progress",
                columns: table => new
                {
                    user_progress_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    progress_percentage = table.Column<int>(type: "integer", nullable: true),
                    last_accessed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_progress_pkey", x => x.user_progress_id);
                    table.ForeignKey(
                        name: "user_progress_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "forum_replies",
                columns: table => new
                {
                    forum_reply_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    forum_thread_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("forum_replies_pkey", x => x.forum_reply_id);
                    table.ForeignKey(
                        name: "forum_replies_forum_thread_id_fkey",
                        column: x => x.forum_thread_id,
                        principalTable: "forum_threads",
                        principalColumn: "forum_thread_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "forum_replies_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_packages",
                columns: table => new
                {
                    user_package_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    package_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_packages_pkey", x => x.user_package_id);
                    table.ForeignKey(
                        name: "user_packages_package_id_fkey",
                        column: x => x.package_id,
                        principalTable: "packages",
                        principalColumn: "package_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "user_packages_payment_id_fkey",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "payment_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "user_packages_status_id_fkey",
                        column: x => x.status_id,
                        principalTable: "statuses",
                        principalColumn: "status_id");
                    table.ForeignKey(
                        name: "user_packages_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_answers",
                columns: table => new
                {
                    user_answer_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    user_exam_attempt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    question_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_answer = table.Column<char>(type: "character(1)", maxLength: 1, nullable: true),
                    is_correct = table.Column<bool>(type: "boolean", nullable: true),
                    answered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_answers_pkey", x => x.user_answer_id);
                    table.ForeignKey(
                        name: "user_answers_question_id_fkey",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "user_answers_user_exam_attempt_id_fkey",
                        column: x => x.user_exam_attempt_id,
                        principalTable: "user_exam_attempts",
                        principalColumn: "user_exam_attempt_id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_blog_posts_status_id",
                table: "blog_posts",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_exams_status_id",
                table: "exams",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_forum_replies_forum_thread_id",
                table: "forum_replies",
                column: "forum_thread_id");

            migrationBuilder.CreateIndex(
                name: "IX_forum_replies_user_id",
                table: "forum_replies",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_forum_threads_forum_category_id",
                table: "forum_threads",
                column: "forum_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_forum_threads_status_id",
                table: "forum_threads",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_forum_threads_user_id",
                table: "forum_threads",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_package_exams_exam_id",
                table: "package_exams",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "package_exams_package_id_exam_id_key",
                table: "package_exams",
                columns: new[] { "package_id", "exam_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_packages_status_id",
                table: "packages",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "uk_user_otp",
                table: "password_reset_tokens",
                columns: new[] { "user_id", "otp_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_package_id",
                table: "payments",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_status_id",
                table: "payments",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_user_id",
                table: "payments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_exam_id",
                table: "questions",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_status_id",
                table: "questions",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "statuses_entity_type_code_key",
                table: "statuses",
                columns: new[] { "entity_type", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_answers_question_id",
                table: "user_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_answers_user_exam_attempt_id",
                table: "user_answers",
                column: "user_exam_attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_exam_attempts_exam_id",
                table: "user_exam_attempts",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_exam_attempts_status_id",
                table: "user_exam_attempts",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_exam_attempts_user_id",
                table: "user_exam_attempts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_packages_package_id",
                table: "user_packages",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_packages_payment_id",
                table: "user_packages",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_packages_status_id",
                table: "user_packages",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_packages_user_id",
                table: "user_packages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "user_progress_user_id_item_type_item_id_key",
                table: "user_progress",
                columns: new[] { "user_id", "item_type", "item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_study_roadmaps_user_exam_attempt_id",
                table: "user_study_roadmaps",
                column: "user_exam_attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_study_roadmaps_user_id",
                table: "user_study_roadmaps",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_status_id",
                table: "users",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "users_email_key",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "users_phone_key",
                table: "users",
                column: "phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blog_posts");

            migrationBuilder.DropTable(
                name: "forum_replies");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "package_exams");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "user_answers");

            migrationBuilder.DropTable(
                name: "user_packages");

            migrationBuilder.DropTable(
                name: "user_progress");

            migrationBuilder.DropTable(
                name: "user_study_roadmaps");

            migrationBuilder.DropTable(
                name: "forum_threads");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "user_exam_attempts");

            migrationBuilder.DropTable(
                name: "forum_categories");

            migrationBuilder.DropTable(
                name: "packages");

            migrationBuilder.DropTable(
                name: "exams");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "statuses");
        }
    }
}
