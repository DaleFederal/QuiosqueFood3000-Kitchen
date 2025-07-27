
using QuiosqueFood3000.Kitchen.Domain.Enums;

namespace QuiosqueFood3000.Kitchen.Domain.Entities;

public class OrderSolicitation
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime GenerateDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public Guid? CustomerId { get; set; }
    public string? AnonymousIdentification { get; set; }
    public List<Product> Products { get; set; } = new();
}

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
