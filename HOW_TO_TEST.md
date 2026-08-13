# 🚀 Complete Guide: How to Test All Endpoints

## Overview
This guide walks you through testing all 41 endpoints of the Nursery Management System API using multiple methods.

---

## 📋 Quick Test Methods Comparison

| Method | Time | Skill Level | Automation | Best For |
|--------|------|-------------|-----------|----------|
| Postman GUI | ~30 min | Beginner | Manual | Visual testing, quick checks |
| Test Script | ~15 min | Intermediate | Automated | Comprehensive testing, CI/CD |
| cURL/Manual | ~45 min | Advanced | Manual | Integration tests, debugging |

---

## Method 1: Using Postman (Recommended for Manual Testing)

### Step 1: Start the API Server
```powershell
cd C:\Users\kiroe\source\repos\NurseryManagementSystem.API
dotnet run --project NurseryManagementSystem.API/NurseryManagementSystem.API.csproj
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5293
```

### Step 2: Open Postman
1. Launch Postman application
2. Go to File → Import
3. Select `Postman/NurseryManagementSystem.postman_collection.json`

### Step 3: Create Environment
1. Click "Environments" (bottom left)
2. Click "+" to create new environment
3. Name it: `NurseryManagementSystem-Local`
4. Add these variables:
   ```
   VARIABLE        INITIAL VALUE              CURRENT VALUE
   baseUrl         http://localhost:5293      http://localhost:5293
   accessToken     (leave empty)              (leave empty)
   refreshToken    (leave empty)              (leave empty)
   userId          (leave empty)              (leave empty)
   ```
5. Click Save

### Step 4: Run the Login Request
1. Select the environment: `NurseryManagementSystem-Local`
2. Open collection → Authentication → Login
3. Update credentials if needed:
   ```json
   {
     "email": "admin@nursery.com",
     "password": "Admin@123"
   }
   ```
4. Click "Send" button
5. **Expected Response** (200 OK):
   ```json
   {
     "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
     "refreshToken": "550e8400-e29b-41d4-a716-446655440000",
     "userId": "660e8400-e29b-41d4-a716-446655440001",
     "expiresIn": 3600
   }
   ```

### Step 5: Verify Auto-Token Population
1. Check environment variables (should be auto-populated)
2. The test script in the Login request sets the variables
3. Proceed to next test

### Step 6: Test Other Endpoints
1. Open Authentication → Refresh Token
2. Click "Send"
3. Should receive 200 OK with new tokens

### Step 7: Test User Endpoints
1. Open Users → Get All Users
2. Click "Send"
3. Should receive 200 OK with user list
4. Continue with Create User, Update User, etc.

### Step 8: Follow Test Workflow
Order of testing (follow this workflow):
```
1. Authentication (Login → Refresh → Revoke)
   ↓
2. Users (Get All → Get One → Create → Update → Assign Role → Active → Password)
   ↓
3. Children (Get All → Get One → Create → Update → Active → Emergency Contacts)
   ↓
4. Attendance (Child Check-In → Check-Out → Get History → Staff Check-In/Out)
   ↓
5. Plans (Get All → Get One → Create → Update → Delete)
   ↓
6. Plan Assignments (Assign → End → Get Child Assignments)
   ↓
7. Billing (Generate → Get List → Get One → Pay → Cancel)
   ↓
8. Schedule (Get All → Create → Update → Delete)
   ↓
9. Session Logs (Get Logs)
```

### Tips for Postman Testing
- **Save responses**: Click "Save Response" to store actual responses
- **Run full collection**: Right-click collection → Run collection
- **Check pre-request scripts**: Automatically generate test data
- **Review test scripts**: Check response validation logic
- **Use variables**: {{baseUrl}}, {{accessToken}} are auto-populated
- **Test errors**: Try invalid credentials, wrong IDs, etc.

---

## Method 2: Using PowerShell Test Script (Automated Testing)

### Step 1: Start the API Server
```powershell
cd C:\Users\kiroe\source\repos\NurseryManagementSystem.API
dotnet run --project NurseryManagementSystem.API/NurseryManagementSystem.API.csproj
```

### Step 2: Open PowerShell Terminal
```powershell
cd C:\Users\kiroe\source\repos\NurseryManagementSystem.API
```

### Step 3: Run the Test Script
```powershell
.\test-endpoints.ps1 -BaseUrl "http://localhost:5293/api"
```

### Step 4: Monitor Test Results
The script will display:
```
================================================================================
1️⃣  AUTHENTICATION ENDPOINTS
================================================================================
✅ PASS - Login with valid credentials
  └─ Status: 200
✅ PASS - Refresh access token
  └─ Status: 200
✅ PASS - Revoke token (Logout)
  └─ Status: 204

================================================================================
2️⃣  USERS MANAGEMENT ENDPOINTS (Requires SuperAdmin)
================================================================================
✅ PASS - Get all users with pagination
  └─ Status: 200
[... more tests ...]

================================================================================
📊 TEST SUMMARY REPORT
================================================================================

📈 Overall Results:
   Total Tests: 50
   Passed: 48
   Failed: 2
   Success Rate: 96.00%
```

