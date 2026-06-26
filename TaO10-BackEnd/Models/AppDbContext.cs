using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.Helpers;

namespace TaO10_BackEnd.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BlogPost> BlogPosts { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<ForumCategory> ForumCategories { get; set; }

    public virtual DbSet<ForumReply> ForumReplies { get; set; }

    public virtual DbSet<ForumThread> ForumThreads { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<PackageExam> PackageExams { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }

    public virtual DbSet<UserExamAttempt> UserExamAttempts { get; set; }

    public virtual DbSet<UserPackage> UserPackages { get; set; }

    public virtual DbSet<UserProgress> UserProgresses { get; set; }
    public virtual DbSet<UserStudyRoadmap> UserStudyRoadmaps { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory());
        builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        builder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
        builder.AddEnvironmentVariables();
        var configuration = builder.Build();
        optionsBuilder.UseNpgsql(DatabaseConnectionString.Get(configuration));

    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.BlogPostId).HasName("blog_posts_pkey");

            entity.ToTable("blog_posts");

            entity.Property(e => e.BlogPostId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("blog_post_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Meta).HasColumnName("meta");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Title)
                .HasMaxLength(500)
                .HasColumnName("title");
            entity.Property(e => e.ViewsCount).HasColumnName("views_count");

            entity.HasOne(d => d.Status).WithMany(p => p.BlogPosts)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("blog_posts_status_id_fkey");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.ExamId).HasName("exams_pkey");

            entity.ToTable("exams");

            entity.Property(e => e.ExamId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("exam_id");
            entity.Property(e => e.AttemptsCount).HasColumnName("attempts_count");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationTime).HasColumnName("duration_time");
            entity.Property(e => e.ExamType)
                .HasMaxLength(100)
                .HasColumnName("exam_type");
            entity.Property(e => e.Level)
                .HasMaxLength(10)
                .HasColumnName("level");
            entity.Property(e => e.QuestionsCount).HasColumnName("questions_count");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Title)
                .HasMaxLength(500)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.ViewsCount).HasColumnName("views_count");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.Status).WithMany(p => p.Exams)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("exams_status_id_fkey");
        });

        modelBuilder.Entity<ForumCategory>(entity =>
        {
            entity.HasKey(e => e.ForumCategoryId).HasName("forum_categories_pkey");

            entity.ToTable("forum_categories");

            entity.Property(e => e.ForumCategoryId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("forum_category_id");
            entity.Property(e => e.Badge)
                .HasMaxLength(100)
                .HasColumnName("badge");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.RepliesCount).HasColumnName("replies_count");
            entity.Property(e => e.ThreadsCount).HasColumnName("threads_count");
        });

        modelBuilder.Entity<ForumReply>(entity =>
        {
            entity.HasKey(e => e.ForumReplyId).HasName("forum_replies_pkey");

            entity.ToTable("forum_replies");

            entity.Property(e => e.ForumReplyId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("forum_reply_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ForumThreadId).HasColumnName("forum_thread_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ForumThread).WithMany(p => p.ForumReplies)
                .HasForeignKey(d => d.ForumThreadId)
                .HasConstraintName("forum_replies_forum_thread_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ForumReplies)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("forum_replies_user_id_fkey");
        });

        modelBuilder.Entity<ForumThread>(entity =>
        {
            entity.HasKey(e => e.ForumThreadId).HasName("forum_threads_pkey");

            entity.ToTable("forum_threads");

            entity.Property(e => e.ForumThreadId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("forum_thread_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Excerpt).HasColumnName("excerpt");
            entity.Property(e => e.ForumCategoryId).HasColumnName("forum_category_id");
            entity.Property(e => e.IsHot)
                .HasDefaultValue(false)
                .HasColumnName("is_hot");
            entity.Property(e => e.IsPinned)
                .HasDefaultValue(false)
                .HasColumnName("is_pinned");
            entity.Property(e => e.RepliesCount).HasColumnName("replies_count");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Tags)
                .HasColumnType("jsonb")
                .HasColumnName("tags");
            entity.Property(e => e.Title)
                .HasMaxLength(500)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ViewsCount).HasColumnName("views_count");

            entity.HasOne(d => d.ForumCategory).WithMany(p => p.ForumThreads)
                .HasForeignKey(d => d.ForumCategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("forum_threads_forum_category_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.ForumThreads)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("forum_threads_status_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ForumThreads)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("forum_threads_user_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("notifications_pkey");

            entity.ToTable("notifications");

            entity.Property(e => e.NotificationId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("notification_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("notifications_user_id_fkey");
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("packages_pkey");

            entity.ToTable("packages");

            entity.Property(e => e.PackageId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("package_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationTime).HasColumnName("duration_time");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Status).WithMany(p => p.Packages)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("packages_status_id_fkey");
        });

        modelBuilder.Entity<PackageExam>(entity =>
        {
            entity.HasKey(e => e.PackageExamId).HasName("package_exams_pkey");

            entity.ToTable("package_exams");

            entity.HasIndex(e => new { e.PackageId, e.ExamId }, "package_exams_package_id_exam_id_key").IsUnique();

            entity.Property(e => e.PackageExamId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("package_exam_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.PackageId).HasColumnName("package_id");

            entity.HasOne(d => d.Exam).WithMany(p => p.PackageExams)
                .HasForeignKey(d => d.ExamId)
                .HasConstraintName("package_exams_exam_id_fkey");

            entity.HasOne(d => d.Package).WithMany(p => p.PackageExams)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("package_exams_package_id_fkey");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.ResetTokenId).HasName("password_reset_tokens_pkey");

            entity.ToTable("password_reset_tokens");

            entity.HasIndex(e => new { e.UserId, e.OtpCode }, "uk_user_otp").IsUnique();

            entity.Property(e => e.ResetTokenId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("reset_token_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiryTime).HasColumnName("expiry_time");
            entity.Property(e => e.IsUsed)
                .HasDefaultValue(false)
                .HasColumnName("is_used");
            entity.Property(e => e.OtpCode)
                .HasMaxLength(500)
                .HasColumnName("otp_code");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("password_reset_tokens_user_id_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.Property(e => e.PaymentId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("payment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpectedAmount).HasColumnName("expected_amount");
            entity.Property(e => e.ExpiredAt).HasColumnName("expired_at");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.ReceivedAmount).HasColumnName("received_amount");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TransactionCode)
                .HasMaxLength(100)
                .HasColumnName("transaction_code");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Package).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("payments_package_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Payments)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payments_status_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("payments_user_id_fkey");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("questions_pkey");

            entity.ToTable("questions");

            entity.Property(e => e.QuestionId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("question_id");
            entity.Property(e => e.CorrectAnswer)
                .HasMaxLength(10)
                .HasColumnName("correct_answer");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.Explanation).HasColumnName("explanation");
            entity.Property(e => e.OptionA).HasColumnName("option_a");
            entity.Property(e => e.OptionB).HasColumnName("option_b");
            entity.Property(e => e.OptionC).HasColumnName("option_c");
            entity.Property(e => e.OptionD).HasColumnName("option_d");
            entity.Property(e => e.Points)
                .HasPrecision(3, 1)
                .HasColumnName("points");
            entity.Property(e => e.QuestionNumber).HasColumnName("question_number");
            entity.Property(e => e.QuestionText).HasColumnName("question_text");
            entity.Property(e => e.Section)
                .HasMaxLength(100)
                .HasColumnName("section");
            entity.Property(e => e.StatusId).HasColumnName("status_id");

            entity.HasOne(d => d.Exam).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("questions_exam_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Questions)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("questions_status_id_fkey");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("refresh_tokens_pkey");

            entity.ToTable("refresh_tokens");

            entity.Property(e => e.RefreshTokenId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("refresh_token_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IsRevoked)
                .HasDefaultValue(false)
                .HasColumnName("is_revoked");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("refresh_tokens_user_id_fkey");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("statuses_pkey");

            entity.ToTable("statuses");

            entity.HasIndex(e => new { e.EntityType, e.Code }, "statuses_entity_type_code_key").IsUnique();

            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("status_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(100)
                .HasColumnName("display_name");
            entity.Property(e => e.EntityType)
                .HasMaxLength(50)
                .HasColumnName("entity_type");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Phone, "users_phone_key").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("user_id");
            entity.Property(e => e.Avatar)
                .HasMaxLength(500)
                .HasColumnName("avatar");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TotalExams).HasColumnName("total_exams");
            entity.Property(e => e.TotalScore).HasColumnName("total_score");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Status).WithMany(p => p.Users)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_status_id_fkey");
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.UserAnswerId).HasName("user_answers_pkey");

            entity.ToTable("user_answers");

            entity.Property(e => e.UserAnswerId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("user_answer_id");
            entity.Property(e => e.AnsweredAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("answered_at");
            entity.Property(e => e.IsCorrect).HasColumnName("is_correct");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.UserAnswer1)
                .HasMaxLength(1)
                .HasColumnName("user_answer");
            entity.Property(e => e.UserExamAttemptId).HasColumnName("user_exam_attempt_id");

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_answers_question_id_fkey");

            entity.HasOne(d => d.UserExamAttempt).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.UserExamAttemptId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_answers_user_exam_attempt_id_fkey");
        });

        modelBuilder.Entity<UserExamAttempt>(entity =>
        {
            entity.HasKey(e => e.UserExamAttemptId).HasName("user_exam_attempts_pkey");

            entity.ToTable("user_exam_attempts");

            entity.Property(e => e.UserExamAttemptId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("user_exam_attempt_id");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CorrectAnswers).HasColumnName("correct_answers");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.Score)
                .HasPrecision(5, 2)
                .HasColumnName("score");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("started_at");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TimeSpentMinutes).HasColumnName("time_spent_minutes");
            entity.Property(e => e.TotalQuestions).HasColumnName("total_questions");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Exam).WithMany(p => p.UserExamAttempts)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_exam_attempts_exam_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.UserExamAttempts)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_exam_attempts_status_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserExamAttempts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_exam_attempts_user_id_fkey");
        });

        modelBuilder.Entity<UserPackage>(entity =>
        {
            entity.HasKey(e => e.UserPackageId).HasName("user_packages_pkey");

            entity.ToTable("user_packages");

            entity.Property(e => e.UserPackageId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("user_package_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("start_date");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Package).WithMany(p => p.UserPackages)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_packages_package_id_fkey");

            entity.HasOne(d => d.Payment).WithMany(p => p.UserPackages)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_packages_payment_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.UserPackages)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_packages_status_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserPackages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_packages_user_id_fkey");
        });

        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.HasKey(e => e.UserProgressId).HasName("user_progress_pkey");

            entity.ToTable("user_progress");

            entity.HasIndex(e => new { e.UserId, e.ItemType, e.ItemId }, "user_progress_user_id_item_type_item_id_key").IsUnique();

            entity.Property(e => e.UserProgressId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("user_progress_id");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ItemType)
                .HasMaxLength(50)
                .HasColumnName("item_type");
            entity.Property(e => e.LastAccessed)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("last_accessed");
            entity.Property(e => e.ProgressPercentage).HasColumnName("progress_percentage");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserProgresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_progress_user_id_fkey");
        });
        modelBuilder.Entity<UserStudyRoadmap>(entity =>
        {
            entity.HasKey(e => e.UserStudyRoadmapId).HasName("user_study_roadmaps_pkey");

            entity.ToTable("user_study_roadmaps");

            entity.HasIndex(e => e.UserId, "ix_user_study_roadmaps_user_id");

            entity.Property(e => e.UserStudyRoadmapId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("user_study_roadmap_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DailyTime)
                .HasMaxLength(100)
                .HasColumnName("daily_time");
            entity.Property(e => e.NextAction)
                .HasMaxLength(500)
                .HasColumnName("next_action");
            entity.Property(e => e.Strengths)
                .HasColumnType("jsonb")
                .HasColumnName("strengths");
            entity.Property(e => e.Summary).HasColumnName("summary");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserExamAttemptId).HasColumnName("user_exam_attempt_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Weaknesses)
                .HasColumnType("jsonb")
                .HasColumnName("weaknesses");
            entity.Property(e => e.Weeks)
                .HasColumnType("jsonb")
                .HasColumnName("weeks");

            entity.HasOne(d => d.User).WithMany(p => p.UserStudyRoadmaps)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_study_roadmaps_user_id_fkey");

            entity.HasOne(d => d.UserExamAttempt).WithMany()
                .HasForeignKey(d => d.UserExamAttemptId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_study_roadmaps_user_exam_attempt_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
