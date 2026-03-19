# Banking App API

A modern, production-ready banking application built with ASP.NET Core 10.0, featuring comprehensive account management, money transfer capabilities, and complete transaction history tracking.

## Overview

This is a full-stack banking application that demonstrates clean architecture principles, CQRS pattern implementation, and enterprise-grade error handling. The API provides secure endpoints for customer management, account operations, and inter-account transfers with double-entry bookkeeping ledger support.

### Key Features

- ✅ **Customer Management** - Create and retrieve customer profiles
- ✅ **Account Management** - Create accounts with optional initial balance
- ✅ **Money Transfers** - Atomic transfer operations between accounts with race condition prevention
- ✅ **Transaction History** - Paginated ledger entries with deterministic sorting
- ✅ **Balance Queries** - Real-time account balance calculations
- ✅ **Double-Entry Bookkeeping** - Complete audit trail via ledger entries
- ✅ **Transaction Safety** - Database transactions ensure data consistency
- ✅ **Interactive API Documentation** - Swagger UI interface for testing endpoints
- ✅ **Banking-Grade Error Handling** - 18 domain-specific error codes with HTTP status codes
- ✅ **Production-Ready** - Clean architecture, comprehensive documentation, zero build warnings

---

## Architecture

### API Architecture

```mermaid
graph TB
    subgraph Client["Client Layer"]
        Browser["Web Browser / Postman"]
    end
    
    subgraph API["ASP.NET Core API Layer"]
        Controllers["Controllers<br/>(Routing & HTTP)"]
        ExceptionHandling["Exception Handling<br/>(Error Responses)"]
    end
    
    subgraph Application["Application Layer - CQRS"]
        CommandHandlers["Command Handlers<br/>- CreateCustomer<br/>- CreateAccount<br/>- TransferMoney"]
        QueryHandlers["Query Handlers<br/>- GetCustomer<br/>- GetAccountBalance<br/>- GetTransactionHistory"]
        Validators["Validators<br/>(FluentValidation)"]
        UnitOfWork["Unit of Work<br/>(Transaction Management)"]
    end
    
    subgraph Domain["Domain Layer"]
        Entities["Entities<br/>- Customer<br/>- Account<br/>- Transaction<br/>- LedgerEntry"]
        ValueObjects["Value Objects<br/>- Money"]
        DomainEvents["Domain Events<br/>- TransferCompleted<br/>- AccountCreated"]
        Specifications["Specifications<br/>- AccountHasSufficientFunds<br/>- ValidEmail"]
    end
    
    subgraph Infrastructure["Infrastructure Layer"]
        Repositories["Repositories<br/>- AccountRepository<br/>- LedgerRepository<br/>- TransactionRepository"]
        DbContext["EF Core DbContext<br/>(BankingDbContext)"]
        Database["SQLite Database"]
    end
    
    Browser -->|HTTP| Controllers
    Controllers --> ExceptionHandling
    Controllers --> CommandHandlers
    Controllers --> QueryHandlers
    CommandHandlers --> Validators
    CommandHandlers --> UnitOfWork
    QueryHandlers --> Repositories
    UnitOfWork --> Repositories
    Repositories --> DbContext
    DbContext --> Database
    Entities -.->|Referenced by| CommandHandlers
    Domain -.->|Used by| Application
```

### Data Flow - Money Transfer Example

```mermaid
sequenceDiagram
    participant Client as Client
    participant API as API Controller
    participant Handler as TransferMoneyCommandHandler
    participant UoW as Unit of Work
    participant Repo as Repository
    participant DB as Database
    
    Client->>API: POST /api/transfers
    API->>Handler: HandleAsync(command)
    Handler->>UoW: BeginTransactionAsync()
    UoW->>DB: BEGIN TRANSACTION
    
    Handler->>Repo: GetBalanceAsync(fromAccount)
    Repo->>DB: Query LedgerEntries
    DB-->>Repo: Current Balance
    Repo-->>Handler: 5000.00
    
    Note over Handler: Validate balance
    Handler->>Handler: Create Transaction & Ledger Entries
    
    Handler->>Repo: AddAsync(transaction)
    Handler->>Repo: AddRangeAsync(debitEntry, creditEntry)
    Handler->>Repo: SaveChangesAsync()
    Repo->>DB: INSERT transaction, ledger entries
    
    Handler->>UoW: CommitTransactionAsync()
    UoW->>DB: COMMIT
    DB-->>UoW: Success
    
    Handler-->>API: Transaction
    API-->>Client: 201 Created + Transaction Details
```

