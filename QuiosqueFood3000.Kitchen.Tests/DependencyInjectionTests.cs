using Amazon.DynamoDBv2;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuiosqueFood3000.Kitchen.Api.IoC;
using QuiosqueFood3000.Kitchen.Application.Interfaces;
using QuiosqueFood3000.Kitchen.Application.Services;
using QuiosqueFood3000.Kitchen.Application.Validators;
using QuiosqueFood3000.Kitchen.Domain.Entities;
using QuiosqueFood3000.Kitchen.Infrastructure.Repositories;

namespace QuiosqueFood3000.Kitchen.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_Should_Register_All_Required_Services_For_Local_DynamoDB()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DynamoDB:ServiceURL"] = "http://localhost:8000"
            })
            .Build();

        // Act
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IAmazonDynamoDB>().Should().NotBeNull();
        serviceProvider.GetService<IOrderSolicitationRepository>().Should().NotBeNull();
        serviceProvider.GetService<IOrderSolicitationService>().Should().NotBeNull();
        serviceProvider.GetService<IValidator<OrderSolicitation>>().Should().NotBeNull();

        // Verify specific implementations
        serviceProvider.GetService<IOrderSolicitationRepository>().Should().BeOfType<OrderSolicitationRepository>();
        serviceProvider.GetService<IOrderSolicitationService>().Should().BeOfType<OrderSolicitationService>();
        serviceProvider.GetService<IValidator<OrderSolicitation>>().Should().BeOfType<OrderSolicitationValidator>();
    }

    [Fact]
    public void AddInfrastructure_Should_Register_All_Required_Services_For_AWS_DynamoDB()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IOrderSolicitationRepository>().Should().NotBeNull();
        serviceProvider.GetService<IOrderSolicitationService>().Should().NotBeNull();
        serviceProvider.GetService<IValidator<OrderSolicitation>>().Should().NotBeNull();

        // Verify specific implementations
        serviceProvider.GetService<IOrderSolicitationRepository>().Should().BeOfType<OrderSolicitationRepository>();
        serviceProvider.GetService<IOrderSolicitationService>().Should().BeOfType<OrderSolicitationService>();
        serviceProvider.GetService<IValidator<OrderSolicitation>>().Should().BeOfType<OrderSolicitationValidator>();
    }

    [Fact]
    public void AddInfrastructure_Should_Configure_Local_DynamoDB_When_ServiceURL_Is_Provided()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceUrl = "http://localhost:8000";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DynamoDB:ServiceURL"] = serviceUrl
            })
            .Build();

        // Act
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dynamoDbClient = serviceProvider.GetService<IAmazonDynamoDB>();
        dynamoDbClient.Should().NotBeNull();
        dynamoDbClient.Should().BeOfType<AmazonDynamoDBClient>();
    }

    [Fact]
    public void AddInfrastructure_Should_Register_Services_With_Correct_Lifetimes()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DynamoDB:ServiceURL"] = "http://localhost:8000"
            })
            .Build();

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var repositoryDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IOrderSolicitationRepository));
        repositoryDescriptor.Should().NotBeNull();
        repositoryDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IOrderSolicitationService));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

        var dynamoDbDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IAmazonDynamoDB));
        dynamoDbDescriptor.Should().NotBeNull();
        dynamoDbDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }
}