### Step 5: Review Test Results
- Check overall success rate
- Note any failed tests
- Review error messages for failures
- Fix issues and re-run if needed

### Step 6: Export Test Report (Optional)
```powershell
.\test-endpoints.ps1 -BaseUrl "http://localhost:5293/api" | Tee-Object -FilePath "test-results.txt"
```

---

## Method 3: Using cURL Commands (Advanced Testing)

### Step 1: Get Access Token
```bash
curl -X POST http://localhost:5293/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@nursery.com",
    "password": "Admin@123"
  }'
```

**Save the response:**
```json
{
  "accessToken": "YOUR_TOKEN_HERE",
  "refreshToken": "YOUR_REFRESH_TOKEN",
  "userId": "YOUR_USER_ID",
  "expiresIn": 3600
}
```

### Step 2: Store Token in Variable
```bash
export TOKEN="YOUR_TOKEN_HERE"
export BASE_URL="http://localhost:5293/api"
```

### Step 3: Test GET Endpoint
```bash
curl -X GET "$BASE_URL/children?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

### Step 4: Test POST Endpoint
```bash
curl -X POST "$BASE_URL/children" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "TestChild",
    "lastName": "Johnson",
    "dateOfBirth": "2021-06-15",
    "parentFirstName": "John",
    "parentLastName": "Johnson",
    "parentEmail": "john@email.com",
    "parentPhoneNumber": "+1234567890",
    "allergies": "Peanuts",
    "specialNeeds": "None",
    "healthInsuranceNumber": "INS123456"
  }'
```

### Step 5: Test PUT Endpoint
```bash
curl -X PUT "$BASE_URL/children/CHILD_ID_HERE" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "UpdatedChild",
    "lastName": "Johnson",
    "dateOfBirth": "2021-06-15",
    "parentFirstName": "John",
    "parentLastName": "Johnson",
    "parentEmail": "john@email.com",
    "parentPhoneNumber": "+1234567890",
    "allergies": "Peanuts, Dairy",
    "specialNeeds": "None",
    "healthInsuranceNumber": "INS123456"
  }'
```

### Step 6: Test DELETE Endpoint
```bash
curl -X DELETE "$BASE_URL/children/CHILD_ID_HERE" \
  -H "Authorization: Bearer $TOKEN"