---

## Database Schema

```mermaid
erDiagram
    CUSTOMERS ||--o{ ACCOUNTS : has
    ACCOUNTS ||--o{ LEDGER_ENTRIES : has
    TRANSACTIONS ||--o{ LEDGER_ENTRIES : creates
    
    CUSTOMERS {
        string Id PK
        string FirstName
        string LastName
        string Email UK
        datetime CreatedAt
    }
    
    ACCOUNTS {
        string Id PK
        string CustomerId FK
        string AccountNumber UK
        string Currency
        datetime CreatedAt
    }
    
    TRANSACTIONS {
        string Id PK
        string Reference
        datetime CreatedAt
    }
    
    LEDGER_ENTRIES {
        string Id PK
        string TransactionId FK "nullable"
        string AccountId FK
        decimal Amount
        string EntryType
        datetime CreatedAt
    }
```

### Schema Details

**CUSTOMERS Table**
- `Id` (GUID) - Primary key
- `FirstName` (TEXT) - Customer's first name
- `LastName` (TEXT) - Customer's last name
- `Email` (TEXT) - Unique email address
- `CreatedAt` (DATETIME) - Timestamp

**ACCOUNTS Table**
- `Id` (GUID) - Primary key
- `CustomerId` (GUID) - Foreign key to Customers
- `AccountNumber` (TEXT) - Unique account number
- `Currency` (TEXT) - ISO 4217 currency code (default: USD)
- `CreatedAt` (DATETIME) - Timestamp

**TRANSACTIONS Table**
- `Id` (GUID) - Primary key
- `Reference` (TEXT) - Transaction reference
- `CreatedAt` (DATETIME) - Timestamp

**LEDGER_ENTRIES Table**
- `Id` (GUID) - Primary key
- `TransactionId` (GUID) - Foreign key to Transactions (nullable for initial balances)
- `AccountId` (GUID) - Foreign key to Accounts
- `Amount` (DECIMAL) - Entry amount
- `EntryType` (TEXT) - "Debit" or "Credit"
- `CreatedAt` (DATETIME) - Timestamp

---

## Error Handling

The API implements a comprehensive, production-grade error handling system with banking-specific error codes.

### HTTP Status Codes

| Code | Meaning | Banking Example |
|------|---------|-----------------|
| **200** | Request succeeded | Fetch account balance |
| **201** | Resource created | New bank account or transfer created |
| **400** | Bad request | Invalid transfer amount |
| **404** | Not found | Account or customer does not exist |
| **409** | Conflict | Duplicate account number |
| **422** | Unprocessable entity | Insufficient funds |
| **500** | Server error | Unexpected exception |

### Banking Error Codes

Every error response includes a domain-specific error code (in addition to HTTP status):

| Code | Meaning | HTTP Status |
|------|---------|------------|
| 4000 | Validation failed | 400 |
| 4001 | Account not found | 404 |
| 4003 | Duplicate account number | 409 |
| 4004 | Account frozen | 422 |
| 4005 | Insufficient funds | 422 |
| 4006 | Invalid transfer amount | 400 |
| 4008 | Currency mismatch | 422 |
| 4010 | Customer not found | 404 |
| 4011 | Duplicate email | 409 |
| 5000 | Internal server error | 500 |

### Error Response Format

All error responses follow a standardized format:

```json
{
  "message": "Insufficient funds.",
  "errorCode": 4005,
  "statusCode": 422,
  "traceId": "0HN6O5RA2VDCF:00000001"
}
```

**Fields**:
- `message` - Generic error message (no sensitive data)
- `errorCode` - Banking-specific error code for programmatic handling
- `statusCode` - HTTP status code
- `traceId` - Unique identifier for support/logging correlation

### Error Examples

**Insufficient Funds (422)**:
```bash
curl -X POST http://localhost:5242/api/transfers \
  -H "Content-Type: application/json" \
  -d '{"fromAccountId":"...","toAccountId":"...","amount":10000}'

# Response:
{
  "message": "Insufficient funds.",
  "errorCode": 4005,
  "statusCode": 422,
  "traceId": "..."
}
```

**Duplicate Account Number (409)**:
```bash
curl -X POST http://localhost:5242/api/accounts \
  -H "Content-Type: application/json" \
  -d '{"customerId":"...","accountNumber":"ACC001"}'

# Response:
{
  "message": "Account number 'ACC001' already exists.",
  "errorCode": 4003,
  "statusCode": 409,
  "traceId": "..."
}
```

