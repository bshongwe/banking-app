namespace BankingApp.Application.DTOs;

/// <summary>
/// Pagination metadata for list responses
/// </summary>
public class PaginationDto
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Generic paginated response wrapper
/// </summary>
public class PaginatedResponseDto<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}

/// <summary>
/// Account list item DTO (minimal projection)
/// </summary>
public class AccountListItemDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "Active";
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Transfer list item DTO with full transfer details
/// </summary>
public class TransferListItemDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid SourceAccountId { get; set; }
    public Guid DestinationAccountId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Customer list item DTO
/// </summary>
public class CustomerListItemDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Account detail DTO including customer info
/// </summary>
public class AccountDetailDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "Active";
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Account sub-item within customer detail response
/// </summary>
public class CustomerAccountItemDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Customer detail DTO including accounts
/// </summary>
public class CustomerDetailDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<CustomerAccountItemDto> Accounts { get; set; } = new();
}
