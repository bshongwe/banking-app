# Banking API - Quick Reference Guide

## Base URL
```
http://localhost:5242/api
```

---

## Customer Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/customers` | List all customers (paginated) | ❌ |
| `GET` | `/customers/{id}` | Get customer by ID | ❌ |
| `POST` | `/customers` | Create new customer | ❌ |
| `PUT` | `/customers/{id}` | Update customer | ❌ |

### Create Customer Example
```bash
curl -X POST http://localhost:5242/api/customers \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com"
  }'
```

### List Customers Example
```bash
curl "http://localhost:5242/api/customers?pageNumber=1&pageSize=10"
```

---

## Account Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/accounts` | List all accounts (paginated) | ❌ |
| `GET` | `/accounts/{id}` | Get account details | ❌ |
| `GET` | `/accounts/{id}/balance` | Get account balance | ❌ |
| `GET` | `/accounts/{id}/transactions` | Get transaction history (paginated) | ❌ |
| `POST` | `/accounts` | Create new account | ❌ |
| `PUT` | `/accounts/{id}` | Update account | ❌ |
| `POST` | `/accounts/{id}/freeze` | Freeze account | ❌ |
| `POST` | `/accounts/{id}/unfreeze` | Unfreeze account | ❌ |

### Create Account Example
```bash
curl -X POST http://localhost:5242/api/accounts \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "accountNumber": "ACC001",
    "initialBalance": 1000.00,
    "currency": "USD"
  }'
```

### Freeze Account Example
```bash
curl -X POST http://localhost:5242/api/accounts/550e8400-e29b-41d4-a716-446655440000/freeze
```

---

## Transfer Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/transfers` | List all transfers (paginated) | ❌ |
| `POST` | `/transfers` | Create transfer | ❌ |

### Create Transfer Example
```bash
curl -X POST http://localhost:5242/api/transfers \
  -H "Content-Type: application/json" \
  -d '{
    "sourceAccountId": "550e8400-e29b-41d4-a716-446655440000",
    "destinationAccountId": "660e8400-e29b-41d4-a716-446655440001",
    "amount": 100.00,
    "reference": "Payment for services"
  }'
```

---

## Query Parameters

### Pagination (applicable to list endpoints)
```
?pageNumber=1&pageSize=10
```
- **pageNumber**: Starting page (default: 1)
- **pageSize**: Items per page (default: 10, max: 100)

### Filtering
- **GET /accounts**: `?customerId={id}` - Filter by customer
- **GET /transfers**: `?accountId={id}` - Filter by account (source or destination)

---

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| `200` | OK - Request successful |
| `201` | Created - Resource created successfully |
| `400` | Bad Request - Invalid input |
| `404` | Not Found - Resource doesn't exist |
| `409` | Conflict - Duplicate email/account number |
| `422` | Unprocessable Entity - Invalid operation state |
| `500` | Internal Server Error |

---

## Response Format

### Success Response (200 OK)
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "createdAt": "2026-03-18T10:30:00Z"
}
```

### Paginated Response
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john.doe@example.com",
      "createdAt": "2026-03-18T10:30:00Z"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3
  }
}
```

### Error Response (400/404/500)
```json
{
  "message": "Customer not found",
  "errorCode": 4010,
  "statusCode": 404,
  "traceId": "0HMVJ7Q9K0PQH:00000001",
  "validationErrors": null
}
```

---

## Account Status Transitions

```
Active → Frozen
  ↓       ↓
Closed  Active
```

- **Active**: Normal transactions allowed
- **Frozen**: No transactions allowed
- **Closed**: Permanent, cannot be reopened

---

## Error Codes

| Code | Description |
|------|-------------|
| 4010 | Customer Not Found |
| 4001 | Account Not Found |
| 4002 | Insufficient Funds |
| 4003 | Duplicate Account Number |
| 4004 | Duplicate Email |
| 4005 | Currency Mismatch |
| 4006 | Invalid Transfer Amount |
| 4007 | Account Frozen |

---

## Testing with cURL/Postman

Sample requests are available in `BankingApp.Api.http`
