using EMS.API.DTOs.Auth;
using EMS.API.Models;
using EMS.API.Repositories;
using EMS.API.Services.Contracts;
using EMS.API.Services.Implementations;
using Moq;

namespace EMS.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IAppUserRepository> _userRepository = null!;
    private Mock<IJwtTokenService> _jwtTokenService = null!;
    private AuthService _authService = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = new Mock<IAppUserRepository>();
        _jwtTokenService = new Mock<IJwtTokenService>();
        _authService = new AuthService(_userRepository.Object, _jwtTokenService.Object);
    }

    [Test]
    public async Task RegisterAsync_ShouldCreateUser_WhenUsernameIsAvailable()
    {
        var request = new RegisterRequestDto
        {
            Username = "newadmin",
            Password = "secure123",
            Role = UserRole.Admin
        };

        _userRepository.Setup(repository => repository.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser?)null);

        AppUser? savedUser = null;
        _userRepository.Setup(repository => repository.AddAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()))
            .Callback<AppUser, CancellationToken>((user, _) =>
            {
                user.Id = 99;
                savedUser = user;
            })
            .Returns(Task.CompletedTask);

        var result = await _authService.RegisterAsync(request);

        Assert.That(result.Id, Is.EqualTo(99));
        Assert.That(result.Role, Is.EqualTo(UserRole.Admin));
        Assert.That(savedUser, Is.Not.Null);
        Assert.That(savedUser!.PasswordHash, Is.Not.EqualTo(request.Password));
        Assert.That(BCrypt.Net.BCrypt.Verify(request.Password, savedUser.PasswordHash), Is.True);
        _userRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void RegisterAsync_ShouldThrow_WhenUsernameAlreadyExists()
    {
        _userRepository.Setup(repository => repository.GetByUsernameAsync("existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppUser { Id = 1, Username = "existing", Role = UserRole.Viewer });

        var request = new RegisterRequestDto
        {
            Username = "existing",
            Password = "secure123",
            Role = UserRole.Viewer
        };

        Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
    }

    [Test]
    public async Task LoginAsync_ShouldReturnJwtResponse_WhenPasswordMatches()
    {
        var user = new AppUser
        {
            Id = 10,
            Username = "viewer",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("viewer123"),
            Role = UserRole.Viewer,
            CreatedAt = DateTime.UtcNow
        };

        var expectedResponse = new AuthResponseDto
        {
            Token = "jwt-token",
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        };

        _userRepository.Setup(repository => repository.GetByUsernameAsync(user.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtTokenService.Setup(service => service.GenerateToken(user))
            .Returns(expectedResponse);

        var result = await _authService.LoginAsync(new LoginRequestDto
        {
            Username = user.Username,
            Password = "viewer123"
        });

        Assert.That(result.Token, Is.EqualTo("jwt-token"));
        Assert.That(result.User.Username, Is.EqualTo(user.Username));
    }
}
