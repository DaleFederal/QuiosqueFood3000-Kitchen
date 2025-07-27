using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using QuiosqueFood3000.Kitchen.Application.Interfaces;
using QuiosqueFood3000.Kitchen.Application.Services;
using QuiosqueFood3000.Kitchen.Domain.Entities;
using QuiosqueFood3000.Kitchen.Domain.Enums;

namespace QuiosqueFood3000.Kitchen.Tests;

public class OrderSolicitationServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IOrderSolicitationRepository> _orderSolicitationRepositoryMock;
    private readonly OrderSolicitationService _sut;

    public OrderSolicitationServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _orderSolicitationRepositoryMock = _fixture.Freeze<Mock<IOrderSolicitationRepository>>();
        _sut = _fixture.Create<OrderSolicitationService>();
    }

    [Fact]
    public async Task CreateOrder_Should_Add_New_Order_And_Set_Initial_Properties()
    {
        // Arrange
        var orderSolicitation = _fixture.Build<OrderSolicitation>()
                                        .Without(o => o.Id)
                                        .Without(o => o.GenerateDate)
                                        .Create();

        _orderSolicitationRepositoryMock.Setup(x => x.Add(It.IsAny<OrderSolicitation>()))
                                        .ReturnsAsync((OrderSolicitation o) => o);

        // Act
        var result = await _sut.CreateOrder(orderSolicitation);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.GenerateDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _orderSolicitationRepositoryMock.Verify(x => x.Add(It.IsAny<OrderSolicitation>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatus_Should_Update_Status_And_DeliveredDate_When_Completed()
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
        var result = await _sut.UpdateOrderStatus(orderId, OrderStatus.Completed);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Completed);
        result.DeliveredDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _orderSolicitationRepositoryMock.Verify(x => x.GetById(orderId), Times.Once);
        _orderSolicitationRepositoryMock.Verify(x => x.Update(It.IsAny<OrderSolicitation>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatus_Should_Update_Status_Without_DeliveredDate_When_Not_Completed()
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
        var result = await _sut.UpdateOrderStatus(orderId, OrderStatus.InProgress);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.InProgress);
        result.DeliveredDate.Should().BeNull();
        _orderSolicitationRepositoryMock.Verify(x => x.GetById(orderId), Times.Once);
        _orderSolicitationRepositoryMock.Verify(x => x.Update(It.IsAny<OrderSolicitation>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatus_Should_Return_Null_When_Order_Not_Found()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderSolicitationRepositoryMock.Setup(x => x.GetById(orderId))
                                        .ReturnsAsync((OrderSolicitation?)null);

        // Act
        var result = await _sut.UpdateOrderStatus(orderId, OrderStatus.Completed);

        // Assert
        result.Should().BeNull();
        _orderSolicitationRepositoryMock.Verify(x => x.GetById(orderId), Times.Once);
        _orderSolicitationRepositoryMock.Verify(x => x.Update(It.IsAny<OrderSolicitation>()), Times.Never);
    }

    [Fact]
    public async Task GetProductionQueue_Should_Return_All_Orders()
    {
        // Arrange
        var orders = _fixture.CreateMany<OrderSolicitation>(3).ToList();
        _orderSolicitationRepositoryMock.Setup(x => x.GetAll())
                                        .ReturnsAsync(orders);

        // Act
        var result = await _sut.GetProductionQueue();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(orders);
        _orderSolicitationRepositoryMock.Verify(x => x.GetAll(), Times.Once);
    }
}