**Invalid Transfer Amount (400)**:
```bash
curl -X POST http://localhost:5242/api/transfers \
  -H "Content-Type: application/json" \
  -d '{"fromAccountId":"...","toAccountId":"...","amount":-100}'

# Response:
{
  "message": "Transfer amount must be greater than zero.",
  "errorCode": 4006,
  "statusCode": 400,
  "traceId": "..."
}
```

**Customer Not Found (404)**:
```bash
curl -X GET http://localhost:5242/api/customers/00000000-0000-0000-0000-000000000000

# Response:
{
  "message": "Customer not found.",
  "errorCode": 4010,
  "statusCode": 404,
  "traceId": "..."
}
```

### Complete Error Documentation

For comprehensive error handling documentation including all 18 banking error codes, development guide, and testing patterns:

📖 **[ERROR_HANDLING.md](./ERROR_HANDLING.md)** - Complete error handling guide  
📖 **[ERROR_QUICK_REFERENCE.md](./ERROR_QUICK_REFERENCE.md)** - Quick error code lookup  
📖 **[ERROR_CODES_MATRIX.md](./ERROR_CODES_MATRIX.md)** - Error code reference matrix  
📖 **[BEFORE_AFTER_COMPARISON.md](./BEFORE_AFTER_COMPARISON.md)** - Implementation details  
📖 **[ERROR_HANDLING_INDEX.md](./ERROR_HANDLING_INDEX.md)** - Master index

---

## Getting Started

### Prerequisites

- **.NET 10.0 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Git** - For version control

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/bshongwe/banking-app.git
cd banking-app
```

2. **Restore dependencies**
```bash
dotnet restore
```

3. **Build the project**
```bash
dotnet build
```

### Running the Application

1. **Start the API**
```bash
cd BankingApp.Api
dotnet run
```

The API will start on `http://localhost:5242`

2. **Access the API Documentation**
- Interactive API console: `http://localhost:5242/swagger-ui.html`
- Raw OpenAPI spec: `http://localhost:5242/openapi/v1.json`

3. **Database**
- SQLite database is automatically created at: `BankingApp.Api/Data/banking_app.db`
- Migrations are automatically applied on startup in Development environment

---

## API Endpoints

### Customers

#### Create Customer
```
POST /api/customers
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com"
}

Response: 201 Created
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "createdAt": "2026-03-17T20:10:30.055805Z",
  "accounts": []
}
```

#### Get Customer
```
GET /api/customers/{id}

Response: 200 OK
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "createdAt": "2026-03-17T20:10:30.055805Z",
  "accounts": [...]
}
```

### Accounts

#### Create Account
```
POST /api/accounts
Content-Type: application/json

{
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "accountNumber": "ACC001",
  "accountType": "Checking",
  "initialBalance": 5000
}

Response: 201 Created
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "accountNumber": "ACC001",
  "currency": "USD",
  "status": "Active",
  "createdAt": "2026-03-17T20:10:30.055805Z"
}
```

#### List Accounts (Phase 1)
```
GET /api/accounts?pageNumber=1&pageSize=10

Response: 200 OK
{
  "items": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440001",
      "customerId": "550e8400-e29b-41d4-a716-446655440000",
      "accountNumber": "ACC001",
      "currency": "USD",
      "status": "Active",
      "customerName": "John Doe",
      "createdAt": "2026-03-17T20:10:30.055805Z"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

#### Get Account Balance
```
GET /api/accounts/{id}/balance

Response: 200 OK
{
  "accountId": "660e8400-e29b-41d4-a716-446655440001",
  "balance": 5000.0
}
```

#### Update Account (Phase 2)
```
PUT /api/accounts/{id}
Content-Type: application/json

{
  "accountNumber": "ACC001-UPDATED"
}

Response: 200 OK
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "accountNumber": "ACC001-UPDATED",
  "status": "Active",
  "updatedAt": "2026-03-18T05:30:00Z"
}
```

#### Freeze Account (Phase 3)
```
POST /api/accounts/{id}/freeze

Response: 200 OK
{
  "message": "Account frozen successfully",
  "account": {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "status": "Frozen"
  }
}
```

#### Unfreeze Account (Phase 3)
```
POST /api/accounts/{id}/unfreeze

