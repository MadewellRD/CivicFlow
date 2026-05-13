namespace CivicFlow.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
