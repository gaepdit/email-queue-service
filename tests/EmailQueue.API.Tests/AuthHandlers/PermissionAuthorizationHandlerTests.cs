using EmailQueue.API.AuthHandlers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EmailQueue.API.Tests.AuthHandlers;

[TestFixture]
public class PermissionAuthorizationHandlerTests
{
    private PermissionAuthorizationHandler _sut;
    private readonly Claim _readClaim = new(ApiKeyAuthenticationHandler.PermissionClaimType, "read");
    private readonly Claim _writeClaim = new(ApiKeyAuthenticationHandler.PermissionClaimType, "write");

    [SetUp]
    public void Setup() => _sut = new PermissionAuthorizationHandler();

    [Test]
    public async Task HandleRequirementAsync_UserHasMatchingPermission_SucceedsRequirement()
    {
        // Arrange
        var requirement = PermissionRequirement.ReadPermission;
        var user = new ClaimsPrincipal(new ClaimsIdentity([_readClaim]));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Test]
    public async Task HandleRequirementAsync_UserHasDifferentPermission_DoesNotSucceedRequirement()
    {
        // Arrange
        var requirement = PermissionRequirement.ReadPermission;
        var user = new ClaimsPrincipal(new ClaimsIdentity([_writeClaim]));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Test]
    public async Task HandleRequirementAsync_UserHasNoPermission_DoesNotSucceedRequirement()
    {
        // Arrange
        var requirement = PermissionRequirement.ReadPermission;
        var user = new ClaimsPrincipal(new ClaimsIdentity([]));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Test]
    public async Task HandleRequirementAsync_UserHasMultiplePermissions_SucceedsBothRequirements()
    {
        // Arrange
        var readRequirement = PermissionRequirement.ReadPermission;
        var writeRequirement = PermissionRequirement.WritePermission;
        var user = new ClaimsPrincipal(new ClaimsIdentity([_readClaim, _writeClaim]));

        var readContext = new AuthorizationHandlerContext([readRequirement], user, null);
        var writeContext = new AuthorizationHandlerContext([writeRequirement], user, null);

        // Act
        await _sut.HandleAsync(readContext);
        await _sut.HandleAsync(writeContext);

        // Assert
        using var scope = new AssertionScope();
        readContext.HasSucceeded.Should().BeTrue();
        writeContext.HasSucceeded.Should().BeTrue();
    }

    [Test]
    public void HandleRequirementAsync_NullUser_ThrowsNullReferenceException()
    {
        // Arrange
        var requirement = PermissionRequirement.ReadPermission;
        var context = new AuthorizationHandlerContext([requirement], null!, null);

        // Act
        var func = async () => await _sut.HandleAsync(context);

        // Act & Assert
        func.Should().ThrowAsync<NullReferenceException>();
    }

    [Test]
    public async Task HandleRequirementAsync_EmptyRequirements_DoesNotSucceedRequirement()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity([_readClaim]));
        var context = new AuthorizationHandlerContext(requirements: [], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Test]
    public async Task HandleRequirementAsync_CaseInsensitivePermissionMatch_SucceedsRequirement()
    {
        // Arrange
        var requirement = PermissionRequirement.ReadPermission;
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ApiKeyAuthenticationHandler.PermissionClaimType, "READ"),
        ]));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Test]
    public async Task HandleRequirementAsync_MultipleRequirementsInSingleContext_DoesNotSucceedWithAllPermissions()
    {
        // Arrange
        var readRequirement = PermissionRequirement.ReadPermission;
        var writeRequirement = PermissionRequirement.WritePermission;
        var user = new ClaimsPrincipal(new ClaimsIdentity([_readClaim]));
        var context = new AuthorizationHandlerContext([readRequirement, writeRequirement], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Test]
    public async Task HandleRequirementAsync_MultipleRequirementsInSingleContext_SucceedsWithAllPermissions()
    {
        // Arrange
        var readRequirement = PermissionRequirement.ReadPermission;
        var writeRequirement = PermissionRequirement.WritePermission;
        var user = new ClaimsPrincipal(new ClaimsIdentity([_readClaim, _writeClaim]));
        var context = new AuthorizationHandlerContext([readRequirement, writeRequirement], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }
}
