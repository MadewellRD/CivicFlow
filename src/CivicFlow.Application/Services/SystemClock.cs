using CivicFlow.Application.Abstractions;

namespace CivicFlow.Application.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
