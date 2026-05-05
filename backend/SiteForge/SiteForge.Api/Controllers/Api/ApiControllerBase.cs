using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SiteForge.Core.DTOs;

namespace SiteForge.Api.Controllers.Api;

public abstract class ApiControllerBase : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Ok(data, message));

    protected ActionResult<ApiResponse<T>> NotFoundResponse<T>(string message = "Resource not found.") =>
        NotFound(ApiResponse<T>.Fail(message));
}
