using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // POST: api/Payments/1
    [HttpPost("{orderId}")]
    public async Task<IActionResult> ProcessPayment(int orderId)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result =
            await _paymentService.ProcessPaymentAsync(
                orderId,
                userId);

        if (!result)
        {
            return BadRequest(new
            {
                message = "Payment could not be processed."
            });
        }

        return Ok(new
        {
            message = "Payment completed successfully.",
            orderId = orderId,
            status = "Paid"
        });
    }
}