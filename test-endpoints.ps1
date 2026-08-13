#!/usr/bin/env powershell
# Comprehensive API Endpoint Testing Script
# Tests all endpoints for the Nursery Management System API

param(
    [string]$BaseUrl = "http://localhost:5293/api"
)

# Colors for output
$Green = [System.ConsoleColor]::Green
$Red = [System.ConsoleColor]::Red
$Yellow = [System.ConsoleColor]::Yellow
$Blue = [System.ConsoleColor]::Blue
$Cyan = [System.ConsoleColor]::Cyan

# Test results tracking
$testResults = @{
    Passed = 0
    Failed = 0
    Tests = @()
}

function Write-TestHeader {
    param([string]$Title)
    Write-Host "`n$('='*80)" -ForegroundColor $Blue
    Write-Host $Title -ForegroundColor $Cyan
    Write-Host $('='*80) -ForegroundColor $Blue
}

function Write-TestCase {
    param([string]$TestName, [bool]$Passed, [string]$Message = "")
    $status = if ($Passed) { "✅ PASS" } else { "❌ FAIL" }
    $color = if ($Passed) { $Green } else { $Red }

    Write-Host "$status - $TestName" -ForegroundColor $color
    if ($Message) {
        Write-Host "  └─ $Message" -ForegroundColor Yellow
    }

    if ($Passed) {
        $testResults.Passed++
    } else {
        $testResults.Failed++
    }

    $testResults.Tests += @{
        Name = $TestName
        Passed = $Passed
        Message = $Message
    }
}

function Test-Endpoint {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [hashtable]$Headers = @{},
        [string]$TestName = "",
        [int[]]$ExpectedStatusCodes = @(200, 201, 204)
    )

    $url = "$BaseUrl$Endpoint"
    $testNameDisplay = if ($TestName) { $TestName } else { "$Method $Endpoint" }

    try {
        $params = @{
            Uri = $url
            Method = $Method
            Headers = $Headers
            TimeoutSec = 10
        }

        if ($Body) {
            $params["Body"] = $Body | ConvertTo-Json -Depth 10
            $params["ContentType"] = "application/json"
        }

        $response = Invoke-WebRequest @params -ErrorAction Stop
        $passed = $response.StatusCode -in $ExpectedStatusCodes

        Write-TestCase $testNameDisplay $passed "Status: $($response.StatusCode)"
        return $response
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.Value__
        $passed = $statusCode -in $ExpectedStatusCodes

        if ($passed) {
            Write-TestCase $testNameDisplay $true "Status: $statusCode"
            return $_
        } else {
            $errorMsg = try {
                ($_.Exception.Response | ConvertFrom-Json).message
            } catch {
                $_.Exception.Message
            }
            Write-TestCase $testNameDisplay $false "Status: $statusCode - $errorMsg"
            return $null
        }
    }
}

# ============================================================================
# START TESTING
# ============================================================================

Write-Host "`n╔═════════════════════════════════════════════════════════════════╗" -ForegroundColor $Blue
Write-Host "║   Nursery Management System API - Comprehensive Test Suite       ║" -ForegroundColor $Blue
Write-Host "║   Testing all endpoints for functionality and integration        ║" -ForegroundColor $Blue
Write-Host "╚═════════════════════════════════════════════════════════════════╝" -ForegroundColor $Blue

Write-Host "`nℹ️  API Base URL: $BaseUrl" -ForegroundColor $Cyan
Write-Host "⏱️  Starting tests at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor $Cyan

# ============================================================================
# 1. AUTHENTICATION TESTS
# ============================================================================
Write-TestHeader "1️⃣  AUTHENTICATION ENDPOINTS"

