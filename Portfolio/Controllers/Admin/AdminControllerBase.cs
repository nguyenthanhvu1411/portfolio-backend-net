using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Portfolio.Controllers.Admin;

public abstract class AdminControllerBase : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out var userId) || userId <= 0)
        {
            throw new UnauthorizedAccessException(
                "Không xác định được người dùng từ access token.");
        }

        return userId;
    }
}
