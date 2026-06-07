using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaO10_BackEnd.Interfaces
{
 public record PaymentItem(string Name, int Quantity, int UnitPrice);

 public record CreatePaymentResult(string CheckoutUrl, string? QrCode);

 public record WebhookResult(string Code, long OrderCode, int Amount);

 public interface IPaymentProvider
 {
 Task<CreatePaymentResult> CreatePaymentLinkAsync(long orderCode, int amount, string description, List<PaymentItem> items, string cancelUrl, string returnUrl);
 Task<WebhookResult?> VerifyWebhookAsync(JsonElement webhookBody);
 }
}
