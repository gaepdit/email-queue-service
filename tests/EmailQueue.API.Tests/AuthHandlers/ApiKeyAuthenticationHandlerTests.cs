using EmailQueue.API.AuthHandlers;
using EmailQueue.API.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EmailQueue.API.Tests.AuthHandlers;

public class ApiKeyAuthenticationHandlerTests
{
    private ILoggerFactory _loggerFactory;
    private ApiKeyAuthenticationHandler _sut;
    private HttpContext _httpContext;

    [SetUp]
    public void Setup()
    {
        // Set up mocks
        const string testScheme = "TestApiKey";
        var options = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Get(testScheme).Returns(new AuthenticationSchemeOptions());
        _loggerFactory = Substitute.For<ILoggerFactory>();

        // Create HTTP context
        _httpContext = new DefaultHttpContext();

        // Create handler
        _sut = new ApiKeyAuthenticationHandler(options, _loggerFactory, Substitute.For<UrlEncoder>());

        // Initialize handler with HTTP context
        var scheme = new AuthenticationScheme(testScheme, testScheme, typeof(ApiKeyAuthenticationHandler));
        _sut.InitializeAsync(scheme, _httpContext).GetAwaiter().GetResult();
    }

    [TearDown]
    public void Cleanup()
    {
        // Clean up any test API keys
        AppSettings.ApiKeys.Clear();

        // Dispose of ILoggerFactory
        _loggerFactory.Dispose();
    }

    [Test]
    public async Task HandleAuthenticateAsync_WithMissingApiKey_ReturnsFailure()
    {
        // Act
        var result = await _sut.AuthenticateAsync();

        // Assert
        using var scope = new AssertionScope();
        result.Succeeded.Should().BeFalse();
        result.Failure!.Message.Should().Be("API Key is missing");
    }

    [Test]
    public async Task HandleAuthenticateAsync_WithEmptyApiKey_ReturnsFailure()
    {
        // Arrange
        _httpContext.Request.Headers[ApiKeyAuthenticationHandler.ApiKeyHeaderName] = string.Empty;

        // Act
        var result = await _sut.AuthenticateAsync();

        // Assert
        using var scope = new AssertionScope();
        result.Succeeded.Should().BeFalse();
        result.Failure!.Message.Should().Be("API Key is empty");
    }

    [Test]
    public async Task HandleAuthenticateAsync_WithInvalidApiKey_ReturnsFailure()
    {
        // Arrange
        _httpContext.Request.Headers[ApiKeyAuthenticationHandler.ApiKeyHeaderName] = "invalid-key";
        AppSettings.ApiKeys.Clear();

        // Act
        var result = await _sut.AuthenticateAsync();

        // Assert
        using var scope = new AssertionScope();
        result.Succeeded.Should().BeFalse();
        result.Failure!.Message.Should().Be("Invalid API Key");
    }

    [Test]
    public async Task HandleAuthenticateAsync_WithValidApiKey_ReturnsSuccess()
    {
        // Arrange
        const string apiKey = "test-api-key";
        var testKey = new ApiKeyConfig
        {
            Key = apiKey,
            Owner = "Test Owner",
            Permissions = ["read", "write"],
        };

        AppSettings.ApiKeys.Clear();
        AppSettings.ApiKeys.Add(testKey);
        _httpContext.Request.Headers[ApiKeyAuthenticationHandler.ApiKeyHeaderName] = apiKey;

        // Act
        var result = await _sut.AuthenticateAsync();

        // Assert
        using var scope = new AssertionScope();
        result.Succeeded.Should().BeTrue();
        var identity = result.Principal?.Identity as ClaimsIdentity;
        identity.Should().NotBeNull();
        Debug.Assert(identity != null);
        identity.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == testKey.Owner);
        identity.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == testKey.Key);
        identity.Claims.Should()
            .Contain(c => c.Type == ApiKeyAuthenticationHandler.PermissionClaimType && c.Value == "read");
        identity.Claims.Should()
            .Contain(c => c.Type == ApiKeyAuthenticationHandler.PermissionClaimType && c.Value == "write");
    }
}
