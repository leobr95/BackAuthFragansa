using System.Security.Claims;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Debts.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Auth.Api.Controllers;

/// <summary>
/// JWT Authentication endpoints: register, login, refresh y perfil (/me).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(IAuthService svc) : ControllerBase
{
    /// <summary>Registra un usuario y emite tokens.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Register user and issue tokens")]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [SwaggerRequestExample(typeof(RegisterRequest), typeof(RegisterRequestExample))]
    [SwaggerResponseExample(201, typeof(AuthResponseExample))]
    [SwaggerResponseExample(400, typeof(ErrorResponseExample))]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var (ok, res, errors) = await svc.RegisterAsync(req, ct);
        if (!ok) return BadRequest(new ErrorResponse(errors ?? new[] { "Invalid data." }));
        return Created("", res);
    }

    /// <summary>Autentica y emite tokens (access + refresh).</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Login and issue tokens")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
    [SwaggerResponseExample(200, typeof(AuthResponseExample))]
    [SwaggerResponseExample(401, typeof(ErrorResponseExample))]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var (ok, res, errors) = await svc.LoginAsync(req, ct);
        if (!ok) return Unauthorized(new ErrorResponse(errors ?? new[] { "Invalid credentials." }));
        return Ok(res);
    }


    /// <summary>Perfil del usuario autenticado.</summary>
    [Authorize]
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Current authenticated user")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(void), 401)]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var sub = User.FindFirstValue("sub")
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(ClaimTypes.Sid)
                  ?? User.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var userId))
            return Unauthorized();

        var me = await svc.GetMeAsync(userId, ct);
        return me is null ? NotFound() : Ok(me);
    }
}


