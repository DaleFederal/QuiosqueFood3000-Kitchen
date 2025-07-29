
using Microsoft.AspNetCore.Mvc;

namespace QuiosqueFood3000.Kitchen.Api.Controllers;

[ApiController]
[Route("/health")]
public class HealthCheckController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Healthy");
    }
}
