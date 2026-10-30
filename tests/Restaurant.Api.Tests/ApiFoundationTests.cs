using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Restaurant.Contracts;
using Xunit;

namespace Restaurant.Api.Tests;

public sealed class ApiFoundationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;
    public ApiFoundationTests(WebApplicationFactory<Program> factory) => client = factory.CreateClient();

    [Fact]
    public async Task Health_endpoint_is_available()
    {
        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Contract_stub_returns_problem_details_with_code()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var response = await client.GetAsync("/api/v1/public/config", cancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        var problem = await response.Content.ReadFromJsonAsync<ApiProblemDto>(cancellationToken);
        problem!.Code.Should().Be(ErrorCodes.NotImplemented);
    }

    [Fact]
    public async Task Swagger_contains_the_contract_surface()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var stream = await client.GetStreamAsync("/swagger/v1/swagger.json", cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var paths = document.RootElement.GetProperty("paths");
        paths.EnumerateObject().Should().HaveCountGreaterThan(50);
        paths.TryGetProperty("/api/v1/public/reservations", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/orders/{id}/pay", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/switch-requests/{id}/staff-resolve", out _).Should().BeTrue();
    }
}
