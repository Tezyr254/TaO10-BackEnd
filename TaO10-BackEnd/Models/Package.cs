using System;
using System.Collections.Generic;

namespace TaO10_BackEnd.Models;

public partial class Package
{
    public Guid PackageId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Price { get; set; }

    public int? DurationTime { get; set; }

    public Guid StatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<PackageExam> PackageExams { get; set; } = new List<PackageExam>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Status Status { get; set; } = null!;

    public virtual ICollection<UserPackage> UserPackages { get; set; } = new List<UserPackage>();
}
