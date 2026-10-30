using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Contracts;

namespace Restaurant.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/manage")]
[ProducesResponseType<ApiProblemDto>(StatusCodes.Status501NotImplemented)]
public sealed class ManageController : StubControllerBase
{
    [HttpGet("areas")]
    public IActionResult Areas() => NotImplementedResult();
    [HttpPost("areas")]
    public IActionResult CreateArea([FromBody] object request) => NotImplementedResult();
    [HttpPut("areas/{id:int}")]
    public IActionResult UpdateArea(int id, [FromBody] object request) => NotImplementedResult();
    [HttpDelete("areas/{id:int}")]
    public IActionResult DeleteArea(int id) => NotImplementedResult();
    [HttpGet("tables")]
    public IActionResult Tables() => NotImplementedResult();
    [HttpPost("tables")]
    public IActionResult CreateTable([FromBody] object request) => NotImplementedResult();
    [HttpPut("tables/{id:int}")]
    public IActionResult UpdateTable(int id, [FromBody] object request) => NotImplementedResult();
    [HttpDelete("tables/{id:int}")]
    public IActionResult DeleteTable(int id) => NotImplementedResult();
    [HttpPut("tables/{id:int}/layout")]
    public IActionResult Layout(int id, [FromBody] object request) => NotImplementedResult();
    [HttpGet("tables/{id:int}/qr")]
    public IActionResult TableQr(int id) => NotImplementedResult();

    [HttpGet("categories")]
    public IActionResult Categories() => NotImplementedResult();
    [HttpPost("categories")]
    public IActionResult CreateCategory([FromBody] object request) => NotImplementedResult();
    [HttpPut("categories/{id:int}")]
    public IActionResult UpdateCategory(int id, [FromBody] object request) => NotImplementedResult();
    [HttpDelete("categories/{id:int}")]
    public IActionResult DeleteCategory(int id) => NotImplementedResult();
    [HttpGet("menu-items")]
    public IActionResult MenuItems() => NotImplementedResult();
    [HttpPost("menu-items")]
    public IActionResult CreateMenuItem([FromBody] object request) => NotImplementedResult();
    [HttpPut("menu-items/{id:int}")]
    public IActionResult UpdateMenuItem(int id, [FromBody] object request) => NotImplementedResult();
    [HttpDelete("menu-items/{id:int}")]
    public IActionResult DeleteMenuItem(int id) => NotImplementedResult();

    [HttpGet("users")]
    public IActionResult Users() => NotImplementedResult();
    [HttpPost("users")]
    public IActionResult CreateUser([FromBody] object request) => NotImplementedResult();
    [HttpPut("users/{id:int}")]
    public IActionResult UpdateUser(int id, [FromBody] object request) => NotImplementedResult();
    [HttpDelete("users/{id:int}")]
    public IActionResult DeleteUser(int id) => NotImplementedResult();
    [HttpGet("settings")]
    public IActionResult Settings() => NotImplementedResult();
    [HttpPut("settings")]
    public IActionResult UpdateSettings([FromBody] object request) => NotImplementedResult();

    [HttpPost("menu-items/{id:int}/image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<UploadImageResponse>(StatusCodes.Status200OK)]
    public IActionResult UploadMenuImage(int id, IFormFile file) => NotImplementedResult();
    [HttpDelete("menu-items/{id:int}/image")]
    public IActionResult DeleteMenuImage(int id) => NotImplementedResult();
    [HttpPost("areas/{id:int}/floor-plan-image")]
    [Consumes("multipart/form-data")]
    public IActionResult UploadFloorImage(int id, IFormFile file) => NotImplementedResult();
    [HttpDelete("areas/{id:int}/floor-plan-image")]
    public IActionResult DeleteFloorImage(int id) => NotImplementedResult();
    [HttpPost("branding/logo")]
    [Consumes("multipart/form-data")]
    public IActionResult UploadLogo(IFormFile file) => NotImplementedResult();
}

[ApiController]
[Authorize]
[Route("api/v1/inventory")]
[ProducesResponseType<ApiProblemDto>(StatusCodes.Status501NotImplemented)]
public sealed class InventoryController : StubControllerBase
{
    [HttpGet("ingredients")]
    [ProducesResponseType<IReadOnlyList<IngredientDto>>(StatusCodes.Status200OK)]
    public IActionResult Ingredients() => NotImplementedResult();
    [HttpPost("ingredients")]
    public IActionResult CreateIngredient([FromBody] object request) => NotImplementedResult();
    [HttpPut("ingredients/{id:int}")]
    public IActionResult UpdateIngredient(int id, [FromBody] object request) => NotImplementedResult();
    [HttpPost("ingredients/{id:int}/adjust")]
    public IActionResult Adjust(int id, AdjustStockRequest request) => NotImplementedResult();
    [HttpGet("movements")]
    [ProducesResponseType<IReadOnlyList<StockMovementDto>>(StatusCodes.Status200OK)]
    public IActionResult Movements() => NotImplementedResult();
}

[ApiController]
[Authorize]
[Route("api/v1/menu/items")]
public sealed class RecipeController : StubControllerBase
{
    [HttpGet("{id:int}/recipe")]
    [ProducesResponseType<RecipeDto>(StatusCodes.Status200OK)]
    public IActionResult Recipe(int id) => NotImplementedResult();
    [HttpPut("{id:int}/recipe")]
    public IActionResult UpdateRecipe(int id, RecipeDto request) => NotImplementedResult();
}

[ApiController]
[Authorize]
[Route("api/v1/reports")]
[ProducesResponseType<ApiProblemDto>(StatusCodes.Status501NotImplemented)]
public sealed class ReportsController : StubControllerBase
{
    [HttpGet("revenue")]
    public IActionResult Revenue([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] string groupBy = "day") => NotImplementedResult();
    [HttpGet("deposits")]
    public IActionResult Deposits() => NotImplementedResult();
    [HttpGet("top-items")]
    public IActionResult TopItems() => NotImplementedResult();
    [HttpGet("low-stock")]
    public IActionResult LowStock() => NotImplementedResult();
    [HttpGet("cash-fund")]
    public IActionResult CashFund() => NotImplementedResult();
    [HttpGet("items-without-recipe")]
    public IActionResult ItemsWithoutRecipe() => NotImplementedResult();
}
