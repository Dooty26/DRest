using Microsoft.AspNetCore.Mvc;
using Restaurant.Contracts;

namespace Restaurant.Api.Controllers;

public abstract class StubControllerBase : ControllerBase
{
    protected ObjectResult NotImplementedResult() => StatusCode(StatusCodes.Status501NotImplemented, new ApiProblemDto(
        "https://httpstatuses.com/501",
        "Chức năng chưa được triển khai",
        StatusCodes.Status501NotImplemented,
        "Endpoint đã có trong hợp đồng C0 và sẽ được hiện thực ở các prompt tiếp theo.",
        ErrorCodes.NotImplemented));
}
