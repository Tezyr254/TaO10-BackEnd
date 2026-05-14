using System;
using System.Collections.Generic;

namespace TaO10_BackEnd.Models;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public Guid? UserId { get; set; }

    public Guid? PackageId { get; set; }

    public int ExpectedAmount { get; set; }

    public int? ReceivedAmount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? TransactionCode { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public DateTime? PaidAt { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Package? Package { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<UserPackage> UserPackages { get; set; } = new List<UserPackage>();
}
