using Auth.Application.DTOs;
using Debts.Api.Contracts;
using Swashbuckle.AspNetCore.Filters;


namespace Auth.Api.Controllers;

public class RegisterRequestExample : IExamplesProvider<UserDto>
{
    public UserDto GetExamples() => new(
        UserId: Guid.NewGuid(),
        Email: "user@example.com",
        FullName: "User Name",
        Role: "user"
    );
}

public class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples() => new(
        Email: "user@example.com",
        Password: "Secret123!"
    );
}


public class AuthResponseExample : IExamplesProvider<AuthResponse>
{
    public AuthResponse GetExamples() => new(
        AccessToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        ExpiresAt: DateTime.UtcNow.AddMinutes(30),
        RefreshToken: "XyZBase64RefreshTokenHere...",
        RefreshExpiresAt: DateTime.UtcNow.AddDays(7),
        User: new UserDto(
            UserId: Guid.Parse("7e61b9fa-26a2-4b07-9f05-9783b6b6f1e3"),
            Email: "user@example.com",
            FullName: "User Name",
            Role: "user"
        )
    );
}

public class ErrorResponseExample : IExamplesProvider<ErrorResponse>
{
    public ErrorResponse GetExamples() => new(new[] { "Title is required.", "Amount must be positive." });
}

public class NotFoundMessageExample : IExamplesProvider<MessageResponse>
{
    public MessageResponse GetExamples() => new("Debt not found.");
}