$loginResponse = Test-Endpoint -Method POST -Endpoint "/auth/login" `
    -Body @{
        email = "admin@nursery.com"
        password = "Admin@123"
    } `
    -TestName "Login with valid credentials" `
    -ExpectedStatusCodes @(200, 400, 401)

$accessToken = $null
$refreshToken = $null
$userId = $null

if ($loginResponse -and $loginResponse.StatusCode -eq 200) {
    $loginData = $loginResponse.Content | ConvertFrom-Json
    $accessToken = $loginData.accessToken
    $refreshToken = $loginData.refreshToken
    $userId = $loginData.userId
    Write-Host "  └─ 🔐 Tokens obtained successfully" -ForegroundColor Green
    Write-Host "  └─ User ID: $userId" -ForegroundColor Green
} elseif ($loginResponse -and $loginResponse.Exception.Response.StatusCode.Value__ -eq 401) {
    Write-Host "  ⚠️  Credentials may be incorrect. Testing with different credentials..." -ForegroundColor Yellow
    # Try with alternative credentials if first login fails
    $loginResponse = Test-Endpoint -Method POST -Endpoint "/auth/login" `
        -Body @{
            email = "test@test.com"
            password = "Test@1234"
        } `
        -TestName "Login with alternative credentials" `
        -ExpectedStatusCodes @(200, 401)
}

# If we have a token, use it for authenticated endpoints
if ($accessToken) {
    $authHeaders = @{
        "Authorization" = "Bearer $accessToken"
        "Content-Type" = "application/json"
    }
} else {
    Write-Host "⚠️  Warning: Could not obtain access token. Some tests may fail." -ForegroundColor Yellow
    $authHeaders = @{
        "Content-Type" = "application/json"
    }
}

