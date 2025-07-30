using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using QuiosqueFood3000.Kitchen.Domain.Entities;
using QuiosqueFood3000.Kitchen.Domain.Enums;
using QuiosqueFood3000.Kitchen.Infrastructure.Repositories;
using System.Text.Json;

namespace QuiosqueFood3000.Kitchen.Tests;

public class OrderSolicitationRepositoryTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAmazonDynamoDB> _dynamoDbClientMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly OrderSolicitationRepository _sut;
    private readonly string _tableName = "TestOrderSolicitations";

    public OrderSolicitationRepositoryTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _dynamoDbClientMock = _fixture.Freeze<Mock<IAmazonDynamoDB>>();
        _configurationMock = _fixture.Freeze<Mock<IConfiguration>>();

        // Setup configuration mock
        _configurationMock.Setup(x => x.GetConnectionString("DynamoDB:OrderSolicitationTableName"))
                         .Returns(_tableName);

        // Setup table existence check to avoid initialization
        _dynamoDbClientMock.Setup(x => x.DescribeTableAsync(_tableName, default))
                          .ReturnsAsync(new DescribeTableResponse
                          {
                              Table = new TableDescription { TableStatus = TableStatus.ACTIVE }
                          });

        _sut = new OrderSolicitationRepository(_dynamoDbClientMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task Add_Should_Put_Item_To_DynamoDB_And_Return_Entity()
    {
        // Arrange
        var orderSolicitation = _fixture.Build<OrderSolicitation>()
                                       .With(x => x.Id, Guid.NewGuid())
                                       .With(x => x.Status, OrderStatus.Received)
                                       .With(x => x.GenerateDate, DateTime.UtcNow)
                                       .With(x => x.CustomerId, Guid.NewGuid())
                                       .With(x => x.AnonymousIdentification, "test-anonymous")
                                       .With(x => x.DeliveredDate, DateTime.UtcNow.AddHours(1))
                                       .Create();

        _dynamoDbClientMock.Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), default))
                          .ReturnsAsync(new PutItemResponse());

        // Act
        var result = await _sut.Add(orderSolicitation);

        // Assert
        result.Should().Be(orderSolicitation);
        _dynamoDbClientMock.Verify(x => x.PutItemAsync(It.Is<PutItemRequest>(req =>
            req.TableName == _tableName &&
            req.Item.ContainsKey("Id") &&
            req.Item.ContainsKey("Status") &&
            req.Item.ContainsKey("GenerateDate") &&
            req.Item.ContainsKey("CustomerId") &&
            req.Item.ContainsKey("AnonymousIdentification") &&
            req.Item.ContainsKey("Products") &&
            req.Item.ContainsKey("DeliveredDate")
        ), default), Times.Once);
    }

    [Fact]
    public async Task Add_Should_Handle_Null_Optional_Fields()
    {
        // Arrange
        var orderSolicitation = _fixture.Build<OrderSolicitation>()
                                       .With(x => x.Id, Guid.NewGuid())
                                       .With(x => x.Status, OrderStatus.Received)
                                       .With(x => x.GenerateDate, DateTime.UtcNow)
                                       .Without(x => x.CustomerId)
                                       .Without(x => x.AnonymousIdentification)
                                       .Without(x => x.DeliveredDate)
                                       .Create();

        _dynamoDbClientMock.Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), default))
                          .ReturnsAsync(new PutItemResponse());

        // Act
        var result = await _sut.Add(orderSolicitation);

        // Assert
        result.Should().Be(orderSolicitation);
        _dynamoDbClientMock.Verify(x => x.PutItemAsync(It.Is<PutItemRequest>(req =>
            req.TableName == _tableName &&
            req.Item["CustomerId"].NULL == true &&
            req.Item["AnonymousIdentification"].NULL == true &&
            !req.Item.ContainsKey("DeliveredDate")
        ), default), Times.Once);
    }

    [Fact]
    public async Task Update_Should_Call_Add_Method()
    {
        // Arrange
        var orderSolicitation = _fixture.Create<OrderSolicitation>();

        _dynamoDbClientMock.Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), default))
                          .ReturnsAsync(new PutItemResponse());

        // Act
        var result = await _sut.Update(orderSolicitation);

        // Assert
        result.Should().Be(orderSolicitation);
        _dynamoDbClientMock.Verify(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), default), Times.Once);
    }

    [Fact]
    public async Task GetById_Should_Return_Order_When_Found()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = _fixture.Build<OrderSolicitation>()
                                   .With(x => x.Id, orderId)
                                   .With(x => x.Status, OrderStatus.InProgress)
                                   .With(x => x.CustomerId, Guid.NewGuid())
                                   .With(x => x.AnonymousIdentification, "test-id")
                                   .With(x => x.DeliveredDate, DateTime.UtcNow)
                                   .Create();

        var dynamoDbItem = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = expectedOrder.Id.ToString() },
            ["Status"] = new AttributeValue { N = ((int)expectedOrder.Status).ToString() },
            ["GenerateDate"] = new AttributeValue { S = expectedOrder.GenerateDate.ToString("O") },
            ["CustomerId"] = new AttributeValue { S = expectedOrder.CustomerId!.Value.ToString() },
            ["AnonymousIdentification"] = new AttributeValue { S = expectedOrder.AnonymousIdentification },
            ["Products"] = new AttributeValue { S = JsonSerializer.Serialize(expectedOrder.Products) },
            ["DeliveredDate"] = new AttributeValue { S = expectedOrder.DeliveredDate!.Value.ToString("O") }
        };

        _dynamoDbClientMock.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
                          .ReturnsAsync(new GetItemResponse
                          {
                              Item = dynamoDbItem,
                              IsItemSet = true
                          });

        // Act
        var result = await _sut.GetById(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedOrder.Id);
        result.Status.Should().Be(expectedOrder.Status);
        result.CustomerId.Should().Be(expectedOrder.CustomerId);
        result.AnonymousIdentification.Should().Be(expectedOrder.AnonymousIdentification);
        result.DeliveredDate.Should().BeCloseTo(expectedOrder.DeliveredDate!.Value, TimeSpan.FromSeconds(1));

        _dynamoDbClientMock.Verify(x => x.GetItemAsync(It.Is<GetItemRequest>(req =>
            req.TableName == _tableName &&
            req.Key["Id"].S == orderId.ToString()
        ), default), Times.Once);
    }

    [Fact]
    public async Task GetById_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _dynamoDbClientMock.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
                          .ReturnsAsync(new GetItemResponse
                          {
                              IsItemSet = false
                          });

        // Act
        var result = await _sut.GetById(orderId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_Should_Handle_Null_Optional_Fields_In_Response()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var generateDate = DateTime.UtcNow;

        var dynamoDbItem = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = orderId.ToString() },
            ["Status"] = new AttributeValue { N = "0" },
            ["GenerateDate"] = new AttributeValue { S = generateDate.ToString("O") },
            ["CustomerId"] = new AttributeValue { NULL = true },
            ["AnonymousIdentification"] = new AttributeValue { NULL = true },
            ["Products"] = new AttributeValue { S = "[]" }
        };

        _dynamoDbClientMock.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
                          .ReturnsAsync(new GetItemResponse
                          {
                              Item = dynamoDbItem,
                              IsItemSet = true
                          });

        // Act
        var result = await _sut.GetById(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.CustomerId.Should().BeNull();
        result.AnonymousIdentification.Should().BeNull();
        result.DeliveredDate.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_Should_Return_All_Orders()
    {
        // Arrange
        var orders = _fixture.CreateMany<OrderSolicitation>(3).ToList();
        var dynamoDbItems = orders.Select(order => new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = order.Id.ToString() },
            ["Status"] = new AttributeValue { N = ((int)order.Status).ToString() },
            ["GenerateDate"] = new AttributeValue { S = order.GenerateDate.ToString("O") },
            ["CustomerId"] = new AttributeValue { NULL = true },
            ["AnonymousIdentification"] = new AttributeValue { NULL = true },
            ["Products"] = new AttributeValue { S = JsonSerializer.Serialize(order.Products) }
        }).ToList();

        _dynamoDbClientMock.Setup(x => x.ScanAsync(It.IsAny<ScanRequest>(), default))
                          .ReturnsAsync(new ScanResponse
                          {
                              Items = dynamoDbItems
                          });

        // Act
        var result = await _sut.GetAll();

        // Assert
        result.Should().HaveCount(3);
        _dynamoDbClientMock.Verify(x => x.ScanAsync(It.Is<ScanRequest>(req =>
            req.TableName == _tableName
        ), default), Times.Once);
    }
}

