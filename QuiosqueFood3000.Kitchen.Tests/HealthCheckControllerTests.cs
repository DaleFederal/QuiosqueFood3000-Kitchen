using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using QuiosqueFood3000.Kitchen.Api.Controllers;

namespace QuiosqueFood3000.Kitchen.Tests;

public class HealthCheckControllerTests
{
    private readonly HealthCheckController _sut;

    public HealthCheckControllerTests()
    {
        _sut = new HealthCheckController();
    }

    [Fact]
    public void Get_Should_Return_Ok_With_Healthy_Message()
    {
        // Act
        var result = _sut.Get();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be("Healthy");
    }
}
