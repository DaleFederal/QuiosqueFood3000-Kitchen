
using QuiosqueFood3000.Kitchen.Domain.Entities;
using QuiosqueFood3000.Kitchen.Domain.Enums;

namespace QuiosqueFood3000.Kitchen.Application.Interfaces;

public interface IOrderSolicitationService
{
    Task<OrderSolicitation> CreateOrder(OrderSolicitation orderSolicitation);
    Task<OrderSolicitation?> UpdateOrderStatus(Guid orderId, OrderStatus status);
    Task<IEnumerable<OrderSolicitation>> GetProductionQueue();
}
