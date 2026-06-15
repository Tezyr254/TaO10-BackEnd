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

            // 3. SEED 5 PACKAGES: Dùng thử, Cấp tốc, Nâng cao, Chuyên sâu, Premium
            var packagesCount = await context.Packages.CountAsync();
            var hasChuyenSau = await context.Packages.AnyAsync(p => p.Name == "Gói Chuyên Sâu");
            var shouldReSeedPackages = packagesCount != 5 || !hasChuyenSau;

            if (shouldReSeedPackages)
            {
                // Clear old package exams and packages first
                var oldPackageExams = await context.PackageExams.ToListAsync();
                context.PackageExams.RemoveRange(oldPackageExams);

                var oldPackages = await context.Packages.ToListAsync();
                context.Packages.RemoveRange(oldPackages);
                await context.SaveChangesAsync();

                var packages = new List<Package>
                {
                    new Package
                    {
                        PackageId = Guid.NewGuid(),
                        Name = "Gói Dùng Thử",
                        Description = "Trải nghiệm miễn phí 3 ngày với 1 bộ đề thi đầy đủ tính năng.",
                        Price = 0,
                        DurationTime = 3,
                        StatusId = activePkgStatusId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Package
                    {
                        PackageId = Guid.NewGuid(),
                        Name = "Gói Cấp Tốc",
                        Description = "Khóa học 1 tháng với 30 bộ đề thi mới nhất theo đúng chuẩn đề thi thực tế.",
                        Price = 129000,
                        DurationTime = 30,
                        StatusId = activePkgStatusId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Package
                    {
                        PackageId = Guid.NewGuid(),
                        Name = "Gói Nâng Cao",
                        Description = "Khóa học 3 tháng với 65 bộ đề thi toàn diện, bảng điều khiển cá nhân và dự đoán kết quả.",
                        Price = 269000,
                        DurationTime = 90,
                        StatusId = activePkgStatusId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Package
                    {
                        PackageId = Guid.NewGuid(),
                        Name = "Gói Chuyên Sâu",
                        Description = "Khóa học 6 tháng với 150+ bộ đề thi chất lượng cao và phân tích kết quả chuyên sâu.",
                        Price = 469000,
                        DurationTime = 180,
                        StatusId = activePkgStatusId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Package
                    {
                        PackageId = Guid.NewGuid(),
                        Name = "Gói Premium",
                        Description = "Khóa học 12 tháng không giới hạn đề thi, đầy đủ tính năng cao cấp nhất.",
                        Price = 899000,
                        DurationTime = 365,
                        StatusId = activePkgStatusId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                await context.Packages.AddRangeAsync(packages);
                await context.SaveChangesAsync();
            }

            // 4. SEED EXAMS & QUESTIONS (8 Exams for testing package tiers)
            Exam exam1, exam2, exam3, exam4, exam5, exam6, exam7, exam8;

            if (!await context.Exams.AnyAsync())
            {
                exam1 = new Exam { ExamId = Guid.NewGuid(), Title = "Đề thi số 1 – Ngữ pháp Tiếng Anh cơ bản 2026", Description = "Ôn tập thì hiện tại đơn, quá khứ đơn và tương lai đơn.", QuestionsCount = 5, DurationTime = 60, Level = "Dễ", Year = 2026, ExamType = "Tuyển sinh lớp 10", ViewsCount = 12000, AttemptsCount = 520, StatusId = activeExamStatusId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                exam2 = new Exam { ExamId = Guid.NewGuid(), Title = "Đề thi số 2 – Đọc hiểu Tiếng Anh 2026", Description = "Luyện kỹ năng đọc hiểu và trả lời câu hỏi.", QuestionsCount = 5, DurationTime = 60, Level = "Trung bình", Year = 2026, ExamType = "Tuyển sinh lớp 10", ViewsCount = 9800, AttemptsCount = 410, StatusId = activeExamStatusId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                exam3 = new Exam { ExamId = Guid.NewGuid(), Title = "Đề thi số 3 – Từ vựng & Phrasal Verbs 2026", Description = "Kiểm tra ngữ âm, từ vựng và cụm từ động từ.", QuestionsCount = 5, DurationTime = 60, Level = "Trung bình", Year = 2026, ExamType = "Tuyển sinh lớp 10", ViewsCount = 7500, AttemptsCount = 330, StatusId = activeExamStatusId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                exam4 = new Exam { ExamId = Guid.NewGuid(), Title = "Đề thi số 4 – Viết lại câu & Tìm lỗi sai 2026", Description = "Ôn luyện viết lại câu và tìm lỗi sai cơ bản.", QuestionsCount = 5, DurationTime = 60, Level = "Khó", Year = 2026, ExamType = "Tuyển sinh lớp 10", ViewsCount = 5100, AttemptsCount = 205, StatusId = activeExamStatusId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                exam5 = new Exam { ExamId = Guid.NewGuid(), Title = "Đề thi số 5 – Ngữ pháp nâng cao 2026", Description = "Câu bị động, câu điều kiện và mệnh đề quan hệ.", QuestionsCount = 5, DurationTime = 60, Level = "Khó", Year = 2026, ExamType = "Tuyển sinh lớp 10", ViewsCount = 4200, AttemptsCount = 180, StatusId = activeExamStatusId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                exam6 = new Exam { ExamId = Guid.NewGuid(), Title = "Đề thi số 6 – Tổng hợp Tiếng Anh 2026", Description = "Đề thi tổng hợp đánh giá toàn diện các kỹ năng.", QuestionsCount = 5, DurationTime = 60, Level = "Khó", Year = 2026, ExamType = "Tuyển sinh lớp 10", ViewsCount = 3800, AttemptsCount = 160, StatusId = activeExamStatusId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                exam7 = new Exam { ExamId = Guid.NewGuid(), Title = "Đề thi số 7 – Đề thi thử chuyên Anh 2026", Description = "Đề thi mô phỏng kỳ thi thực tế với độ khó cao.", QuestionsCount = 5, DurationTime = 60, Level = "Khó", Year = 2026, ExamType = "Thi thử", ViewsCount = 3200, AttemptsCount = 140, StatusId = activeExamStatusId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                exam8 = new Exam { ExamId = Guid.NewGuid(), Title = "Đề thi số 8 – Đề thi thử trường chuyên 2026", Description = "Đề thi khó dành cho các trường chuyên hàng đầu.", QuestionsCount = 5, DurationTime = 60, Level = "Khó", Year = 2026, ExamType = "Thi thử", ViewsCount = 2800, AttemptsCount = 120, StatusId = activeExamStatusId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

                await context.Exams.AddRangeAsync(exam1, exam2, exam3, exam4, exam5, exam6, exam7, exam8);
                await context.SaveChangesAsync();

                // Mock Questions for all 8 Exams (5 questions per exam)
                var questions = new List<Question>();
                var examsList = new[] { exam1, exam2, exam3, exam4, exam5, exam6, exam7, exam8 };

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
                            QuestionText = $"[{ex.Title.Substring(0, Math.Min(15, ex.Title.Length))}] Câu hỏi số {i}: Chọn từ thích hợp: 'She usually _______ early in the morning.'",
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

            // Always recreate package-exams mappings if packages were modified or mapping table is empty
            if (shouldReSeedPackages || !await context.PackageExams.AnyAsync())
            {
                var oldPackageExams = await context.PackageExams.ToListAsync();
                if (oldPackageExams.Any())
                {
                    context.PackageExams.RemoveRange(oldPackageExams);
                    await context.SaveChangesAsync();
                }

                var examsList = await context.Exams.OrderBy(e => e.Title).ToListAsync();
                var e1 = examsList.ElementAtOrDefault(0);
                var e2 = examsList.ElementAtOrDefault(1);
                var e3 = examsList.ElementAtOrDefault(2);
                var e4 = examsList.ElementAtOrDefault(3);
                var e5 = examsList.ElementAtOrDefault(4);
                var e6 = examsList.ElementAtOrDefault(5);
                var e7 = examsList.ElementAtOrDefault(6);
                var e8 = examsList.ElementAtOrDefault(7);

                var freePkg = await context.Packages.FirstOrDefaultAsync(p => p.Name == "Gói Dùng Thử");
                var captocPkg = await context.Packages.FirstOrDefaultAsync(p => p.Name == "Gói Cấp Tốc");
                var nangcaoPkg = await context.Packages.FirstOrDefaultAsync(p => p.Name == "Gói Nâng Cao");
                var chuyensauPkg = await context.Packages.FirstOrDefaultAsync(p => p.Name == "Gói Chuyên Sâu");
                var premiumPkg = await context.Packages.FirstOrDefaultAsync(p => p.Name == "Gói Premium");

                var packageExams = new List<PackageExam>();

                // Gói Dùng Thử: 1 đề
                if (freePkg != null && e1 != null)
                {
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = freePkg.PackageId, ExamId = e1.ExamId, CreatedAt = DateTime.UtcNow });
                }

                // Gói Cấp Tốc: 3 đề
                if (captocPkg != null && e1 != null && e2 != null && e3 != null)
                {
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = captocPkg.PackageId, ExamId = e1.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = captocPkg.PackageId, ExamId = e2.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = captocPkg.PackageId, ExamId = e3.ExamId, CreatedAt = DateTime.UtcNow });
                }

                // Gói Nâng Cao: 5 đề
                if (nangcaoPkg != null && e1 != null && e2 != null && e3 != null && e4 != null && e5 != null)
                {
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = nangcaoPkg.PackageId, ExamId = e1.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = nangcaoPkg.PackageId, ExamId = e2.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = nangcaoPkg.PackageId, ExamId = e3.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = nangcaoPkg.PackageId, ExamId = e4.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = nangcaoPkg.PackageId, ExamId = e5.ExamId, CreatedAt = DateTime.UtcNow });
                }

                // Gói Chuyên Sâu: 6 đề
                if (chuyensauPkg != null && e1 != null && e2 != null && e3 != null && e4 != null && e5 != null && e6 != null)
                {
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = chuyensauPkg.PackageId, ExamId = e1.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = chuyensauPkg.PackageId, ExamId = e2.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = chuyensauPkg.PackageId, ExamId = e3.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = chuyensauPkg.PackageId, ExamId = e4.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = chuyensauPkg.PackageId, ExamId = e5.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = chuyensauPkg.PackageId, ExamId = e6.ExamId, CreatedAt = DateTime.UtcNow });
                }

                // Gói Premium: 8 đề
                if (premiumPkg != null && e1 != null && e2 != null && e3 != null && e4 != null && e5 != null && e6 != null && e7 != null && e8 != null)
                {
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = premiumPkg.PackageId, ExamId = e1.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = premiumPkg.PackageId, ExamId = e2.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = premiumPkg.PackageId, ExamId = e3.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = premiumPkg.PackageId, ExamId = e4.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = premiumPkg.PackageId, ExamId = e5.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = premiumPkg.PackageId, ExamId = e6.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = premiumPkg.PackageId, ExamId = e7.ExamId, CreatedAt = DateTime.UtcNow });
                    packageExams.Add(new PackageExam { PackageExamId = Guid.NewGuid(), PackageId = premiumPkg.PackageId, ExamId = e8.ExamId, CreatedAt = DateTime.UtcNow });
                }

                await context.PackageExams.AddRangeAsync(packageExams);
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
