using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using TaO10_BackEnd.Interfaces;

namespace TaO10_BackEnd.Services
{
    public class PayOSPaymentProvider : IPaymentProvider
    {
        private readonly PayOSClient _payOS;

        public PayOSPaymentProvider(PayOSClient payOS)
        {
            _payOS = payOS;
        }

        public async Task<CreatePaymentResult> CreatePaymentLinkAsync(long orderCode, int amount, string description, List<PaymentItem> items, string cancelUrl, string returnUrl)
        {
            var paymentItems = items.Select(i => new PaymentLinkItem { Name = i.Name, Quantity = i.Quantity, Price = i.UnitPrice }).ToList();

            var paymentData = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = amount,
                Description = description,
                Items = paymentItems,
                CancelUrl = cancelUrl,
                ReturnUrl = returnUrl
            };

            var result = await _payOS.PaymentRequests.CreateAsync(paymentData);
            return new CreatePaymentResult(result.CheckoutUrl, result.QrCode);
        }

        public async Task<WebhookResult?> VerifyWebhookAsync(JsonElement webhookBody)
        {
            try
            {
                var webhookTypeString = webhookBody.GetRawText();
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var webhook = JsonSerializer.Deserialize<Webhook>(webhookTypeString, options);
                
                if (webhook == null)
                    return null;

                var webhookData = await _payOS.Webhooks.VerifyAsync(webhook);
                if (webhookData == null)
                    return null;

                // Webhook code usually "00" for success in payload
                var code = webhook.Code;
                return new WebhookResult(code, webhookData.OrderCode, (int)webhookData.Amount);
            }
            catch
            {
                return null;
            }
        }
    }
}
