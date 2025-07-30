using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using QuiosqueFood3000.Kitchen.Application.Interfaces;
using QuiosqueFood3000.Kitchen.Application.Services;
using QuiosqueFood3000.Kitchen.Domain.Entities;
using QuiosqueFood3000.Kitchen.Domain.Enums;

namespace QuiosqueFood3000.Kitchen.Tests;

public class OrderSolicitationServiceIntegrationTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IOrderSolicitationRepository> _orderSolicitationRepositoryMock;
    private readonly OrderSolicitationService _sut;

    public OrderSolicitationServiceIntegrationTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _orderSolicitationRepositoryMock = _fixture.Freeze<Mock<IOrderSolicitationRepository>>();
        _sut = _fixture.Create<OrderSolicitationService>();
    }

    [Theory]
    [InlineData(OrderStatus.Received)]
    [InlineData(OrderStatus.InProgress)]
    [InlineData(OrderStatus.Ready)]
    public async Task UpdateOrderStatus_Should_Not_Set_DeliveredDate_For_Non_Completed_Status(OrderStatus status)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var existingOrder = _fixture.Build<OrderSolicitation>()
                                   .With(o => o.Id, orderId)
                                   .With(o => o.Status, OrderStatus.Received)
                                   .Without(o => o.DeliveredDate)
                                   .Create();

        _orderSolicitationRepositoryMock.Setup(x => x.GetById(orderId))
                                        .ReturnsAsync(existingOrder);
        _orderSolicitationRepositoryMock.Setup(x => x.Update(It.IsAny<OrderSolicitation>()))
                                        .ReturnsAsync((OrderSolicitation o) => o);

        // Act
        var result = await _sut.UpdateOrderStatus(orderId, status);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(status);
        result.DeliveredDate.Should().BeNull();
    }

    [Fact]
    public async Task CreateOrder_Should_Generate_New_Id_Even_If_Id_Is_Already_Set()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var orderSolicitation = _fixture.Build<OrderSolicitation>()
                                       .With(o => o.Id, existingId)
                                       .With(o => o.GenerateDate, DateTime.MinValue)
                                       .Create();

        _orderSolicitationRepositoryMock.Setup(x => x.Add(It.IsAny<OrderSolicitation>()))
                                        .ReturnsAsync((OrderSolicitation o) => o);

        // Act
        var result = await _sut.CreateOrder(orderSolicitation);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(existingId);
        result.Id.Should().NotBe(Guid.Empty);
        result.GenerateDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetProductionQueue_Should_Return_Empty_List_When_Repository_Returns_Empty()
    {
        // Arrange
        _orderSolicitationRepositoryMock.Setup(x => x.GetAll())
                                        .ReturnsAsync(new List<OrderSolicitation>());

        // Act
        var result = await _sut.GetProductionQueue();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateOrderStatus_Should_Preserve_Original_Order_Properties()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var anonymousId = "test-anonymous-123";
        var generateDate = DateTime.UtcNow.AddHours(-2);
        var products = _fixture.CreateMany<Product>(2).ToList();

        var existingOrder = _fixture.Build<OrderSolicitation>()
                                   .With(o => o.Id, orderId)
                                   .With(o => o.CustomerId, customerId)
                                   .With(o => o.AnonymousIdentification, anonymousId)
                                   .With(o => o.GenerateDate, generateDate)
                                   .With(o => o.Products, products)
                                   .With(o => o.Status, OrderStatus.Received)
                                   .Without(o => o.DeliveredDate)
                                   .Create();

        _orderSolicitationRepositoryMock.Setup(x => x.GetById(orderId))
                                        .ReturnsAsync(existingOrder);
        _orderSolicitationRepositoryMock.Setup(x => x.Update(It.IsAny<OrderSolicitation>()))
                                        .ReturnsAsync((OrderSolicitation o) => o);

        // Act
        var result = await _sut.UpdateOrderStatus(orderId, OrderStatus.Completed);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.CustomerId.Should().Be(customerId);
        result.AnonymousIdentification.Should().Be(anonymousId);
        result.GenerateDate.Should().Be(generateDate);
        result.Products.Should().BeEquivalentTo(products);
        result.Status.Should().Be(OrderStatus.Completed);
        result.DeliveredDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
