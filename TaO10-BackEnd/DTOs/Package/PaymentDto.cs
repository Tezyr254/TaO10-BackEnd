using System;
using System.Collections.Generic;
using TaO10_BackEnd.DTOs.Exams;

namespace TaO10_BackEnd.DTOs.Package
{
    public class CheckoutRequest
    {
        public Guid PackageId { get; set; }
        public string PaymentMethod { get; set; } = null!; 
    }

    public class CheckoutResponse
    {
        public Guid PaymentId { get; set; }
        public string CheckoutUrl { get; set; } = null!;
        public string? QrCodeUrl { get; set; }
        public int ExpectedAmount { get; set; }
        public string Message { get; set; } = null!;
    }

    public class PaymentResponse
    {
        public Guid PaymentId { get; set; }
        public string PackageName { get; set; } = null!;
        public int ExpectedAmount { get; set; }
        public int? ReceivedAmount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? TransactionCode { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? PaidAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class AdminPaymentResponse : PaymentResponse
    {
        public string UserEmail { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
    }

    public class PaymentPackageDetailResponse
    {
        public Guid PackageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Price { get; set; }
        public int? DurationTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<ExamDto> Exams { get; set; } = new();
    }
}
