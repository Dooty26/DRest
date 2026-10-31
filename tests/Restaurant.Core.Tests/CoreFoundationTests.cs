using FluentAssertions;
using Restaurant.Core;
using Xunit;

namespace Restaurant.Core.Tests;

public sealed class CoreFoundationTests
{
    [Fact]
    public void Vietnam_time_converts_to_and_from_utc()
    {
        var utc = new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc);

        var local = VietnamTime.ToLocal(utc);

        local.Should().Be(new DateTime(2026, 10, 31, 19, 0, 0, DateTimeKind.Unspecified));
        VietnamTime.ToUtc(local).Should().Be(utc);
    }

    [Fact]
    public void Vietnam_calendar_day_has_an_exclusive_utc_range()
    {
        var range = VietnamTime.GetUtcRange(new DateOnly(2026, 10, 31));

        range.Start.Should().Be(new DateTime(2026, 10, 30, 17, 0, 0, DateTimeKind.Utc));
        range.EndExclusive.Should().Be(new DateTime(2026, 10, 31, 17, 0, 0, DateTimeKind.Utc));
    }

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
