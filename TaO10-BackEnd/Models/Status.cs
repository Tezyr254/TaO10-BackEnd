using System;
using System.Collections.Generic;

namespace TaO10_BackEnd.Models;

public partial class Status
{
    public Guid StatusId { get; set; }

    public string EntityType { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual ICollection<ForumThread> ForumThreads { get; set; } = new List<ForumThread>();

    public virtual ICollection<Package> Packages { get; set; } = new List<Package>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<UserExamAttempt> UserExamAttempts { get; set; } = new List<UserExamAttempt>();

    public virtual ICollection<UserPackage> UserPackages { get; set; } = new List<UserPackage>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