Response: 200 OK
{
  "message": "Account unfrozen successfully",
  "account": {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "status": "Active"
  }
}
```

#### Get Transaction History
```
GET /api/accounts/{id}/transactions?pageNumber=1&pageSize=10

Response: 200 OK
{
  "items": [
    {
      "id": "770e8400-e29b-41d4-a716-446655440002",
      "transactionId": "880e8400-e29b-41d4-a716-446655440003",
      "amount": 100.0,
      "entryType": "Credit",
      "createdAt": "2026-03-17T20:10:30.056795Z",
      "transactionReference": "TRF-2026-001"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

### Transfers

#### Transfer Money
```
POST /api/transfers
Content-Type: application/json

{
  "fromAccountId": "660e8400-e29b-41d4-a716-446655440001",
  "toAccountId": "660e8400-e29b-41d4-a716-446655440002",
  "amount": 500,
  "reference": "Payment for services"
}

Response: 201 Created
{
  "id": "990e8400-e29b-41d4-a716-446655440004",
  "reference": "Payment for services",
  "createdAt": "2026-03-17T20:10:30.057795Z"
}
```

#### List Transfers (Phase 1)
```
GET /api/transfers?pageNumber=1&pageSize=10

Response: 200 OK
{
  "items": [
    {
      "id": "990e8400-e29b-41d4-a716-446655440004",
      "sourceAccountId": "660e8400-e29b-41d4-a716-446655440001",
      "destinationAccountId": "660e8400-e29b-41d4-a716-446655440002",
      "amount": 500.0,
      "reference": "Payment for services",
      "createdAt": "2026-03-17T20:10:30.057795Z"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

## Customer Endpoints

### List Customers (Phase 1)
```
GET /api/customers?pageNumber=1&pageSize=10

Response: 200 OK
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com",
      "createdAt": "2026-03-17T20:10:30.055805Z"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

### Update Customer (Phase 2)
```
PUT /api/customers/{id}
Content-Type: application/json

{
  "firstName": "Jonathan",
  "lastName": "Smith",
  "email": "jonathan.smith@example.com"
}

Response: 200 OK
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "firstName": "Jonathan",
  "lastName": "Smith",
  "email": "jonathan.smith@example.com",
  "updatedAt": "2026-03-18T05:30:00Z"
}
```

---

## Testing

### Quick Testing Checklist

✅ **API is Running**
```bash
curl -I http://localhost:5242/api/customers
# Expected: HTTP/1.1 200 OK
```

✅ **Database Migrations Applied**
```bash
# Migrations automatically applied on startup
# - InitialCreate (base schema)
# - MakeLedgerEntryTransactionIdNullable (ledger flexibility)
# - AddAccountStatusField (account freeze/unfreeze)
```

✅ **All 16 Endpoints Verified**
- 3 Phase 1 endpoints: List Customers, List Accounts, List Transfers
- 2 Phase 2 endpoints: Update Customer, Update Account
- 2 Phase 3 endpoints: Freeze Account, Unfreeze Account
- 9 Original endpoints: Create/Get operations, transfers

### Manual Testing

#### Test Phase 1: List Endpoints with Pagination

```bash
# List customers with pagination
curl -s http://localhost:5242/api/customers?pageNumber=1&pageSize=10 | jq '.pagination'

# Expected output:
# {
#   "pageNumber": 1,
#   "pageSize": 10,
#   "totalCount": <number>,
#   "totalPages": <calculated>
# }
```

#### Test Phase 2: Update Endpoints

```bash
# Update customer (first get a valid customer ID)
CUSTOMER_ID=$(curl -s http://localhost:5242/api/customers?pageNumber=1 | jq -r '.items[0].id')

curl -X PUT http://localhost:5242/api/customers/$CUSTOMER_ID \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Updated","lastName":"Name","email":"unique@example.com"}'

# Expected: 200 OK with updated customer data
```

#### Test Phase 3: Account State Transitions

```bash
# Get account ID
ACCOUNT_ID=$(curl -s http://localhost:5242/api/accounts?pageNumber=1 | jq -r '.items[0].id')

# Freeze account
curl -X POST http://localhost:5242/api/accounts/$ACCOUNT_ID/freeze

# Expected: 200 OK
# {"message":"Account frozen successfully","account":{"status":"Frozen"}}

# Unfreeze account
curl -X POST http://localhost:5242/api/accounts/$ACCOUNT_ID/unfreeze

# Expected: 200 OK
# {"message":"Account unfrozen successfully","account":{"status":"Active"}}
```

### Error Testing

#### Test Duplicate Detection

```bash
# Try creating customer with duplicate email (will fail)
curl -X POST http://localhost:5242/api/customers \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Test","lastName":"User","email":"john@example.com"}'

# Expected: 409 Conflict
# {"message":"Customer with email already exists","errorCode":4011,"statusCode":409}
```

#### Test Invalid State Transitions

```bash
# Try freezing an already frozen account (will fail)
curl -X POST http://localhost:5242/api/accounts/$ACCOUNT_ID/freeze

# Expected: 422 Unprocessable Entity
# {"message":"Account is already frozen","errorCode":4004,"statusCode":422}
```

### Using VS Code REST Client

1. Install the **REST Client** extension by Huachao Mao
2. Open `BankingApp.Api.http`
3. Click "Send Request" on any endpoint
4. View response in the preview panel

### Using cURL Script

Create `test_api.sh`:
```bash
#!/bin/bash

API="http://localhost:5242/api"

echo "=== Testing List Endpoints ==="
echo "Customers:"
curl -s "$API/customers?pageNumber=1&pageSize=5" | jq '.pagination'

echo -e "\nAccounts:"
curl -s "$API/accounts?pageNumber=1&pageSize=5" | jq '.pagination'

echo -e "\nTransfers:"
curl -s "$API/transfers?pageNumber=1&pageSize=5" | jq '.pagination'

echo -e "\n=== Testing Create/Get Cycle ==="
CUSTOMER=$(curl -s -X POST "$API/customers" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Test","lastName":"User","email":"test-'$(date +%s)'@example.com"}')

CUSTOMER_ID=$(echo $CUSTOMER | jq -r '.id')
echo "Created customer: $CUSTOMER_ID"

# Get and display
curl -s "$API/customers/$CUSTOMER_ID" | jq '.id,.firstName,.lastName'
```

Run with:
```bash
chmod +x test_api.sh
./test_api.sh
```

---

## Implementation Summary

### Phase 1: List Endpoints (Pagination)
- **ListCustomersQueryHandler**: Fetch customers with pagination, sorting by lastName
- **ListAccountsQueryHandler**: Fetch accounts with optional customer filter
- **ListTransfersQueryHandler**: Fetch transfers using ledger-based queries
- **Feature**: pageNumber, pageSize (1-100), totalCount, totalPages

### Phase 2: Update Endpoints
- **UpdateCustomerCommandHandler**: Update customer details with duplicate email detection
- **UpdateAccountCommandHandler**: Update account details with duplicate account detection
- **Feature**: Idempotent operations, validation checks

### Phase 3: State Management
- **FreezeAccountCommandHandler**: Transition account to Frozen status
- **UnfreezeAccountCommandHandler**: Transition account back to Active status
- **Feature**: State validation prevents invalid transitions

### Database Changes
- **AddAccountStatusField Migration**: Added Status column to Accounts table
  - Default value: "Active"
  - Allowed values: Active, Frozen, Closed
  - Enables account lifecycle management

### Error Handling
All error responses include:
- `message`: Generic error message (no sensitive data)
- `errorCode`: Banking-specific code (4000-5003 range)
- `statusCode`: HTTP status
- `traceId`: Correlation ID for support

Common error codes used:
- 4000: Validation failed (400)
- 4004: Account frozen (422)
- 4005: Insufficient funds (422)
- 4010: Customer not found (404)
- 4011: Duplicate email (409)
- 4003: Duplicate account (409)

---

## Project Structure

```
banking-app/
├── BankingApp.Api/                          # ASP.NET Core API
│   ├── Controllers/                         # HTTP endpoints
│   ├── Program.cs                           # Startup configuration
│   ├── appsettings.json                     # Configuration
│   └── wwwroot/
│       └── swagger-ui.html                  # Interactive API console with request builder (Swagger UI)
│
├── BankingApp.Application/                  # Business logic
│   ├── CQRS/
│   │   ├── Commands/                        # Command definitions
│   │   ├── CommandHandlers/                 # Command execution
│   │   ├── Queries/                         # Query definitions
│   │   └── QueryHandlers/                   # Query execution
│   ├── Repositories/                        # Repository interfaces
│   ├── Exceptions/                          # Custom exceptions
│   ├── Validators/                          # FluentValidation validators
│   └── UnitOfWork/                          # Transaction management
│
├── BankingApp.Domain/                       # Pure domain logic
│   ├── Entities/                            # Domain entities
│   ├── ValueObjects/                        # Immutable value objects
│   ├── Specifications/                      # Business rules
│   └── Events/                              # Domain events
│
├── BankingApp.Infrastructure/               # Data access
│   ├── Data/
│   │   ├── BankingDbContext.cs              # EF Core DbContext
│   │   └── Migrations/                      # Database migrations
│   ├── Repositories/                        # Repository implementations
│   └── Services/                            # Infrastructure services
│
└── README.md                                # This file
```

---

## Key Technical Decisions

### CQRS Pattern
Commands and Queries are separated for clear intent and scalability. Command handlers manage state changes, query handlers optimize read access.

### Unit of Work with Transactions
Database transactions ensure ACID properties. Critical operations like transfers are atomic - all or nothing.

### Double-Entry Bookkeeping
Every transaction creates both debit and credit ledger entries for complete audit trail and financial accuracy.

### Race Condition Prevention
Concurrent transfer attempts are handled atomically. Database-level UNIQUE constraints prevent duplicate accounts.

### Exception Handling
Custom exceptions (`InsufficientFundsException`, `ResourceNotFoundException`) provide semantic error handling with generic messages to clients and detailed logging internally.

---

## Dependencies

- **Microsoft.AspNetCore.OpenApi** (10.0.5) - OpenAPI/Swagger support
- **Microsoft.EntityFrameworkCore** (10.0.5) - ORM
- **Microsoft.EntityFrameworkCore.Sqlite** (10.0.5) - SQLite provider
- **FluentValidation** (12.1.1) - Input validation
- **MediatR** (14.1.0) - CQRS mediator pattern

---

## Security Considerations

- ✅ **Input Validation** - All inputs validated with FluentValidation
- ✅ **SQL Injection Prevention** - Parameterized queries via EF Core
- ✅ **Sensitive Data** - Balance information logged internally only, generic messages to clients
- ✅ **Transaction Safety** - Database constraints prevent invalid states
- ✅ **Error Handling** - Generic error messages prevent information disclosure

---

## Performance Optimizations

- ✅ **Pagination** - Transaction history supports 1-100 items per page
- ✅ **Deterministic Sorting** - Secondary sort by ID ensures consistent ordering
- ✅ **Indexes** - Unique constraints on account numbers and emails
- ✅ **Lazy Loading** - Entities loaded only when needed

---

## Troubleshooting

### Port Already in Use
```bash
# Kill the process using port 5242
lsof -i :5242 | grep -v COMMAND | awk '{print $2}' | xargs kill -9
```

### Database Lock
SQLite uses file locking. If you get "database is locked":
```bash
# Delete the old database and let it recreate
rm BankingApp.Api/Data/banking_app.db
dotnet run
```

### Migration Issues
```bash
# Reapply migrations
dotnet ef database drop --project BankingApp.Infrastructure
dotnet ef database update --project BankingApp.Infrastructure
```

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## Author

**Ernie Bshongwe**
- GitHub: [@bshongwe](https://github.com/bshongwe)

---
## End Points

#### List Endpoints with Pagination
- Added `GET /api/customers` with pagination (pageNumber, pageSize 1-100, totalCount, totalPages)
- Added `GET /api/accounts` with optional customer filter and pagination
- Added `GET /api/transfers` with pagination and ledger-based queries
- All endpoints support consistent pagination metadata

#### Update Operations
- Added `PUT /api/customers/{id}` with duplicate email detection
- Added `PUT /api/accounts/{id}` with duplicate account detection
- Both endpoints support idempotent updates with validation

#### State Management
- Added `POST /api/accounts/{id}/freeze` to transition account to Frozen status
- Added `POST /api/accounts/{id}/unfreeze` to transition account to Active status
- State validation prevents invalid transitions (e.g., freezing already frozen account)
- Added Account.Status field to database schema (Active, Frozen, Closed)

#### Database Migration
- Migration `AddAccountStatusField` adds Status column to Accounts table
- Default value set to "Active"
- Supports future account lifecycle management

#### Testing & Verification
- ✅ All 7 new endpoints implemented and tested
- ✅ Pagination verified with multiple test queries
- ✅ Duplicate detection working (email, account number)
- ✅ State transitions validated
- ✅ Error handling confirmed
- ✅ 0 build warnings/errors
- ✅ All 16 endpoints verified live