```

---

## Method 4: Using Visual Studio Built-in Tools

### Using Visual Studio REST Client
1. Create file: `.http` or `.rest`
2. Add requests like this:
   ```http
   ### Login
   POST http://localhost:5293/api/auth/login
   Content-Type: application/json

   {
     "email": "admin@nursery.com",
     "password": "Admin@123"
   }

   ### Get Children
   GET http://localhost:5293/api/children?pageNumber=1&pageSize=10
   Authorization: Bearer {{accessToken}}
   ```
3. Click "Send Request" above each request

---

## 📊 Testing Checklist

### Pre-Test Setup
- [ ] API server is running on http://localhost:5293
- [ ] Database is accessible
- [ ] Test credentials are available (admin@nursery.com / Admin@123)
- [ ] Choose testing method (Postman, Script, or cURL)
- [ ] Postman collection imported (if using Postman)

### Authentication Tests
- [ ] Login returns access token
- [ ] Login returns refresh token
- [ ] Tokens auto-populate in environment
- [ ] Invalid credentials return 401
- [ ] Refresh token generates new access token
- [ ] Revoke token logs out user

### User Tests
- [ ] Can get all users (paginated)
- [ ] Can get user by ID
- [ ] Can create new user
- [ ] Can update user information
- [ ] Can assign role to user
- [ ] Can activate/deactivate user
- [ ] Can change user password

### Children Tests
- [ ] Can get all children (paginated, filtered)
- [ ] Can get child by ID
- [ ] Can create new child
- [ ] Can update child information
- [ ] Can activate/deactivate child
- [ ] Can add emergency contact
- [ ] Can remove emergency contact

### Attendance Tests
- [ ] Child check-in records time
- [ ] Child check-out calculates duration
- [ ] Can retrieve child attendance history
- [ ] Staff check-in records time
- [ ] Staff check-out calculates duration
- [ ] Can retrieve staff attendance records

### Plan Tests
- [ ] Can get all plans
- [ ] Can get plan by ID
- [ ] Can create new plan (SuperAdmin)
- [ ] Can update plan (SuperAdmin)
- [ ] Can delete plan (SuperAdmin)

### Plan Assignment Tests
- [ ] Can assign plan to child
- [ ] Can end plan assignment
- [ ] Can get child assignments

### Billing Tests
- [ ] Can generate monthly invoices
- [ ] Can get invoices list
- [ ] Can get invoice by ID
- [ ] Can mark invoice as paid
- [ ] Can cancel invoice

### Schedule Tests
- [ ] Can get schedule slots
- [ ] Can create schedule slot
- [ ] Can update schedule slot
- [ ] Can delete schedule slot

### Session Log Tests
- [ ] Can get session logs (SuperAdmin)

---

## ✅ Expected Results Summary

### Authentication
- Login: **200 OK**
- Refresh: **200 OK**
- Revoke: **204 No Content**

### Users
- GET all: **200 OK**
- GET by ID: **200 OK**
- POST create: **201 Created**
- PUT update: **204 No Content**
- PUT role: **204 No Content**
- PUT active: **204 No Content**
- PUT password: **204 No Content**

### Children
- GET all: **200 OK**
- GET by ID: **200 OK**
- POST create: **201 Created**
- PUT update: **204 No Content**
- PUT active: **204 No Content**
- POST emergency-contact: **200/201 OK/Created**
- DELETE emergency-contact: **204 No Content**

### Attendance
- POST check-in: **200 OK**
- POST check-out: **200 OK**
- GET history: **200 OK**
- POST staff check-in: **200 OK**
- POST staff check-out: **200 OK**
- GET staff records: **200 OK**

### Plans
- GET all: **200 OK**
- GET by ID: **200 OK**
- POST create: **201 Created** (SuperAdmin)
- PUT update: **204 No Content** (SuperAdmin)
- DELETE: **204 No Content** (SuperAdmin)

### Plan Assignments
- POST assign: **200/201 OK/Created**
- PUT end: **204 No Content**
- GET child assignments: **200 OK**

### Billing
- POST generate: **200 OK**
- GET list: **200 OK**
- GET by ID: **200 OK**
- PUT pay: **204 No Content**
- PUT cancel: **204 No Content**

### Schedule
- GET all: **200 OK**
- POST create: **200/201 OK/Created**
- PUT update: **204 No Content**
- DELETE: **204 No Content**

### Session Logs
- GET logs: **200 OK** (SuperAdmin)

---

## 🔧 Troubleshooting

### Issue: "Connection refused"
**Solution**: Make sure API is running
```powershell
dotnet run --project NurseryManagementSystem.API/NurseryManagementSystem.API.csproj
```

### Issue: "401 Unauthorized"
**Solution**: 
- Check if token is expired (60 min expiry)
- Run refresh endpoint to get new token
- Try logging in again

### Issue: "403 Forbidden"
**Solution**:
- Endpoint requires SuperAdmin role
- Log in with admin account
- Check user role in database

### Issue: "404 Not Found"
**Solution**:
- Resource ID may be invalid
- Create the resource first
- Check URL path for typos

### Issue: "409 Conflict"
**Solution**:
- Resource already exists (e.g., email, plan name)
- Use different email/name
- Check for duplicate entries

### Issue: "Validation Error (400)"
**Solution**:
- Check request body format
- Verify all required fields present
- Check date/time format (yyyy-MM-dd, ISO 8601)
- Validate email format
- Check password requirements

---

## 📈 Performance Testing

### Load Testing (Optional)
```powershell
# Test response time
Measure-Command {
  curl -X GET "http://localhost:5293/api/children?pageNumber=1&pageSize=20" `
    -H "Authorization: Bearer $TOKEN"
}
```

### Expected Response Times
- List endpoints: < 500ms
- Get by ID: < 200ms
- Create: < 300ms
- Update: < 300ms
- Delete: < 200ms

---

## 📚 Documentation References

- **Full API Documentation**: [API_DOCUMENTATION.md](./Postman/API_DOCUMENTATION.md)
- **Frontend Quick Reference**: [FRONTEND_QUICK_REFERENCE.md](./Postman/FRONTEND_QUICK_REFERENCE.md)
- **Testing Report**: [TESTING_REPORT.md](./TESTING_REPORT.md)
- **Endpoint Checklist**: [ENDPOINT_VALIDATION_CHECKLIST.md](./ENDPOINT_VALIDATION_CHECKLIST.md)
- **Test Summary**: [TEST_SUMMARY.md](./TEST_SUMMARY.md)

---

## 🎯 Next Steps After Testing

1. **If all tests pass ✅**
   - All endpoints are working correctly
   - API is ready for frontend integration
   - Proceed to deployment

2. **If some tests fail ❌**
   - Review error messages
   - Check API logs
   - Debug specific endpoint
   - Fix issues
   - Re-run tests

3. **For Production Deployment**
   - Update JWT secret key to secure value
   - Configure HTTPS
   - Update database connection string
   - Set up logging
   - Configure backup strategy
   - Enable rate limiting
   - Review security checklist

---

## 🏁 Conclusion

You now have multiple ways to test all 41 endpoints:

1. **Postman** (Visual, interactive)
2. **Test Script** (Automated, comprehensive)
3. **cURL** (Command-line, scriptable)
4. **VS REST Client** (In IDE)

Choose the method that best fits your workflow and start testing!

**Good luck! 🚀**

---

**Generated**: August 2026
**API Version**: 1.0
**.NET Version**: .NET 10
**Total Endpoints**: 41
**Test Coverage**: 100%
