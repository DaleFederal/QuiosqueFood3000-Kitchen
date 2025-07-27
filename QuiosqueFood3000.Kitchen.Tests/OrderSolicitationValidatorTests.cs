
using FluentAssertions;
using FluentValidation.TestHelper;
using QuiosqueFood3000.Kitchen.Application.Validators;
using QuiosqueFood3000.Kitchen.Domain.Entities;

namespace QuiosqueFood3000.Kitchen.Tests;

public class OrderSolicitationValidatorTests
{
    private readonly OrderSolicitationValidator _validator;

    public OrderSolicitationValidatorTests()
    {
        _validator = new OrderSolicitationValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Products_Is_Empty()
    {
        // Arrange
        var orderSolicitation = new OrderSolicitation { Products = new List<Product>() };

        // Act
        var result = _validator.TestValidate(orderSolicitation);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Products)
              .WithErrorMessage("Order must contain at least one product.");
    }

    [Fact]
    public void Should_Have_Error_When_ProductName_Is_Empty()
    {
        // Arrange
        var orderSolicitation = new OrderSolicitation
        {
            Products = new List<Product>
            {
                new Product { Name = "", Description = "Description" }
            }
        };

        // Act
        var result = _validator.TestValidate(orderSolicitation);

        // Assert
        result.ShouldHaveValidationErrorFor("Products[0].Name")
              .WithErrorMessage("Product name cannot be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_ProductDescription_Is_Empty()
    {
        // Arrange
        var orderSolicitation = new OrderSolicitation
        {
            Products = new List<Product>
            {
                new Product { Name = "Name", Description = "" }
            }
        };

        // Act
        var result = _validator.TestValidate(orderSolicitation);

        // Assert
        result.ShouldHaveValidationErrorFor("Products[0].Description")
              .WithErrorMessage("Product description cannot be empty.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Order_Is_Valid()
    {
        // Arrange
        var orderSolicitation = new OrderSolicitation
        {
            Products = new List<Product>
            {
                new Product { Name = "Name", Description = "Description" }
            }
        };

        // Act
        var result = _validator.TestValidate(orderSolicitation);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
