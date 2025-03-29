using IPS.Grow.Infra.AccessControl;
using IPS.Grow.Infra.Models;
using IPS.Grow.Infra.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace IPS.Grow.Web.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
[Authorize]
public class BaseApiController : ControllerBase
{
    /// <summary>
    /// Not found
    /// </summary>
    /// <returns></returns>
    protected internal new IActionResult NotFound()
    {
        var result = new ApiErrorResponse(ErrorMessages.NotFound, StatusCodes.Status404NotFound, HttpContext.TraceIdentifier)
        {
            Detail = ErrorMessages.NotFoundDetail,
        };
        return new ObjectResult(result)
        {
            StatusCode = result.StatusCode
        };
    }

    /// <summary>
    /// Unauthorized
    /// </summary>
    /// <returns></returns>
    protected internal new IActionResult Unauthorized() => Unauthorized(ErrorMessages.UnauthorizedDetail);

    /// <summary>
    /// Unauthorized
    /// </summary>
    /// <param name="detail"></param>
    /// <returns></returns>
    protected internal IActionResult Unauthorized(string detail)
    {
        var result = new ApiErrorResponse(ErrorMessages.Unauthorized, StatusCodes.Status401Unauthorized, HttpContext.TraceIdentifier)
        {
            Detail = detail
        };
        return new ObjectResult(result)
        {
            StatusCode = result.StatusCode
        };
    }

    /// <summary>
    /// Forbidden response
    /// </summary>
    /// <returns></returns>
    protected internal IActionResult Forbidden()
    {
        var result = new ApiErrorResponse(ErrorMessages.Forbidden, StatusCodes.Status403Forbidden, HttpContext.TraceIdentifier)
        {
            Detail = ErrorMessages.UnauthorizedDetail,
        };
        return new ObjectResult(result)
        {
            StatusCode = result.StatusCode
        };
    }

    /// <summary>
    /// Internal Server Error
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    protected internal IActionResult ServerError(Exception exception) => ServerError(exception.Message);

    protected internal IActionResult ServerError(string message)
    {
        var result = new ApiErrorResponse(ErrorMessages.ServerError, StatusCodes.Status500InternalServerError, HttpContext.TraceIdentifier)
        {
            Detail = message,
        };

        return new ObjectResult(result)
        {
            StatusCode = result.StatusCode
        };
    }

    /// <summary>
    /// Remote Ip Address
    /// </summary>
    protected string IpAddress
    {
        get
        {
            if (Request.Headers.TryGetValue("X-Forwarded-For", out Microsoft.Extensions.Primitives.StringValues value))
                return value!;
            else
                return HttpContext!.Connection!.RemoteIpAddress!.MapToIPv4().ToString();
        }
    }

    public string Username => HttpContext.User.GetString(JwtRegisteredClaimNames.NameId)!;
    public string UserId => HttpContext.User.GetString(JwtRegisteredClaimNames.UniqueName)!;
    public Uri CurrentDomain => new Uri($"{Request.Scheme}://{Request.Host}", UriKind.RelativeOrAbsolute);
    protected const int SeqNo = 1; // TODO if client require (extract from header to support multiple tenants)
}