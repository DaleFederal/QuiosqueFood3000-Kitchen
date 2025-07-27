
using QuiosqueFood3000.Kitchen.Application.Interfaces;
using QuiosqueFood3000.Kitchen.Domain.Entities;
using QuiosqueFood3000.Kitchen.Domain.Enums;

namespace QuiosqueFood3000.Kitchen.Application.Services;

public class OrderSolicitationService : IOrderSolicitationService
{
    private readonly IOrderSolicitationRepository _orderSolicitationRepository;

    public OrderSolicitationService(IOrderSolicitationRepository orderSolicitationRepository)
    {
        _orderSolicitationRepository = orderSolicitationRepository;
    }

    public async Task<OrderSolicitation> CreateOrder(OrderSolicitation orderSolicitation)
    {
        orderSolicitation.Id = Guid.NewGuid();
        orderSolicitation.GenerateDate = DateTime.UtcNow;
        return await _orderSolicitationRepository.Add(orderSolicitation);
    }

    public async Task<OrderSolicitation?> UpdateOrderStatus(Guid orderId, OrderStatus status)
    {
        var order = await _orderSolicitationRepository.GetById(orderId);
        if (order == null) return null;

        order.Status = status;
        if (status == OrderStatus.Completed)
            order.DeliveredDate = DateTime.UtcNow;

        return await _orderSolicitationRepository.Update(order);
    }

    public async Task<IEnumerable<OrderSolicitation>> GetProductionQueue()
    {
        return await _orderSolicitationRepository.GetAll();
    }
}
