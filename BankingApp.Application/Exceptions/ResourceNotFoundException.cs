namespace BankingApp.Application.Exceptions;

/// <summary>
/// Thrown when a requested resource (account, customer, etc.) is not found
/// </summary>
public class ResourceNotFoundException : Exception
{
    public string? ResourceType { get; }
    public object? ResourceId { get; }

    public ResourceNotFoundException(string message) : base(message)
    {
    }

    public ResourceNotFoundException(string resourceType, object resourceId)
        : base($"{resourceType} with ID {resourceId} not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}
