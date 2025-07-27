using Microsoft.AspNetCore.Mvc;
using QuiosqueFood3000.Kitchen.Application.Interfaces;
using QuiosqueFood3000.Kitchen.Domain.Entities;
using QuiosqueFood3000.Kitchen.Domain.Enums;

namespace QuiosqueFood3000.Kitchen.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderSolicitationController : ControllerBase
{
    private readonly IOrderSolicitationService _orderSolicitationService;

    public OrderSolicitationController(IOrderSolicitationService orderSolicitationService)
    {
        _orderSolicitationService = orderSolicitationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderSolicitation orderSolicitation)
    {
        var createdOrder = await _orderSolicitationService.CreateOrder(orderSolicitation);
        return Ok(createdOrder);
    }

    [HttpPut("{orderId}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] OrderStatus status)
    {
        var updatedOrder = await _orderSolicitationService.UpdateOrderStatus(orderId, status);
        if (updatedOrder == null) return NotFound();
        return Ok(updatedOrder);
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetProductionQueue()
    {
        var queue = await _orderSolicitationService.GetProductionQueue();
        return Ok(queue);
    }
}
