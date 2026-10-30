using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Contracts;

namespace Restaurant.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[ProducesResponseType<ApiProblemDto>(StatusCodes.Status501NotImplemented)]
public sealed class AuthController : StubControllerBase
{
    [AllowAnonymous, HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    public IActionResult Login(LoginRequest request) => NotImplementedResult();

    [Authorize, HttpGet("me")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    public IActionResult Me() => NotImplementedResult();
}

[ApiController]
[Authorize]
[Route("api/v1")]
[ProducesResponseType<ApiProblemDto>(StatusCodes.Status501NotImplemented)]
public sealed class StaffOperationsController : StubControllerBase
{
    [HttpGet("floor")]
    [ProducesResponseType<FloorPlanDto>(StatusCodes.Status200OK)]
    public IActionResult Floor([FromQuery] int? areaId) => NotImplementedResult();

    [HttpPost("tables/{id:int}/open")]
    public IActionResult OpenTable(int id, OpenTableRequest request) => NotImplementedResult();
    [HttpPost("tables/{id:int}/mark-cleaned")]
    public IActionResult MarkCleaned(int id) => NotImplementedResult();
    [HttpPost("tables/{id:int}/request-cleaning")]
    public IActionResult RequestCleaning(int id) => NotImplementedResult();
    [HttpPost("tables/{id:int}/move")]
    public IActionResult MoveTable(int id, MoveTableRequest request) => NotImplementedResult();
    [HttpPost("tables/merge")]
    public IActionResult MergeTables(MergeTablesRequest request) => NotImplementedResult();
    [HttpPost("tables/{id:int}/unmerge")]
    public IActionResult UnmergeTable(int id) => NotImplementedResult();

    [HttpGet("reservations")]
    [ProducesResponseType<IReadOnlyList<ReservationSummaryDto>>(StatusCodes.Status200OK)]
    public IActionResult Reservations([FromQuery] DateOnly? date, [FromQuery] ReservationStatus? status, [FromQuery] string? q) => NotImplementedResult();
    [HttpPost("reservations")]
    public IActionResult CreateStaffReservation(CreateReservationRequest request) => NotImplementedResult();
    [HttpGet("reservations/{id:int}")]
    [ProducesResponseType<ReservationDetailDto>(StatusCodes.Status200OK)]
    public IActionResult Reservation(int id) => NotImplementedResult();
    [HttpPost("reservations/{id:int}/check-in")]
    public IActionResult CheckIn(int id, [FromBody] object request) => NotImplementedResult();
    [HttpPost("reservations/{id:int}/cancel")]
    public IActionResult Cancel(int id, [FromBody] object request) => NotImplementedResult();
    [HttpPost("reservations/{id:int}/no-show")]
    public IActionResult NoShow(int id) => NotImplementedResult();
    [HttpPut("reservations/{id:int}/guest-count")]
    [ProducesResponseType<GuestCountResponse>(StatusCodes.Status200OK)]
    public IActionResult GuestCount(int id, GuestCountRequest request) => NotImplementedResult();
    [HttpPost("reservations/{id:int}/apply-apology")]
    public IActionResult ApplyApology(int id) => NotImplementedResult();
    [HttpPost("reservations/{id:int}/refund-record")]
    public IActionResult RecordRefund(int id, RefundRecordRequest request) => NotImplementedResult();

    [HttpGet("waitlist")]
    [ProducesResponseType<IReadOnlyList<WaitlistEntryDto>>(StatusCodes.Status200OK)]
    public IActionResult Waitlist() => NotImplementedResult();
    [HttpPost("waitlist")]
    public IActionResult AddWaitlist(AddWaitlistRequest request) => NotImplementedResult();
    [HttpPost("waitlist/{id:int}/notify")]
    public IActionResult NotifyWaitlist(int id) => NotImplementedResult();
    [HttpPost("waitlist/{id:int}/seat")]
    public IActionResult SeatWaitlist(int id, SeatWaitlistRequest request) => NotImplementedResult();
    [HttpPost("waitlist/{id:int}/cancel")]
    public IActionResult CancelWaitlist(int id) => NotImplementedResult();

    [HttpGet("orders/{id:int}")]
    [ProducesResponseType<OrderDto>(StatusCodes.Status200OK)]
    public IActionResult Order(int id) => NotImplementedResult();
    [HttpPost("orders/{id:int}/items")]
    public IActionResult AddOrderItems(int id, StaffOrderItemsRequest request) => NotImplementedResult();
    [HttpPatch("orders/{id:int}/items/{detailId:int}")]
    public IActionResult UpdateOrderItem(int id, int detailId, UpdateOrderItemRequest request) => NotImplementedResult();
    [HttpPost("orders/{id:int}/request-bill")]
    public IActionResult RequestBill(int id) => NotImplementedResult();

    [HttpGet("kitchen/tickets")]
    [ProducesResponseType<IReadOnlyList<KitchenTicketModel>>(StatusCodes.Status200OK)]
    public IActionResult KitchenTickets([FromQuery] OrderItemStatus? status) => NotImplementedResult();
    [HttpPost("kitchen/items/{detailId:int}/status")]
    public IActionResult UpdateKitchenStatus(int detailId, UpdateKitchenStatusRequest request) => NotImplementedResult();
    [HttpPost("menu/items/{id:int}/availability")]
    public IActionResult MenuAvailability(int id, AvailabilityRequest request) => NotImplementedResult();

    [HttpGet("orders/{id:int}/invoice-preview")]
    [ProducesResponseType<InvoiceDto>(StatusCodes.Status200OK)]
    public IActionResult InvoicePreview(int id, [FromQuery] string? voucherCode, [FromQuery] decimal? discountPercent) => NotImplementedResult();
    [HttpPost("orders/{id:int}/pay")]
    [ProducesResponseType<InvoiceDto>(StatusCodes.Status200OK)]
    public IActionResult Pay(int id, PayOrderRequest request) => NotImplementedResult();
    [HttpGet("invoices/{id:int}/receipt")]
    [ProducesResponseType<ReceiptModel>(StatusCodes.Status200OK)]
    public IActionResult Receipt(int id) => NotImplementedResult();
    [HttpGet("orders/{id:int}/kitchen-ticket")]
    [ProducesResponseType<KitchenTicketModel>(StatusCodes.Status200OK)]
    public IActionResult KitchenTicket(int id, [FromQuery] int[]? detailIds) => NotImplementedResult();

    [HttpGet("switch-requests")]
    [ProducesResponseType<IReadOnlyList<SwitchRequestDto>>(StatusCodes.Status200OK)]
    public IActionResult SwitchRequests([FromQuery] SwitchStatus? status) => NotImplementedResult();
    [HttpGet("switch-requests/{id:int}")]
    [ProducesResponseType<SwitchRequestDto>(StatusCodes.Status200OK)]
    public IActionResult SwitchRequest(int id) => NotImplementedResult();
    [HttpPost("switch-requests/{id:int}/staff-resolve")]
    public IActionResult ResolveSwitch(int id, StaffResolveSwitchRequest request) => NotImplementedResult();
}
