using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuiosqueFood3000.Kitchen.Api.Controllers;
using QuiosqueFood3000.Kitchen.Application.Interfaces;
using QuiosqueFood3000.Kitchen.Domain.Entities;
using QuiosqueFood3000.Kitchen.Domain.Enums;

namespace QuiosqueFood3000.Kitchen.Tests;

public class OrderSolicitationControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IOrderSolicitationService> _orderSolicitationServiceMock;
    private readonly OrderSolicitationController _sut;

    public OrderSolicitationControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _orderSolicitationServiceMock = _fixture.Freeze<Mock<IOrderSolicitationService>>();
        _sut = _fixture.Create<OrderSolicitationController>();
    }

    [Fact]
    public async Task CreateOrder_Should_Return_Ok_With_Created_Order()
    {
        // Arrange
        var orderSolicitation = _fixture.Create<OrderSolicitation>();
        var createdOrder = _fixture.Create<OrderSolicitation>();

        _orderSolicitationServiceMock.Setup(x => x.CreateOrder(orderSolicitation))
                                     .ReturnsAsync(createdOrder);

        // Act
        var result = await _sut.CreateOrder(orderSolicitation);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(createdOrder);
        _orderSolicitationServiceMock.Verify(x => x.CreateOrder(orderSolicitation), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatus_Should_Return_Ok_When_Order_Exists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var status = OrderStatus.InProgress;
        var updatedOrder = _fixture.Create<OrderSolicitation>();

        _orderSolicitationServiceMock.Setup(x => x.UpdateOrderStatus(orderId, status))
                                     .ReturnsAsync(updatedOrder);

        // Act
        var result = await _sut.UpdateOrderStatus(orderId, status);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(updatedOrder);
        _orderSolicitationServiceMock.Verify(x => x.UpdateOrderStatus(orderId, status), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatus_Should_Return_NotFound_When_Order_Does_Not_Exist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var status = OrderStatus.InProgress;

        _orderSolicitationServiceMock.Setup(x => x.UpdateOrderStatus(orderId, status))
                                     .ReturnsAsync((OrderSolicitation?)null);

        // Act
        var result = await _sut.UpdateOrderStatus(orderId, status);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _orderSolicitationServiceMock.Verify(x => x.UpdateOrderStatus(orderId, status), Times.Once);
    }

    [Fact]
    public async Task GetProductionQueue_Should_Return_Ok_With_Queue()
    {
        // Arrange
        var queue = _fixture.CreateMany<OrderSolicitation>(3).ToList();

        _orderSolicitationServiceMock.Setup(x => x.GetProductionQueue())
                                     .ReturnsAsync(queue);

        // Act
        var result = await _sut.GetProductionQueue();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(queue);
        _orderSolicitationServiceMock.Verify(x => x.GetProductionQueue(), Times.Once);
    }
}
