namespace itinerary_be.UnitTests;

using Moq;
using Microsoft.Extensions.Logging;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Services;

/// <summary>
/// Unit tests for UserService class
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_mockRepository.Object, _mockLogger.Object);
    }

    #region GetOrCreateUserAsync Tests

    [Fact]
    public async Task GetOrCreateUserAsync_ExistingEmail_ReturnsExistingUserAndDoesNotCreate()
    {
        // Arrange
        var email = "existing@example.com";
        var existingUser = new User { Id = Guid.NewGuid(), Email = email, Name = "Existing User", CreatedAt = DateTime.UtcNow };

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.GetOrCreateUserAsync(email, "New Name From Google");

        // Assert
        Assert.Equal(existingUser.Id, result.Id);
        Assert.Equal(existingUser.Name, result.Name);

        _mockRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateUserAsync_NewEmail_CreatesAndReturnsNewUser()
    {
        // Arrange
        var email = "new@example.com";
        var name = "New User";

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetOrCreateUserAsync(email, name);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(email, result.Email);
        Assert.Equal(name, result.Name);

        _mockRepository.Verify(r => r.CreateAsync(It.Is<User>(u => u.Email == email && u.Name == name)), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateUserAsync_RepositoryThrowsException_ExceptionPropagates()
    {
        // Arrange
        var email = "broken@example.com";

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _userService.GetOrCreateUserAsync(email, "Some Name"));
    }

    #endregion
}
