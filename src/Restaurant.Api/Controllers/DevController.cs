using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Contracts;

namespace Restaurant.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/dev")]
[ProducesResponseType<ApiProblemDto>(StatusCodes.Status501NotImplemented)]
public sealed class DevController(IWebHostEnvironment environment) : StubControllerBase
{
    private IActionResult DevelopmentOnly() => environment.IsDevelopment() ? NotImplementedResult() : NotFound();

    [HttpPost("seed-demo")]
    public IActionResult SeedDemo() => DevelopmentOnly();
    [HttpPost("simulate-payment")]
    public IActionResult SimulatePayment(DevSimulatePaymentRequest request) => DevelopmentOnly();
    [HttpPost("simulate-zalo-reply")]
    public IActionResult SimulateZaloReply(DevSimulateZaloReplyRequest request) => DevelopmentOnly();
    [HttpGet("zalo-outbox")]
    public IActionResult ZaloOutbox([FromQuery] string phone) => DevelopmentOnly();
    [HttpPost("trigger-early-vacate")]
    public IActionResult TriggerEarlyVacate(DevTriggerEarlyVacateRequest request) => DevelopmentOnly();
    [HttpPost("advance-clock")]
    public IActionResult AdvanceClock(DevAdvanceClockRequest request) => DevelopmentOnly();
}
