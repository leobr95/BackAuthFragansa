using System;
using System.Threading.Tasks;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Application.Services;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Auth.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    [Test]
    public async Task Register_Should_Fail_When_Email_Exists()
    {
        var repo = new Mock<IUserRepository>();
        var hasher = new Mock<IPasswordHasher>();
        var jwt = new Mock<IJwtTokenGenerator>();

        repo.Setup(r => r.EmailExistsAsync("a@b.com", default)).ReturnsAsync(true);

        var svc = new AuthService(repo.Object, hasher.Object, jwt.Object);
        var (ok, _, errors) = await svc.RegisterAsync(new RegisterRequest("a@b.com","123456","Test"));

        ok.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("already", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public async Task Login_Should_Fail_With_Wrong_Password()
    {
        var repo = new Mock<IUserRepository>();
        var hasher = new Mock<IPasswordHasher>();
        var jwt = new Mock<IJwtTokenGenerator>();

        var u = new User { UserId = Guid.NewGuid(), Email = "a@b.com", PasswordHash = "hash", FullName = "A", Role = "user" };
        repo.Setup(r => r.GetByEmailAsync("a@b.com", default)).ReturnsAsync(u);
        hasher.Setup(h => h.Verify("bad", "hash")).Returns(false);

        var svc = new AuthService(repo.Object, hasher.Object, jwt.Object);
        var (ok, _, errors) = await svc.LoginAsync(new LoginRequest("a@b.com","bad"));

        ok.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }
}