// Additional test class for testing repository initialization scenarios
public class OrderSolicitationRepositoryInitializationTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAmazonDynamoDB> _dynamoDbClientMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly string _tableName = "TestOrderSolicitations";

    public OrderSolicitationRepositoryInitializationTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _dynamoDbClientMock = _fixture.Freeze<Mock<IAmazonDynamoDB>>();
        _configurationMock = _fixture.Freeze<Mock<IConfiguration>>();

        _configurationMock.Setup(x => x.GetConnectionString("DynamoDB:OrderSolicitationTableName"))
                         .Returns(_tableName);
    }

    [Fact]
    public void Constructor_Should_Use_Default_Table_Name_When_Configuration_Is_Null()
    {
        // Arrange
        _configurationMock.Setup(x => x.GetConnectionString("DynamoDB:OrderSolicitationTableName"))
                         .Returns((string?)null);

        _dynamoDbClientMock.Setup(x => x.DescribeTableAsync("OrderSolicitations", default))
                          .ReturnsAsync(new DescribeTableResponse
                          {
                              Table = new TableDescription { TableStatus = TableStatus.ACTIVE }
                          });

        // Act & Assert
        var repository = new OrderSolicitationRepository(_dynamoDbClientMock.Object, _configurationMock.Object);
        repository.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_Should_Create_Table_When_Not_Exists()
    {
        // Arrange
        _dynamoDbClientMock.Setup(x => x.DescribeTableAsync(_tableName, default))
                          .ThrowsAsync(new ResourceNotFoundException("Table not found"));

        _dynamoDbClientMock.Setup(x => x.CreateTableAsync(It.IsAny<CreateTableRequest>(), default))
                          .ReturnsAsync(new CreateTableResponse());

        // Setup for WaitForTableToBeActiveAsync
        _dynamoDbClientMock.SetupSequence(x => x.DescribeTableAsync(_tableName, default))
                          .ThrowsAsync(new ResourceNotFoundException("Table not found"))
                          .ReturnsAsync(new DescribeTableResponse
                          {
                              Table = new TableDescription { TableStatus = TableStatus.ACTIVE }
                          });

        // Act & Assert
        var repository = new OrderSolicitationRepository(_dynamoDbClientMock.Object, _configurationMock.Object);
        repository.Should().NotBeNull();

        _dynamoDbClientMock.Verify(x => x.CreateTableAsync(It.Is<CreateTableRequest>(req =>
            req.TableName == _tableName &&
            req.KeySchema.Any(k => k.AttributeName == "Id" && k.KeyType == KeyType.HASH) &&
            req.AttributeDefinitions.Any(a => a.AttributeName == "Id" && a.AttributeType == ScalarAttributeType.S) &&
            req.BillingMode == BillingMode.PAY_PER_REQUEST
        ), default), Times.Once);
    }
}
