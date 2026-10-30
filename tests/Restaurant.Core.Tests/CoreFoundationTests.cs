using FluentAssertions;
using Restaurant.Core;
using Xunit;

namespace Restaurant.Core.Tests;

public sealed class CoreFoundationTests
{
    [Fact]
    public async Task Fake_storage_round_trips_without_network()
    {
        var storage = new FakeFileStorage();
        await using var stream = new MemoryStream([1, 2, 3]);
        var cancellationToken = TestContext.Current.CancellationToken;
        var stored = await storage.UploadAsync("menu-images", "items/1/test.webp", stream, "image/webp", cancellationToken);
        stored.Url.Should().Be("https://storage.invalid/menu-images/items/1/test.webp");
        storage.Paths.Should().ContainSingle();
        await storage.DeleteAsync("menu-images", stored.Path, cancellationToken);
        storage.Paths.Should().BeEmpty();
    }
}
