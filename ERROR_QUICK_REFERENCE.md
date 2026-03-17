# Error Handling Quick Reference

## Error Codes at a Glance

### By Category

#### Account Errors (4001-4004)
```
4001 - Account not found
4002 - Account invalid status
4003 - Account number already exists (409)
4004 - Account frozen (422)
```

#### Transaction Errors (4005-4008)
```
4005 - Insufficient funds (422)
4006 - Invalid transfer amount (400)
4007 - Transfer not allowed
4008 - Currency mismatch (422)
```

#### Customer Errors (4010-4011)
```
4010 - Customer not found (404)
4011 - Duplicate email (409)
```

#### Validation (4000)
```
4000 - Generic validation failed (400)
```

#### Server (5000, 5003)
```
5000 - Internal server error (500)
5003 - Service unavailable
```

---

## HTTP Status Codes

### Success (2xx)
| Code | When | Example |
|------|------|---------|
| 200 | Operation succeeded | Transfer completed |
| 201 | Resource created | New customer/account |
| 206 | Partial/paginated data | Transaction history |

### Client Errors (4xx)
| Code | Domain Code | When | Example |
|------|---|------|---------|
| 400 | 4000, 4006 | Bad request, invalid amount | Negative transfer |
| 404 | 4001, 4010 | Not found | Account doesn't exist |
| 409 | 4003, 4011 | Conflict/duplicate | Duplicate account # |
| 422 | 4004, 4005, 4008 | Can't process | Insufficient funds |

### Server Errors (5xx)
| Code | Domain Code | When |
|------|---|------|
| 500 | 5000 | Unexpected error |
| 503 | 5003 | Service down |

---

## Exception to Code Mapping

| Exception | HTTP | Domain Code | Use When |
|-----------|------|---|---|
| `DuplicateAccountNumberException` | 409 | 4003 | AccountNumber exists |
| `DuplicateEmailException` | 409 | 4011 | Email exists |
| `InvalidTransferAmountException` | 400 | 4006 | Amount ≤ 0 |
| `CurrencyMismatchException` | 422 | 4008 | USD → EUR |
| `AccountFrozenException` | 422 | 4004 | Account closed |
| `InsufficientFundsException` | 422 | 4005 | Balance < amount |
| `ResourceNotFoundException` | 404 | 4001/4010 | Resource missing |
| `ArgumentException` | 400 | 4000 | Invalid input |
| Other `Exception` | 500 | 5000 | Unexpected |

---

## How to Throw Exceptions

```csharp
// In command handlers
throw new InvalidTransferAmountException(amount);
throw new InsufficientFundsException(balance, amount);
throw new CurrencyMismatchException("USD", "EUR");
throw new DuplicateAccountNumberException(accountNumber);
throw new DuplicateEmailException(email);
throw new AccountFrozenException(accountId);
throw new ResourceNotFoundException("Account", accountId);
throw new ArgumentException("Invalid input");
```

---

## Response Examples

### 422 - Insufficient Funds
```json
{
  "message": "Insufficient funds.",
  "errorCode": 4005,
  "statusCode": 422,
  "traceId": "0HN6O5RA2VDCF:00000001"
}
```

### 409 - Duplicate Account
```json
{
  "message": "Account number 'ACC001' already exists.",
  "errorCode": 4003,
  "statusCode": 409,
  "traceId": "0HN6O5RA2VDCF:00000002"
}
```

### 404 - Not Found
```json
{
  "message": "Customer with ID ... not found.",
  "errorCode": 4010,
  "statusCode": 404,
  "traceId": "0HN6O5RA2VDCF:00000003"
}
```

### 400 - Bad Request
```json
{
  "message": "Transfer amount must be greater than zero.",
  "errorCode": 4006,
  "statusCode": 400,
  "traceId": "0HN6O5RA2VDCF:00000004"
}
```

### 422 - Currency Mismatch
```json
{
  "message": "Cannot transfer between accounts with different currencies.",
  "errorCode": 4008,
  "statusCode": 422,
  "traceId": "0HN6O5RA2VDCF:00000005"
}
```