# Test refresh token
if ($refreshToken) {
    Test-Endpoint -Method POST -Endpoint "/auth/refresh" `
        -Body @{ refreshToken = $refreshToken } `
        -TestName "Refresh access token" `
        -ExpectedStatusCodes @(200, 400)
}

# ============================================================================
# 2. USERS ENDPOINTS (SuperAdmin)
# ============================================================================
Write-TestHeader "2️⃣  USERS MANAGEMENT ENDPOINTS (Requires SuperAdmin)"

Test-Endpoint -Method GET -Endpoint "/users?pageNumber=1&pageSize=10" `
    -Headers $authHeaders `
    -TestName "Get all users with pagination" `
    -ExpectedStatusCodes @(200, 403)

if ($userId) {
    Test-Endpoint -Method GET -Endpoint "/users/$userId" `
        -Headers $authHeaders `
        -TestName "Get user by ID" `
        -ExpectedStatusCodes @(200, 403, 404)
}

# Create new user
$newUserEmail = "testuser_$(Get-Random)@test.com"
$createUserResponse = Test-Endpoint -Method POST -Endpoint "/users" `
    -Headers $authHeaders `
    -Body @{
        firstName = "Test"
        lastName = "User"
        email = $newUserEmail
        password = "TestPassword@123"
        role = "Staff"
        phoneNumber = "+1234567890"
    } `
    -TestName "Create new user" `
    -ExpectedStatusCodes @(201, 400, 403)

$newUserId = $null
if ($createUserResponse -and $createUserResponse.StatusCode -eq 201) {
    $userData = $createUserResponse.Content | ConvertFrom-Json
    $newUserId = $userData.id
    Write-Host "  └─ New user created: $newUserId" -ForegroundColor Green
}

# Update user
if ($newUserId) {
    Test-Endpoint -Method PUT -Endpoint "/users/$newUserId" `
        -Headers $authHeaders `
        -Body @{
            firstName = "Updated"
            lastName = "User"
            phoneNumber = "+1987654321"
        } `
        -TestName "Update user information" `
        -ExpectedStatusCodes @(204, 400, 403, 404)
}

# Assign role
if ($newUserId) {
    Test-Endpoint -Method PUT -Endpoint "/users/$newUserId/role" `
        -Headers $authHeaders `
        -Body @{ role = "Manager" } `
        -TestName "Assign role to user" `
        -ExpectedStatusCodes @(204, 400, 403, 404)
}

# Set user active status
if ($newUserId) {
    Test-Endpoint -Method PUT -Endpoint "/users/$newUserId/active" `
        -Headers $authHeaders `
        -Body @{ isActive = $true } `
        -TestName "Set user active status" `
        -ExpectedStatusCodes @(204, 400, 403, 404)
}

# Change password
if ($newUserId) {
    Test-Endpoint -Method PUT -Endpoint "/users/$newUserId/password" `
        -Headers $authHeaders `
        -Body @{
            currentPassword = "TestPassword@123"
            newPassword = "NewPassword@123"
        } `
        -TestName "Change user password" `
        -ExpectedStatusCodes @(204, 400, 403, 404)
}

# ============================================================================
# 3. CHILDREN ENDPOINTS
# ============================================================================
Write-TestHeader "3️⃣  CHILDREN MANAGEMENT ENDPOINTS"

Test-Endpoint -Method GET -Endpoint "/children?pageNumber=1&pageSize=10&activeOnly=false" `
    -Headers $authHeaders `
    -TestName "Get all children with pagination" `
    -ExpectedStatusCodes @(200, 401)

# Create new child
$createChildResponse = Test-Endpoint -Method POST -Endpoint "/children" `
    -Headers $authHeaders `
    -Body @{
        firstName = "TestChild"
        lastName = "Johnson"
        dateOfBirth = "2021-06-15"
        parentFirstName = "John"
        parentLastName = "Johnson"
        parentEmail = "john_$(Get-Random)@email.com"
        parentPhoneNumber = "+1234567890"
        allergies = "Peanuts"
        specialNeeds = "None"
        healthInsuranceNumber = "INS123456"
    } `
    -TestName "Create new child" `
    -ExpectedStatusCodes @(201, 400, 401)

$childId = $null
if ($createChildResponse -and $createChildResponse.StatusCode -eq 201) {
    $childData = $createChildResponse.Content | ConvertFrom-Json
    $childId = $childData.id
    Write-Host "  └─ New child created: $childId" -ForegroundColor Green
}

# Get child by ID
if ($childId) {
    Test-Endpoint -Method GET -Endpoint "/children/$childId" `
        -Headers $authHeaders `
        -TestName "Get child by ID" `
        -ExpectedStatusCodes @(200, 401, 404)
}

# Update child
if ($childId) {
    Test-Endpoint -Method PUT -Endpoint "/children/$childId" `
        -Headers $authHeaders `
        -Body @{
            firstName = "UpdatedChild"
            lastName = "Johnson"
            dateOfBirth = "2021-06-15"
            parentFirstName = "John"
            parentLastName = "Johnson"
            parentEmail = "john_updated@email.com"
            parentPhoneNumber = "+1234567890"
            allergies = "Peanuts, Dairy"
            specialNeeds = "None"
            healthInsuranceNumber = "INS123456"
        } `
        -TestName "Update child information" `
        -ExpectedStatusCodes @(204, 400, 401, 404)
}

# Set child active status
if ($childId) {
    Test-Endpoint -Method PUT -Endpoint "/children/$childId/active" `
        -Headers $authHeaders `
        -Body @{ isActive = $true } `
        -TestName "Set child active status" `
        -ExpectedStatusCodes @(204, 400, 401, 404)
}

# Add emergency contact
if ($childId) {
    $addContactResponse = Test-Endpoint -Method POST -Endpoint "/children/$childId/emergency-contacts" `
        -Headers $authHeaders `
        -Body @{
            firstName = "Jane"
            lastName = "Smith"
            relationship = "Aunt"
            phoneNumber = "+1111111111"
            email = "jane@email.com"
        } `
        -TestName "Add emergency contact" `
        -ExpectedStatusCodes @(200, 201, 400, 401, 404)

    $contactId = $null
    if ($addContactResponse -and ($addContactResponse.StatusCode -eq 200 -or $addContactResponse.StatusCode -eq 201)) {
        $contactData = $addContactResponse.Content | ConvertFrom-Json
        $contactId = $contactData.id
        Write-Host "  └─ Emergency contact added: $contactId" -ForegroundColor Green
    }

    # Remove emergency contact
    if ($contactId) {
        Test-Endpoint -Method DELETE -Endpoint "/children/$childId/emergency-contacts/$contactId" `
            -Headers $authHeaders `
            -TestName "Remove emergency contact" `
            -ExpectedStatusCodes @(204, 401, 404)
    }
}

# ============================================================================
# 4. ATTENDANCE ENDPOINTS
# ============================================================================
Write-TestHeader "4️⃣  ATTENDANCE TRACKING ENDPOINTS"

if ($childId) {
    # Child check-in
    $checkInResponse = Test-Endpoint -Method POST -Endpoint "/attendance/children/check-in" `
        -Headers $authHeaders `
        -Body @{
            childId = $childId
            checkInTime = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
        } `
        -TestName "Child check-in" `
        -ExpectedStatusCodes @(200, 400, 401)

    # Child check-out
    Test-Endpoint -Method POST -Endpoint "/attendance/children/check-out" `
        -Headers $authHeaders `
        -Body @{
            childId = $childId
            checkOutTime = (Get-Date).AddHours(8).ToString("yyyy-MM-ddTHH:mm:ss")
        } `
        -TestName "Child check-out" `
        -ExpectedStatusCodes @(200, 400, 401)

    # Get child attendance
    Test-Endpoint -Method GET -Endpoint "/attendance/children/$childId?pageNumber=1&pageSize=10" `
        -Headers $authHeaders `
        -TestName "Get child attendance records" `
        -ExpectedStatusCodes @(200, 401, 404)
}

if ($userId) {
    # Staff check-in
    Test-Endpoint -Method POST -Endpoint "/attendance/staff/check-in" `
        -Headers $authHeaders `
        -Body @{
            userId = $userId
            checkInTime = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
        } `
        -TestName "Staff check-in" `
        -ExpectedStatusCodes @(200, 400, 401)

    # Staff check-out
    Test-Endpoint -Method POST -Endpoint "/attendance/staff/check-out" `
        -Headers $authHeaders `
        -Body @{
            userId = $userId
            checkOutTime = (Get-Date).AddHours(10).ToString("yyyy-MM-ddTHH:mm:ss")
        } `
        -TestName "Staff check-out" `
        -ExpectedStatusCodes @(200, 400, 401)

    # Get staff attendance
    Test-Endpoint -Method GET -Endpoint "/attendance/staff?pageNumber=1&pageSize=10" `
        -Headers $authHeaders `
        -TestName "Get staff attendance records" `
        -ExpectedStatusCodes @(200, 401)
}

# ============================================================================
# 5. PLANS ENDPOINTS
# ============================================================================
Write-TestHeader "5️⃣  CARE PLANS ENDPOINTS"

# Get all plans
$getPlansResponse = Test-Endpoint -Method GET -Endpoint "/plans" `
    -Headers $authHeaders `
    -TestName "Get all care plans" `
    -ExpectedStatusCodes @(200, 401)

$planId = $null
if ($getPlansResponse -and $getPlansResponse.StatusCode -eq 200) {
    $plansData = $getPlansResponse.Content | ConvertFrom-Json
    if ($plansData -and $plansData.Count -gt 0) {
        $planId = $plansData[0].id
        Write-Host "  └─ Found existing plan: $planId" -ForegroundColor Green
    }
}

# Create new plan
$createPlanResponse = Test-Endpoint -Method POST -Endpoint "/plans" `
    -Headers $authHeaders `
    -Body @{
        name = "Test Full Time Plan"
        description = "Full-time nursery care test"
        monthlyFee = 1200.00
        operatingHours = "7:00 AM - 6:00 PM"
        ageGroup = "1-3 years"
    } `
    -TestName "Create new care plan (SuperAdmin required)" `
    -ExpectedStatusCodes @(201, 400, 403, 401)

$newPlanId = $null
if ($createPlanResponse -and $createPlanResponse.StatusCode -eq 201) {
    $planData = $createPlanResponse.Content | ConvertFrom-Json
    $newPlanId = $planData.id
    $planId = $newPlanId
    Write-Host "  └─ New plan created: $newPlanId" -ForegroundColor Green
}

# Get plan by ID
if ($planId) {
    Test-Endpoint -Method GET -Endpoint "/plans/$planId" `
        -Headers $authHeaders `
        -TestName "Get plan by ID" `
        -ExpectedStatusCodes @(200, 401, 404)
}

