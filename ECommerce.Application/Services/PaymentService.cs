using ECommerce.Application.Interfaces;
using ECommerce.Domain.Models;

namespace ECommerce.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<bool> ProcessPaymentAsync(
    int orderId,
    string userId)
    {
        var order =
            await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            return false;
        }

        if (order.UserId != userId)
        {
            return false;
        }

        var existingPayment =
            await _paymentRepository.GetByOrderIdAsync(order.Id);

        if (existingPayment != null)
        {
            return existingPayment.Status == "Completed";
        }

        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = order.TotalAmount,
            PaymentMethod = "Simulated",
            Status = "Completed",
            PaymentDate = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment);

        order.Status = "Paid";

        await _paymentRepository.SaveChangesAsync();
        await _paymentRepository.SaveChangesAsync();

        return true;
    }
}