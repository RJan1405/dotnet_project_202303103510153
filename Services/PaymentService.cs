using System;

namespace EventTicketingSystem.Services
{
    public interface IPaymentService
    {
        string GeneratePaymentLink(string orderId, decimal amount, string currency = "INR");
        string GenerateTransactionId();
    }

    public class PaymentService : IPaymentService
    {
        public string GeneratePaymentLink(string orderId, decimal amount, string currency = "INR")
        {
            // In a real application, you would integrate with Razorpay or another payment gateway
            // For demo purposes, we'll just return a mock payment link
            return $"/Payment/Process?orderId={orderId}&amount={amount}";
        }

        public string GenerateTransactionId()
        {
            // Generate a random transaction ID for demo purposes
            return "TXN_" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        }
    }
}