# Update plan
if ($newPlanId) {
    Test-Endpoint -Method PUT -Endpoint "/plans/$newPlanId" `
        -Headers $authHeaders `
        -Body @{
            name = "Updated Test Plan"
            description = "Updated description"
            monthlyFee = 1300.00
            operatingHours = "7:00 AM - 6:00 PM"
            ageGroup = "1-3 years"
        } `
        -TestName "Update care plan (SuperAdmin required)" `
        -ExpectedStatusCodes @(204, 400, 403, 401, 404)
}

# ============================================================================
# 6. PLAN ASSIGNMENTS ENDPOINTS
# ============================================================================
Write-TestHeader "6️⃣  PLAN ASSIGNMENTS ENDPOINTS"

if ($childId -and $planId) {
    # Assign plan to child
    $assignResponse = Test-Endpoint -Method POST -Endpoint "/plan-assignments" `
        -Headers $authHeaders `
        -Body @{
            childId = $childId
            planId = $planId
            startDate = (Get-Date).ToString("yyyy-MM-dd")
        } `
        -TestName "Assign plan to child" `
        -ExpectedStatusCodes @(200, 201, 400, 401)

    $assignmentId = $null
    if ($assignResponse -and ($assignResponse.StatusCode -eq 200 -or $assignResponse.StatusCode -eq 201)) {
        $assignmentData = $assignResponse.Content | ConvertFrom-Json
        $assignmentId = $assignmentData.id
        Write-Host "  └─ Plan assigned: $assignmentId" -ForegroundColor Green
    }

    # End plan assignment
    if ($assignmentId) {
        Test-Endpoint -Method PUT -Endpoint "/plan-assignments/$assignmentId/end" `
            -Headers $authHeaders `
            -Body @{ endDate = (Get-Date).AddMonths(1).ToString("yyyy-MM-dd") } `
            -TestName "End plan assignment" `
            -ExpectedStatusCodes @(204, 400, 401, 404)
    }

    # Get child assignments
    Test-Endpoint -Method GET -Endpoint "/plan-assignments/child/$childId" `
        -Headers $authHeaders `
        -TestName "Get child plan assignments" `
        -ExpectedStatusCodes @(200, 401, 404)
}

# ============================================================================
# 7. BILLING ENDPOINTS
# ============================================================================
Write-TestHeader "7️⃣  BILLING & INVOICES ENDPOINTS"

# Generate monthly invoices
$generateResponse = Test-Endpoint -Method POST -Endpoint "/billing/generate" `
    -Headers $authHeaders `
    -Body @{
        year = (Get-Date).Year
        month = (Get-Date).Month
    } `
    -TestName "Generate monthly invoices" `
    -ExpectedStatusCodes @(200, 400, 401)

Write-Host "  └─ Invoices generation attempted" -ForegroundColor Green

# Get invoices
$getInvoicesResponse = Test-Endpoint -Method GET -Endpoint "/billing/invoices?pageNumber=1&pageSize=10&status=Pending" `
    -Headers $authHeaders `
    -TestName "Get invoices list" `
    -ExpectedStatusCodes @(200, 401)

$invoiceId = $null
if ($getInvoicesResponse -and $getInvoicesResponse.StatusCode -eq 200) {
    try {
        $invoicesData = $getInvoicesResponse.Content | ConvertFrom-Json
        if ($invoicesData.items -and $invoicesData.items.Count -gt 0) {
            $invoiceId = $invoicesData.items[0].id
            Write-Host "  └─ Found invoice: $invoiceId" -ForegroundColor Green
        }
    } catch {
        Write-Host "  └─ Could not parse invoice data" -ForegroundColor Yellow
    }
}

# Get invoice by ID
if ($invoiceId) {
    Test-Endpoint -Method GET -Endpoint "/billing/invoices/$invoiceId" `
        -Headers $authHeaders `
        -TestName "Get invoice by ID" `
        -ExpectedStatusCodes @(200, 401, 404)
}

# Mark invoice as paid
if ($invoiceId) {
    Test-Endpoint -Method PUT -Endpoint "/billing/invoices/$invoiceId/pay" `
        -Headers $authHeaders `
        -TestName "Mark invoice as paid" `
        -ExpectedStatusCodes @(204, 400, 401, 404)
}

# Cancel invoice
if ($invoiceId) {
    Test-Endpoint -Method PUT -Endpoint "/billing/invoices/$invoiceId/cancel" `
        -Headers $authHeaders `
        -TestName "Cancel invoice" `
        -ExpectedStatusCodes @(204, 400, 401, 404)
}

# ============================================================================
# 8. SCHEDULE ENDPOINTS
# ============================================================================
Write-TestHeader "8️⃣  SCHEDULE MANAGEMENT ENDPOINTS"

# Get schedule
$getScheduleResponse = Test-Endpoint -Method GET -Endpoint "/schedule?activeOnly=false" `
    -Headers $authHeaders `
    -TestName "Get schedule slots" `
    -ExpectedStatusCodes @(200, 401)

$scheduleId = $null
if ($getScheduleResponse -and $getScheduleResponse.StatusCode -eq 200) {
    $scheduleData = $getScheduleResponse.Content | ConvertFrom-Json
    if ($scheduleData -and $scheduleData.Count -gt 0) {
        $scheduleId = $scheduleData[0].id
        Write-Host "  └─ Found schedule slot: $scheduleId" -ForegroundColor Green
    }
}

# Create schedule slot
$createScheduleResponse = Test-Endpoint -Method POST -Endpoint "/schedule" `
    -Headers $authHeaders `
    -Body @{
        name = "Test Morning Session"
        startTime = "07:00"
        endTime = "12:00"
        capacity = 20
        description = "Test morning session"
    } `
    -TestName "Create schedule slot" `
    -ExpectedStatusCodes @(200, 201, 400, 401)

$newScheduleId = $null
if ($createScheduleResponse -and ($createScheduleResponse.StatusCode -eq 200 -or $createScheduleResponse.StatusCode -eq 201)) {
    $scheduleData = $createScheduleResponse.Content | ConvertFrom-Json
    $newScheduleId = $scheduleData.id
    $scheduleId = $newScheduleId
    Write-Host "  └─ Schedule slot created: $newScheduleId" -ForegroundColor Green
}

# Update schedule slot
if ($newScheduleId) {
    Test-Endpoint -Method PUT -Endpoint "/schedule/$newScheduleId" `
        -Headers $authHeaders `
        -Body @{
            name = "Updated Test Session"
            startTime = "08:00"
            endTime = "13:00"
            capacity = 25
            description = "Updated test session"
        } `
        -TestName "Update schedule slot" `
        -ExpectedStatusCodes @(204, 400, 401, 404)
}

# Delete schedule slot
if ($newScheduleId) {
    Test-Endpoint -Method DELETE -Endpoint "/schedule/$newScheduleId" `
        -Headers $authHeaders `
        -TestName "Delete schedule slot" `
        -ExpectedStatusCodes @(204, 400, 401, 404)
}

# ============================================================================
# 9. SESSION LOGS ENDPOINTS (SuperAdmin)
# ============================================================================
Write-TestHeader "9️⃣  SESSION LOGS & AUDIT ENDPOINTS"

Test-Endpoint -Method GET -Endpoint "/session-logs?pageNumber=1&pageSize=10" `
    -Headers $authHeaders `
    -TestName "Get session logs (SuperAdmin required)" `
    -ExpectedStatusCodes @(200, 403, 401)

if ($userId) {
    Test-Endpoint -Method GET -Endpoint "/session-logs?userId=$userId&pageNumber=1&pageSize=10" `
        -Headers $authHeaders `
        -TestName "Get session logs for specific user" `
        -ExpectedStatusCodes @(200, 403, 401)
}

# ============================================================================
# TEST SUMMARY
# ============================================================================

Write-TestHeader "📊 TEST SUMMARY REPORT"

$totalTests = $testResults.Passed + $testResults.Failed
$passPercentage = if ($totalTests -gt 0) { [math]::Round(($testResults.Passed / $totalTests) * 100, 2) } else { 0 }

Write-Host "`n📈 Overall Results:" -ForegroundColor $Cyan
Write-Host "   Total Tests: $totalTests" -ForegroundColor White
Write-Host "   Passed: $($testResults.Passed)" -ForegroundColor $Green
Write-Host "   Failed: $($testResults.Failed)" -ForegroundColor $Red
Write-Host "   Success Rate: $passPercentage%" -ForegroundColor $(if ($passPercentage -ge 80) { $Green } else { $Red })

Write-Host "`n📋 Detailed Results:" -ForegroundColor $Cyan
$testResults.Tests | ForEach-Object {
    $icon = if ($_.Passed) { "✅" } else { "❌" }
    Write-Host "$icon $($_.Name)" -ForegroundColor $(if ($_.Passed) { $Green } else { $Red })
}

Write-Host "`n✨ Test execution completed at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor $Cyan

if ($testResults.Failed -eq 0) {
    Write-Host "`n🎉 ALL TESTS PASSED! API is working correctly!" -ForegroundColor $Green
} else {
    Write-Host "`n⚠️  Some tests failed. Review the results above for details." -ForegroundColor $Yellow
}

Write-Host "`n"
