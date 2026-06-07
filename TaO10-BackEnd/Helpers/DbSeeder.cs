using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Helpers
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Ensure Database is created
            await context.Database.EnsureCreatedAsync();

            // 1. SEED STATUSES
            var requiredStatuses = new List<Status>
            {
                new Status { StatusId = Guid.NewGuid(), EntityType = "User", Code = "ACTIVE", DisplayName = "Hoạt động", Description = "Tài khoản đang hoạt động bình thường" },
                new Status { StatusId = Guid.NewGuid(), EntityType = "User", Code = "BLOCKED", DisplayName = "Bị khóa", Description = "Tài khoản bị khóa do vi phạm" },
                
                new Status { StatusId = Guid.NewGuid(), EntityType = "Package", Code = "ACTIVE", DisplayName = "Đang bán", Description = "Gói học tập đang mở bán" },
                new Status { StatusId = Guid.NewGuid(), EntityType = "Package", Code = "INACTIVE", DisplayName = "Ngừng bán", Description = "Gói học tập tạm ngừng bán" },
                
                new Status { StatusId = Guid.NewGuid(), EntityType = "Payment", Code = "PENDING", DisplayName = "Chờ thanh toán", Description = "Giao dịch đang chờ khách thanh toán" },
                new Status { StatusId = Guid.NewGuid(), EntityType = "Payment", Code = "SUCCESS", DisplayName = "Thành công", Description = "Thanh toán thành công" },
                new Status { StatusId = Guid.NewGuid(), EntityType = "Payment", Code = "FAILED", DisplayName = "Thất bại", Description = "Thanh toán thất bại hoặc hủy" },
                
                new Status { StatusId = Guid.NewGuid(), EntityType = "UserPackage", Code = "ACTIVE", DisplayName = "Đang hoạt động", Description = "Gói dịch vụ người dùng đang hoạt động" },
                new Status { StatusId = Guid.NewGuid(), EntityType = "UserPackage", Code = "EXPIRED", DisplayName = "Hết hạn", Description = "Gói dịch vụ đã hết hạn sử dụng" },
                
                new Status { StatusId = Guid.NewGuid(), EntityType = "Exam", Code = "ACTIVE", DisplayName = "Đang mở", Description = "Đề thi đang hiển thị để làm" },
                new Status { StatusId = Guid.NewGuid(), EntityType = "Exam", Code = "INACTIVE", DisplayName = "Bản nháp", Description = "Đề thi nháp, chưa công bố" },
                
                new Status { StatusId = Guid.NewGuid(), EntityType = "Question", Code = "ACTIVE", DisplayName = "Hoạt động", Description = "Câu hỏi đang sử dụng" },
                
                new Status { StatusId = Guid.NewGuid(), EntityType = "ForumThread", Code = "ACTIVE", DisplayName = "Hoạt động", Description = "Chủ đề thảo luận hoạt động bình thường" },
                new Status { StatusId = Guid.NewGuid(), EntityType = "ForumThread", Code = "LOCKED", DisplayName = "Bị khóa", Description = "Chủ đề thảo luận bị khóa" },
                
                new Status { StatusId = Guid.NewGuid(), EntityType = "BlogPost", Code = "PUBLISHED", DisplayName = "Đã xuất bản", Description = "Bài viết blog hiển thị cho người đọc" },
                new Status { StatusId = Guid.NewGuid(), EntityType = "BlogPost", Code = "DRAFT", DisplayName = "Bản nháp", Description = "Bài viết blog dạng nháp" },
                new Status { StatusId = Guid.NewGuid(), EntityType = "UserExamAttempt", Code = "ACTIVE", DisplayName = "Đang làm", Description = "Đang làm bài thi" },
                new Status { StatusId = Guid.NewGuid(), EntityType = "UserExamAttempt", Code = "COMPLETED", DisplayName = "Hoàn thành", Description = "Đã hoàn thành bài thi" }
            };

            foreach (var status in requiredStatuses)
            {
                var exists = await context.Statuses.AnyAsync(s => s.EntityType == status.EntityType && s.Code == status.Code);
                if (!exists)
                {
                    context.Statuses.Add(status);
                }
            }
            await context.SaveChangesAsync();

            // Reference Status IDs
            var activeUserStatusId = (await context.Statuses.FirstAsync(s => s.EntityType == "User" && s.Code == "ACTIVE")).StatusId;
            var activePkgStatusId = (await context.Statuses.FirstAsync(s => s.EntityType == "Package" && s.Code == "ACTIVE")).StatusId;
            var activeExamStatusId = (await context.Statuses.FirstAsync(s => s.EntityType == "Exam" && s.Code == "ACTIVE")).StatusId;
            var activeQuestionStatusId = (await context.Statuses.FirstAsync(s => s.EntityType == "Question" && s.Code == "ACTIVE")).StatusId;
            var activeThreadStatusId = (await context.Statuses.FirstAsync(s => s.EntityType == "ForumThread" && s.Code == "ACTIVE")).StatusId;
            var publishedBlogStatusId = (await context.Statuses.FirstAsync(s => s.EntityType == "BlogPost" && s.Code == "PUBLISHED")).StatusId;

          

            // 3. SEED PACKAGES (Basic, Pro, Premium)
            if (!await context.Packages.AnyAsync())
            {
                var packages = new List<Package>
                {
                    new Package
                    {
                        PackageId = Guid.NewGuid(),
                        Name = "Basic",
                        Description = "Gói cơ bản mở khóa Đề thi số 1 & Đề thi số 2.",
                        Price = 199000,
                        DurationTime = 30,
                        StatusId = activePkgStatusId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Package
                    {
                        PackageId = Guid.NewGuid(),
                        Name = "Pro",
                        Description = "Gói trung cấp mở khóa Đề thi số 1, Đề thi số 2 & Đề thi số 3.",
                        Price = 299000,
                        DurationTime = 30,
                        StatusId = activePkgStatusId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Package
                    {
                        PackageId = Guid.NewGuid(),
                        Name = "Premium",
                        Description = "Gói cao cấp mở khóa toàn bộ Đề thi số 1, Đề thi số 2, Đề thi số 3 & Đề thi số 4.",
                        Price = 399000,
                        DurationTime = 30,
                        StatusId = activePkgStatusId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                await context.Packages.AddRangeAsync(packages);
                await context.SaveChangesAsync();
            }

            // 4. SEED EXAMS & QUESTIONS (4 Exams)
            if (!await context.Exams.AnyAsync())
            {
                var exam1 = new Exam
                {
                    ExamId = Guid.NewGuid(),
                    Title = "Đề thi số 1 – Luyện thi tổng hợp Tiếng Anh vào lớp 10 năm 2026",
                    Description = "Đề thi số 1 nâng cao thuộc cấu trúc thi tuyển sinh Tiếng Anh năm 2026.",
                    QuestionsCount = 5,
                    DurationTime = 60,
                    Level = "Khó",
                    Year = 2026,
                    ExamType = "Tuyển sinh lớp 10",
                    ViewsCount = 12000,
                    AttemptsCount = 520,
                    StatusId = activeExamStatusId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var exam2 = new Exam
                {
                    ExamId = Guid.NewGuid(),
                    Title = "Đề thi số 2 – Luyện thi tổng hợp Tiếng Anh vào lớp 10 năm 2026",
                    Description = "Đề thi số 2 ôn tập các dạng bài đọc hiểu và cấu trúc ngữ pháp phổ biến.",
                    QuestionsCount = 5,
                    DurationTime = 60,
                    Level = "Khó",
                    Year = 2026,
                    ExamType = "Tuyển sinh lớp 10",
                    ViewsCount = 9800,
                    AttemptsCount = 410,
                    StatusId = activeExamStatusId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var exam3 = new Exam
                {
                    ExamId = Guid.NewGuid(),
                    Title = "Đề thi số 3 – Luyện thi tổng hợp Tiếng Anh vào lớp 10 năm 2026",
                    Description = "Đề thi số 3 kiểm tra ngữ âm, từ vựng và cụm từ động từ (Phrasal Verbs).",
                    QuestionsCount = 5,
                    DurationTime = 60,
                    Level = "Trung bình",
                    Year = 2026,
                    ExamType = "Tuyển sinh lớp 10",
                    ViewsCount = 7500,
                    AttemptsCount = 330,
                    StatusId = activeExamStatusId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var exam4 = new Exam
                {
                    ExamId = Guid.NewGuid(),
                    Title = "Đề thi số 4 – Luyện thi tổng hợp Tiếng Anh vào lớp 10 năm 2026",
                    Description = "Đề thi số 4 ôn luyện viết lại câu và tìm lỗi sai cơ bản.",
                    QuestionsCount = 5,
                    DurationTime = 60,
                    Level = "Dễ",
                    Year = 2026,
                    ExamType = "Tuyển sinh lớp 10",
                    ViewsCount = 5100,
                    AttemptsCount = 205,
                    StatusId = activeExamStatusId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Exams.AddRangeAsync(exam1, exam2, exam3, exam4);
                await context.SaveChangesAsync();

                // Load Package References
                var basicPkg = await context.Packages.FirstOrDefaultAsync(p => p.Name == "Basic");
                var proPkg = await context.Packages.FirstOrDefaultAsync(p => p.Name == "Pro");
                var premiumPkg = await context.Packages.FirstOrDefaultAsync(p => p.Name == "Premium");

                var packageExams = new List<PackageExam>();

                if (basicPkg != null)
                {
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = basicPkg.PackageId, ExamId = exam1.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = basicPkg.PackageId, ExamId = exam2.ExamId, CreatedAt = DateTime.UtcNow });
                }

                if (proPkg != null)
                {
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = proPkg.PackageId, ExamId = exam1.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = proPkg.PackageId, ExamId = exam2.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = proPkg.PackageId, ExamId = exam3.ExamId, CreatedAt = DateTime.UtcNow });
                }

                if (premiumPkg != null)
                {
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = premiumPkg.PackageId, ExamId = exam1.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = premiumPkg.PackageId, ExamId = exam2.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = premiumPkg.PackageId, ExamId = exam3.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = premiumPkg.PackageId, ExamId = exam4.ExamId, CreatedAt = DateTime.UtcNow });
                }

                await context.PackageExams.AddRangeAsync(packageExams);
                await context.SaveChangesAsync();

                // Mock Questions for Exam 1, 2, 3, 4 (5 questions per exam)
                var questions = new List<Question>();

                var examsList = new[] { exam1, exam2, exam3, exam4 };
                foreach (var ex in examsList)
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        questions.Add(new Question
                        {
                            QuestionId = Guid.NewGuid(),
                            ExamId = ex.ExamId,
                            QuestionNumber = i,
                            Section = i <= 2 ? "Grammar" : i <= 4 ? "Vocabulary" : "Reading",
                            QuestionText = $"[{ex.Title.Substring(0, 8)}] Câu hỏi số {i}: Chọn từ thích hợp: 'She usually _______ early in the morning.'",
                            OptionA = "wake up",
                            OptionB = "wakes up",
                            OptionC = "woke up",
                            OptionD = "waking up",
                            CorrectAnswer = "B",
                            Explanation = $"Đáp án đúng B. Chủ ngữ 'She' đi với động từ thêm 's/es' ở thì hiện tại đơn.",
                            Points = 2.0m,
                            StatusId = activeQuestionStatusId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await context.Questions.AddRangeAsync(questions);
                await context.SaveChangesAsync();
            }

            // 5. SEED FORUM CATEGORIES
            if (!await context.ForumCategories.AnyAsync())
            {
                var cat1 = new ForumCategory { ForumCategoryId = Guid.NewGuid(), Name = "TOEIC", Description = "Khu vực trao đổi kiến thức, tài liệu và mẹo thi TOEIC đạt điểm cao.", ThreadsCount = 1, RepliesCount = 1, Badge = "TOEIC Master", CreatedAt = DateTime.UtcNow };
                var cat2 = new ForumCategory { ForumCategoryId = Guid.NewGuid(), Name = "IELTS", Description = "Cộng đồng luyện thi IELTS 4 kỹ năng Nghe - Nói - Đọc - Viết.", ThreadsCount = 0, RepliesCount = 0, Badge = "IELTS 8.0+", CreatedAt = DateTime.UtcNow };
                var cat3 = new ForumCategory { ForumCategoryId = Guid.NewGuid(), Name = "Kinh nghiệm học tập", Description = "Chia sẻ phương pháp học tiếng Anh hiệu quả, tài liệu quý báu.", ThreadsCount = 1, RepliesCount = 0, Badge = "Kinh Nghiệm", CreatedAt = DateTime.UtcNow };

                await context.ForumCategories.AddRangeAsync(cat1, cat2, cat3);
                await context.SaveChangesAsync();
            }

            // 6. SEED BLOG POSTS
            if (!await context.BlogPosts.AnyAsync())
            {
                var blogs = new List<BlogPost>
                {
                    new BlogPost
                    {
                        BlogPostId = Guid.NewGuid(),
                        Title = "Cấu trúc đề thi Tiếng Anh vào lớp 10 năm 2026 – Những thay đổi quan trọng cần biết",
                        Content = "Kỳ thi tuyển sinh lớp 10 THPT môn Tiếng Anh năm 2026 dự kiến sẽ có một số điều chỉnh về phân bổ câu hỏi. Cụ thể, các câu hỏi ngữ pháp sẽ giảm bớt và tập trung nhiều hơn vào đọc hiểu...",
                        Meta = "📅 15/03/2026 · ⏱ 5 phút đọc · 👁 8.2k lượt xem",
                        ViewsCount = 8200,
                        StatusId = publishedBlogStatusId,
                        CreatedAt = DateTime.UtcNow,
                        PublishedAt = DateTime.UtcNow
                    },
                    new BlogPost
                    {
                        BlogPostId = Guid.NewGuid(),
                        Title = "10 mẹo làm bài thi Tiếng Anh đạt điểm cao cho học sinh lớp 9",
                        Content = "Để đạt điểm cao trong kỳ thi Tiếng Anh, học sinh cần rèn luyện phản xạ đọc đề, phân bổ thời gian hợp lý, không bỏ sót các câu hỏi trắc nghiệm dễ và luôn kiểm tra kỹ đáp án...",
                        Meta = "📅 12/03/2026 · ⏱ 7 phút đọc · 👁 6.5k lượt xem",
                        ViewsCount = 6500,
                        StatusId = publishedBlogStatusId,
                        CreatedAt = DateTime.UtcNow,
                        PublishedAt = DateTime.UtcNow
                    }
                };

                await context.BlogPosts.AddRangeAsync(blogs);
                await context.SaveChangesAsync();
            }
        }
    }
}
