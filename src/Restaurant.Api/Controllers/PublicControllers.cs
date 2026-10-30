using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Contracts;

namespace Restaurant.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/public")]
[Produces("application/json")]
[ProducesResponseType<ApiProblemDto>(StatusCodes.Status501NotImplemented)]
public sealed class PublicController : StubControllerBase
{
    [HttpGet("config")]
    [ProducesResponseType<PublicConfigDto>(StatusCodes.Status200OK)]
    public IActionResult GetConfig() => NotImplementedResult();

    [HttpGet("availability")]
    [ProducesResponseType<AvailabilityDto>(StatusCodes.Status200OK)]
    public IActionResult GetAvailability([FromQuery] DateOnly date, [FromQuery] int partySize) => NotImplementedResult();

    [HttpPost("reservations")]
    [ProducesResponseType<CreateReservationResponse>(StatusCodes.Status201Created)]
    public IActionResult CreateReservation([FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey, CreateReservationRequest request) => NotImplementedResult();

    [HttpGet("reservations/{code}")]
    [ProducesResponseType<ReservationLookupDto>(StatusCodes.Status200OK)]
    public IActionResult GetReservation(string code, [FromQuery] string phone) => NotImplementedResult();

    [HttpPost("reservations/{code}/cancel")]
    [ProducesResponseType<CancelReservationResponse>(StatusCodes.Status200OK)]
    public IActionResult CancelReservation(string code, CancelReservationRequest request) => NotImplementedResult();

    [HttpPost("reservations/{code}/reschedule")]
    [ProducesResponseType<CreateReservationResponse>(StatusCodes.Status200OK)]
    public IActionResult RescheduleReservation(string code, RescheduleReservationRequest request) => NotImplementedResult();

    [HttpGet("menu")]
    [ProducesResponseType<PublicMenuDto>(StatusCodes.Status200OK)]
    public IActionResult GetMenu() => NotImplementedResult();

    [HttpGet("tables/{tableCode}/session")]
    [ProducesResponseType<TableSessionDto>(StatusCodes.Status200OK)]
    public IActionResult GetTableSession(string tableCode, [FromQuery] string sig) => NotImplementedResult();

    [HttpPost("tables/{tableCode}/orders")]
    [ProducesResponseType<CreatePublicOrderResponse>(StatusCodes.Status200OK)]
    public IActionResult CreateTableOrder(string tableCode, [FromQuery] string sig, [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey, CreatePublicOrderRequest request) => NotImplementedResult();

    [HttpGet("tables/{tableCode}/orders")]
    [ProducesResponseType<PublicOrderStatusDto>(StatusCodes.Status200OK)]
    public IActionResult GetTableOrder(string tableCode, [FromQuery] string sig) => NotImplementedResult();
}

[ApiController]
[AllowAnonymous]
[Route("api/v1/webhooks")]
[ProducesResponseType<ApiProblemDto>(StatusCodes.Status501NotImplemented)]
public sealed class WebhooksController : StubControllerBase
{
    [HttpPost("payment/{provider}")]
    public IActionResult Payment(string provider, [FromBody] object payload) => NotImplementedResult();

    [HttpPost("zalo")]
    public IActionResult Zalo([FromBody] object payload) => NotImplementedResult();
}
