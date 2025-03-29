using IPS.Grow.Infra.Models;
using IPS.Grow.Infra.Resources;
using IPS.Grow.Infra.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPS.Grow.Web.Server.Controllers;

/// <summary>
/// Auth Controller
/// </summary>
/// <param name="authService"></param>
[Route("api/auth")]
public class AuthController(IAuthService authService) : BaseApiController
{
    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginAsync(LoginModel user, CancellationToken ct = default)
    {
        var maybe = await authService.LoginAsync(user, IpAddress, ct);
        return maybe.Match(user =>
        {
            RefreshToken = user.RefreshToken!;
            return Ok(user);
        }, () => Unauthorized(ErrorMessages.InvalidCredential));
    }

    /// <summary>
    /// Refresh user token from http cookie
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshTokenAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(RefreshToken))
            return NoContent();
        //
        var res = await authService.RefreshTokenAsync(RefreshToken, Username, IpAddress, ct);
        return res.Match(token =>
        {
            RefreshToken = token.RefreshToken!;
            return Ok(token);
        }, () => Unauthorized(ErrorMessages.SessionExpired));
    }

    /// <summary>
    /// Revoke user token
    /// </summary>
    /// <returns></returns>

    [Authorize]
    [HttpPost("revoke-token")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeTokenAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(RefreshToken))
            return Unauthorized(ErrorMessages.TokenExpired);
        //
        var response = await authService.RevokeTokenAsync(RefreshToken, Username, IpAddress, ct);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("user")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddUserAsync(NewUserModel user, CancellationToken ct = default)
    {
        var res = await authService.CreateUserAsync(user, ct);
        return Ok(res);
    }

    #region Privates

    private string? RefreshToken
    {
        get
        {
            return Request.Cookies[JwtSettings.RefreshTokenKey];
        }
        set
        {
            if (value is not null)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddDays(1)
                };
                Response.Cookies.Append(JwtSettings.RefreshTokenKey, value, cookieOptions);
            }
            else
            {
                Response.Cookies.Delete(JwtSettings.RefreshTokenKey);
            }
        }
    }

    #endregion
}