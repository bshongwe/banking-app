# Banking API - Comprehensive Testing Guide

**Last Updated**: 18 March 2026  
**API Version**: 1.0.0  
**Status**: ✅ Production Ready

---

## Table of Contents

1. [Quick Start Testing](#quick-start-testing)
2. [Manual Testing Procedures](#manual-testing-procedures)
3. [Automated Testing](#automated-testing)
4. [Troubleshooting](#troubleshooting)
5. [Performance Testing](#performance-testing)
6. [Security Testing](#security-testing)
7. [Error Scenario Testing](#error-scenario-testing)

---

## Quick Start Testing

### Prerequisites

1. API running on `http://localhost:5242`
2. `jq` installed for JSON parsing (optional but recommended)
3. `curl` available in terminal

### Verify API is Running

```bash
curl -I http://localhost:5242/api/customers
```

Expected response:
```
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```

---

## Manual Testing Procedures

### Phase 1: List Endpoints

#### Test 1.1: List Customers with Pagination

**Objective**: Verify pagination works correctly

**Request**:
```bash
curl -s http://localhost:5242/api/customers?pageNumber=1&pageSize=10 | jq .
```

**Expected Response**:
```json
{
  "items": [
    {
      "id": "7d51cd44-3dcd-43ef-bd3c-ebf6c8e5dc30",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john.doe@example.com",
      "createdAt": "2026-03-18T05:18:00.434244"
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

**Validation Checklist**:
- [ ] Status Code: 200
- [ ] Response is valid JSON
- [ ] `items` array present
- [ ] `pagination` object present with pageNumber, pageSize, totalCount, totalPages
- [ ] totalPages calculated correctly: `ceil(totalCount / pageSize)`

---

#### Test 1.2: List Customers - Test Pagination (Multiple Pages)

**Objective**: Verify pagination offset works correctly

**Setup**: Ensure multiple customers exist (create additional customers first)

**Request**:
```bash
# Get page 1
curl -s "http://localhost:5242/api/customers?pageNumber=1&pageSize=5" | jq '.items | length'

# Get page 2  
curl -s "http://localhost:5242/api/customers?pageNumber=2&pageSize=5" | jq '.items | length'
```

**Expected Behavior**:
- Page 1: Returns up to 5 items
- Page 2: Returns remaining items (if total > 5)
- No overlap between pages
- Correct offset applied

**Validation Checklist**:
- [ ] Page 1 returns different items than Page 2
- [ ] First item of Page 2 is different from first item of Page 1
- [ ] totalCount is consistent across pages
- [ ] totalPages is calculated consistently

---

#### Test 1.3: Get Single Customer

**Objective**: Verify individual customer retrieval

**Request**:
```bash
CUSTOMER_ID="7d51cd44-3dcd-43ef-bd3c-ebf6c8e5dc30"
curl -s http://localhost:5242/api/customers/$CUSTOMER_ID | jq .
```

**Expected Response**:
```json
{
  "id": "7d51cd44-3dcd-43ef-bd3c-ebf6c8e5dc30",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "createdAt": "2026-03-18T05:18:00.434244Z",
  "accounts": []
}
```

**Validation Checklist**:
- [ ] Status Code: 200
- [ ] All customer fields returned
- [ ] Related accounts array included
- [ ] No pagination metadata (single record endpoint)

---

#### Test 1.4: List Accounts with Optional Filter

**Objective**: Verify account listing and customer filtering

**Request - All Accounts**:
```bash
curl -s "http://localhost:5242/api/accounts?pageNumber=1&pageSize=10" | jq '.pagination'
```

**Request - Filter by Customer**:
```bash
CUSTOMER_ID="7d51cd44-3dcd-43ef-bd3c-ebf6c8e5dc30"
curl -s "http://localhost:5242/api/accounts?customerId=$CUSTOMER_ID&pageNumber=1&pageSize=10" | jq '.items | length'
```

**Validation Checklist**:
- [ ] Status Code: 200
- [ ] Pagination metadata included
- [ ] Customer name included in response
- [ ] Without filter: returns all accounts
- [ ] With filter: returns only accounts for that customer

---

#### Test 1.5: List Transfers

**Objective**: Verify transfer listing with pagination

**Request**:
```bash
curl -s "http://localhost:5242/api/transfers?pageNumber=1&pageSize=10" | jq .
```

**Validation Checklist**:
- [ ] Status Code: 200
- [ ] Items array present
- [ ] Pagination metadata included
- [ ] Each transfer has: id, amount, sourceAccountId, destinationAccountId, createdAt

---

### Phase 2: Update Endpoints

#### Test 2.1: Update Customer

**Objective**: Verify customer update functionality

**Request**:
```bash
CUSTOMER_ID="7d51cd44-3dcd-43ef-bd3c-ebf6c8e5dc30"
curl -X PUT http://localhost:5242/api/customers/$CUSTOMER_ID \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jonathan",
    "lastName": "Smith",
    "email": "jonathan.smith@example.com"
  }' | jq .
```

**Validation Checklist**:
- [ ] Status Code: 200
- [ ] Updated firstName returned
- [ ] Updated email returned
- [ ] Verify update persists with GET request

**Test Duplicate Email Detection**:
```bash
# Try updating to existing email
curl -X PUT http://localhost:5242/api/customers/$CUSTOMER_ID \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jonathan",
    "lastName": "Smith",
    "email": "existing@example.com"
  }'
```

**Expected**: Status Code 409 Conflict

---

#### Test 2.2: Update Account

**Objective**: Verify account update functionality

**Request**:
```bash
ACCOUNT_ID="550e8400-e29b-41d4-a716-446655440000"
curl -X PUT http://localhost:5242/api/accounts/$ACCOUNT_ID \
  -H "Content-Type: application/json" \
  -d '{
    "accountNumber": "ACC-001-UPDATED"
  }' | jq .
```

**Validation Checklist**:
- [ ] Status Code: 200
- [ ] Updated accountNumber returned
- [ ] Change persists in database

---

### Phase 3: Action Endpoints

#### Test 3.1: Freeze Account

**Objective**: Verify account freeze functionality

**Prerequisites**: Account must exist and be in Active status

**Request**:
```bash
ACCOUNT_ID="550e8400-e29b-41d4-a716-446655440000"
curl -X POST http://localhost:5242/api/accounts/$ACCOUNT_ID/freeze | jq .
```

**Expected Response**:
```json
{
  "message": "Account frozen successfully",
  "account": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "status": "Frozen",
    ...
  }
}
```

**Validation Checklist**:
- [ ] Status Code: 200
- [ ] Message indicates success
- [ ] Account status changed to "Frozen"
- [ ] Attempting to freeze again returns 422 error

**Test Invalid State Transition**:
```bash
# Try freezing already frozen account
curl -X POST http://localhost:5242/api/accounts/$ACCOUNT_ID/freeze
```

**Expected**: Status Code 422 Unprocessable Entity

---

#### Test 3.2: Unfreeze Account

**Objective**: Verify account unfreeze functionality

**Prerequisites**: Account must be in Frozen status

**Request**:
```bash
ACCOUNT_ID="550e8400-e29b-41d4-a716-446655440000"
curl -X POST http://localhost:5242/api/accounts/$ACCOUNT_ID/unfreeze | jq .
```

**Expected Response**:
```json
{
  "message": "Account unfrozen successfully",
  "account": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "status": "Active",
    ...
  }
}
```

**Validation Checklist**:
- [ ] Status Code: 200
- [ ] Message indicates success
- [ ] Account status changed to "Active"
- [ ] Attempting to unfreeze again returns 422 error

---

## Automated Testing

### Using VS Code REST Client

1. Install extension: **REST Client** by Huachao Mao
2. Open `BankingApp.Api.http`
3. Click "Send Request" on each test
4. Verify responses

### Using Postman

1. Import the collection from `BankingApp.Api.http`
2. Create environment with variable: `{{base_url}}` = `http://localhost:5242`
3. Run collection in sequence

### Bash Script Testing

Create `test_all.sh`:

```bash
#!/bin/bash

API="http://localhost:5242/api"
PASS=0
FAIL=0

test_endpoint() {
  local name=$1
  local method=$2
  local endpoint=$3
  local expected=$4
  
  echo -n "Testing $name... "
  
  if [ "$method" = "GET" ]; then
    status=$(curl -s -o /dev/null -w "%{http_code}" "$API$endpoint")
  else
    status=$(curl -s -X $method -o /dev/null -w "%{http_code}" "$API$endpoint")
  fi
  
  if [ "$status" = "$expected" ]; then
    echo "✅ PASS (HTTP $status)"
    ((PASS++))
  else
    echo "❌ FAIL (Expected $expected, got $status)"
    ((FAIL++))
  fi
}

# Run tests
test_endpoint "List Customers" "GET" "/customers" "200"
test_endpoint "List Accounts" "GET" "/accounts" "200"
test_endpoint "List Transfers" "GET" "/transfers" "200"
test_endpoint "Invalid Customer" "GET" "/customers/invalid-id" "400"

echo
echo "Results: $PASS passed, $FAIL failed"
```

Run with:
```bash
chmod +x test_all.sh
./test_all.sh
```

---

## Troubleshooting

### Common Issues and Solutions

#### Issue: "Connection refused" or "Empty reply from server"

**Diagnosis**:
```bash
curl -v http://localhost:5242/api/customers
```

**Solutions**:
1. Check API is running:
   ```bash
   lsof -i :5242
   ```

2. If not running, start it (from your project directory):
   ```bash
   cd /path/to/banking-app
   dotnet run --project BankingApp.Api
   ```

3. Wait for startup (takes 3-5 seconds)

---

#### Issue: 404 "Not Found" errors

**Diagnosis**:
```bash
curl -I http://localhost:5242/
```

**Solutions**:
1. Verify correct URL path:
   - ✅ Correct: `/api/customers`
   - ❌ Wrong: `/customers` or `/api/v1/customers`

2. Check endpoint exists in controller:
   ```bash
   grep -r "HttpGet" BankingApp.Api/Controllers/
   ```

3. Rebuild if needed:
   ```bash
   dotnet clean && dotnet build
   ```

---

#### Issue: 409 "Conflict" on duplicate resources

**Diagnosis**: Attempting to create/update with duplicate unique field

**Solutions**:
1. Check existing values:
   ```bash
   curl -s http://localhost:5242/api/customers | jq '.items[].email'
   ```

2. Use unique value:
   ```bash
   curl -X POST http://localhost:5242/api/customers \
     -H "Content-Type: application/json" \
     -d "{
       \"firstName\": \"New\",
       \"lastName\": \"User\",
       \"email\": \"unique-$(date +%s)@example.com\"
     }"
   ```

---

#### Issue: 422 "Unprocessable Entity" on freeze/unfreeze

**Diagnosis**: Attempting invalid state transition

**Current Status**:
```bash
curl -s http://localhost:5242/api/accounts/{id} | jq '.status'
```

**Valid Transitions**:
- Active → Frozen (use `/freeze`)
- Frozen → Active (use `/unfreeze`)

**Invalid**:
- Active → Active
- Frozen → Frozen
- Any → Closed

---

## Performance Testing

### Response Time Testing

```bash
#!/bin/bash

for i in {1..10}; do
  echo "Request $i:"
  time curl -s http://localhost:5242/api/customers | jq '. | keys'
done
```

**Expected**: Each request < 100ms

### Load Testing with Apache Bench

```bash
# 100 requests, 10 concurrent
ab -n 100 -c 10 http://localhost:5242/api/customers

# Expected: 500+ req/sec on modern hardware
```

---

## Security Testing

### 1. Test No PII Exposure

```bash
# Get error response
curl -s http://localhost:5242/api/customers/invalid-id

# Should NOT contain: email, phone, full customer data in error message
```

### 2. Test Input Validation

```bash
# Invalid email
curl -X POST http://localhost:5242/api/customers \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Test","lastName":"User","email":"notanemail"}' | jq '.errorCode'

# Expected: 4000 (Bad Request)
```

### 3. Test SQL Injection Prevention

```bash
# Try SQL injection in query
curl -s "http://localhost:5242/api/customers?pageNumber=1'; DROP TABLE Customers;--" | jq .

# Should return normal pagination (parameterized queries protect)
```

---

## Error Scenario Testing

### Scenario 1: Non-existent Customer

```bash
curl -s http://localhost:5242/api/customers/00000000-0000-0000-0000-000000000000 | jq .
```

**Expected**:
```json
{
  "message": "Customer not found",
  "errorCode": 4010,
  "statusCode": 404,
  "traceId": "0HMVJ7Q9K0PQH:00000001"
}
```

### Scenario 2: Duplicate Email

```bash
curl -X POST http://localhost:5242/api/customers \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Test","lastName":"User","email":"john.doe@example.com"}'
```

**Expected**:
```json
{
  "message": "Customer with email already exists",
  "errorCode": 4004,
  "statusCode": 409
}
```

### Scenario 3: Invalid Pagination

```bash
curl -s "http://localhost:5242/api/customers?pageSize=101"
```

**Expected**: 400 Bad Request

---

## Test Completion Checklist

- [ ] All Phase 1 (List) endpoints tested
- [ ] All Phase 2 (Update) endpoints tested
- [ ] All Phase 3 (Action) endpoints tested
- [ ] Pagination working (single and multiple pages)
- [ ] Filtering working (account by customer, transfer by account)
- [ ] Error scenarios tested
- [ ] Duplicate detection working
- [ ] State transitions working
- [ ] Performance acceptable (< 100ms per request)
- [ ] No PII exposed in responses
- [ ] Response times consistent

---

## Next: Integration Testing

Once all manual tests pass:

1. Set up automated CI/CD tests
2. Configure error monitoring
3. Set up performance baselines
4. Plan load testing schedule