---

## Client-Side Error Handling Pattern

```javascript
// JavaScript/TypeScript
async function handleTransfer(request) {
  try {
    const response = await fetch('/api/transfers', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request)
    });

    if (response.ok) {
      return await response.json();
    }

    const error = await response.json();
    
    // Handle by domain code for better UX
    switch (error.errorCode) {
      case 4005:
        throw new Error('Insufficient funds in account');
      case 4008:
        throw new Error('Cannot transfer between different currencies');
      case 4001:
        throw new Error('Account not found');
      case 4006:
        throw new Error('Invalid transfer amount');
      default:
        throw new Error(error.message);
    }
  } catch (error) {
    console.error(error);
    // Show user-friendly message
  }
}
```

---

## Endpoints & Error Codes

### POST /api/customers
**Success**: 201 Created
**Errors**: 400, 409, 500

### POST /api/accounts
**Success**: 201 Created
**Errors**: 400, 404, 409, 422, 500

### GET /api/accounts/{id}
**Success**: 200 OK
**Errors**: 404, 500

### GET /api/accounts/{id}/balance
**Success**: 200 OK
**Errors**: 404, 500

### GET /api/accounts/{id}/transactions
**Success**: 200 OK (206 paginated)
**Errors**: 400, 404, 500

### POST /api/transfers
**Success**: 200 OK
**Errors**: 400, 404, 409, 422, 500

### GET /api/customers/{id}
**Success**: 200 OK
**Errors**: 404, 500

---

## Testing Commands

```bash
# Insufficient funds
curl -X POST http://localhost:5242/api/transfers \
  -H "Content-Type: application/json" \
  -d '{"fromAccountId":"...","toAccountId":"...","amount":10000,"reference":"Test"}' | jq .

# Duplicate account
curl -X POST http://localhost:5242/api/accounts \
  -H "Content-Type: application/json" \
  -d '{"customerId":"...","accountNumber":"ACC001"}' | jq .

# Invalid amount
curl -X POST http://localhost:5242/api/transfers \
  -H "Content-Type: application/json" \
  -d '{"fromAccountId":"...","toAccountId":"...","amount":-100,"reference":"Test"}' | jq .

# Not found
curl -X GET http://localhost:5242/api/customers/00000000-0000-0000-0000-000000000000 | jq .
```

---

## Architecture

```
Exception Thrown
       ↓
ErrorHandlingMiddleware
       ↓
   Map to:
   - HTTP Status Code
   - Domain Error Code
   - Generic Message
   - TraceId
       ↓
ErrorResponse JSON
       ↓
Client
```

---

## Error Code Convention

```
4000-4009: General & transaction errors
4010-4019: Resource not found
4020-4028: Conflict/duplicate
4029-4999: Future expansion

5000-5099: Server errors
```

---

## Key Files

| File | Purpose |
|------|---------|
| `Exceptions/BankingErrorCode.cs` | Enum of all error codes |
| `DTOs/ErrorResponse.cs` | Standardized error response DTO |
| `Middleware/ErrorHandlingMiddleware.cs` | Global exception handler |
| `Controllers/*.cs` | Clean endpoints (no try-catch) |
| `ERROR_HANDLING.md` | Complete documentation |
| `ERROR_CODES_MATRIX.md` | Reference matrix |
| `BEFORE_AFTER_COMPARISON.md` | Detailed comparison |

---

## Checklist for Adding New Error

- [ ] Add code to `BankingErrorCode` enum
- [ ] Create exception class with `static ErrorCode` property
- [ ] Add case to `ErrorHandlingMiddleware` switch
- [ ] Throw in handler/command
- [ ] Update endpoint `[ProducesResponseType]`
- [ ] Add example to documentation
- [ ] Test manually

---

## Future Enhancements

⏳ Rate Limiting (429)
⏳ Authentication (401)
⏳ Authorization (403)
⏳ Soft Deletes (410)
⏳ Optimistic Locking (412)
⏳ Async Processing (202)

