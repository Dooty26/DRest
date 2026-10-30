using Restaurant.Contracts;

namespace Restaurant.Core;

public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock(TimeProvider timeProvider) : IClock
{
    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;
}

public sealed record StoredFile(string Url, string Path);

public interface IFileStorage
{
    Task<StoredFile> UploadAsync(string bucket, string path, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string bucket, string path, CancellationToken cancellationToken = default);
}

public sealed class FakeFileStorage : IFileStorage
{
    private readonly Dictionary<string, byte[]> files = new(StringComparer.Ordinal);
    public IReadOnlyCollection<string> Paths => files.Keys;

    public async Task<StoredFile> UploadAsync(string bucket, string path, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        files[$"{bucket}/{path}"] = buffer.ToArray();
        return new StoredFile($"https://storage.invalid/{bucket}/{path}", path);
    }

    public Task DeleteAsync(string bucket, string path, CancellationToken cancellationToken = default)
    {
        files.Remove($"{bucket}/{path}");
        return Task.CompletedTask;
    }
}

public interface IRealtimePublisher
{
    Task PublishAsync<T>(string eventName, T payload, CancellationToken cancellationToken = default);
}
