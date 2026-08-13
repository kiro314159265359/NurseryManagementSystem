# Nursery Management System API Documentation

## Table of Contents
1. [Overview](#overview)
2. [Base URL & Authentication](#base-url--authentication)
3. [API Endpoints](#api-endpoints)
4. [Data Models](#data-models)
5. [Error Handling](#error-handling)
6. [Getting Started](#getting-started)

---

## Overview

The Nursery Management System API is a comprehensive RESTful API designed for managing nursery operations including:
- **User Management**: Create and manage staff members with role-based access
- **Children Management**: Register and track children in the nursery
- **Attendance Tracking**: Record check-in and check-out times for children and staff
- **Billing System**: Generate and manage monthly invoices
- **Care Plans**: Create and assign care plans to children
- **Schedule Management**: Manage nursery schedule slots
- **Session Logging**: Audit trail for user logins and activities

**Base URL**: `http://localhost:5293/api` (Development)

---

## Base URL & Authentication

### API Base URL
```
http://localhost:5293/api
https://localhost:7007/api (HTTPS)
```

### Authentication
All endpoints (except login) require Bearer token authentication. Include the token in the Authorization header:

```
Authorization: Bearer <accessToken>
```

### Token Management
- Access tokens expire after **60 minutes** (configurable)
- Refresh tokens expire after **7 days** (configurable)
- Use the Refresh endpoint to get a new access token before expiry

---

## API Endpoints

### 1. Authentication Endpoints

#### 1.1 User Login
**POST** `/auth/login`

Login with email and password to obtain access and refresh tokens.

**Request Body:**
```json
{
  "email": "admin@nursery.com",
  "password": "Admin@123"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "d7c9e5f8-1a2b-4c5d-9e8f-7a6b5c4d3e2f",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "expiresIn": 3600
}
```

**Error Responses:**
- `400 Bad Request`: Invalid email or password
- `401 Unauthorized`: Account not found or inactive

---

#### 1.2 Refresh Token
**POST** `/auth/refresh`

Obtain a new access token using a valid refresh token.

**Request Body:**
```json
{
  "refreshToken": "d7c9e5f8-1a2b-4c5d-9e8f-7a6b5c4d3e2f"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4-e5f6-7a8b-9c0d-e1f2a3b4c5d6",
  "expiresIn": 3600
}
```

---

#### 1.3 Revoke Token (Logout)
**POST** `/auth/revoke`

Invalidate the refresh token and log out the user.

**Headers:**
```
Authorization: Bearer <accessToken>
```

**Request Body:**
```json
{
  "refreshToken": "d7c9e5f8-1a2b-4c5d-9e8f-7a6b5c4d3e2f"
}
```

**Response (204 No Content)**

---

### 2. Users Endpoints
**Authorization**: All endpoints require **SuperAdmin** role

#### 2.1 Get All Users
**GET** `/users?pageNumber=1&pageSize=20&search=`

Retrieve a paginated list of all users with optional search.

**Query Parameters:**
- `pageNumber` (int, default: 1): Page number
- `pageSize` (int, default: 20): Items per page
- `search` (string, optional): Search by name or email

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@nursery.com",
      "phoneNumber": "+1234567890",
      "role": "Manager",
      "isActive": true,
      "createdAt": "2024-01-01T10:00:00Z"
    }
  ],
  "totalCount": 45,
  "pageNumber": 1,
  "totalPages": 3
}
```

---

#### 2.2 Get User By ID
**GET** `/users/{id}`

Retrieve detailed information about a specific user.

**Path Parameters:**
- `id` (UUID): User ID

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@nursery.com",
  "phoneNumber": "+1234567890",
  "role": "Manager",
  "isActive": true,
  "createdAt": "2024-01-01T10:00:00Z"
}
```

---

#### 2.3 Create User
**POST** `/users`

Create a new user account.

**Request Body:**
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@nursery.com",
  "password": "SecurePass@123",
  "role": "Staff",
  "phoneNumber": "+1987654321"
}
```

**Validation Rules:**
- Email must be unique and valid
- Password must be at least 8 characters
- Role must be one of: SuperAdmin, Manager, Staff

**Response (201 Created):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001"
}
```

---

#### 2.4 Update User
**PUT** `/users/{id}`

Update user information.

**Path Parameters:**
- `id` (UUID): User ID

**Request Body:**
```json
{
  "firstName": "Jane",
  "lastName": "Johnson",
  "phoneNumber": "+1234567890"
}
```

**Response (204 No Content)**

---

#### 2.5 Assign Role
**PUT** `/users/{id}/role`

Change user role.

**Path Parameters:**
- `id` (UUID): User ID

**Request Body:**
```json
{
  "role": "Manager"
}
```

**Response (204 No Content)**

---

#### 2.6 Set User Active Status
**PUT** `/users/{id}/active`

Activate or deactivate a user account.

**Path Parameters:**
- `id` (UUID): User ID

**Request Body:**
```json
{
  "isActive": true
}
```

**Response (204 No Content)**

---

#### 2.7 Change Password
**PUT** `/users/{id}/password`

Change user password.

**Path Parameters:**
- `id` (UUID): User ID

**Request Body:**
```json
{
  "currentPassword": "OldPass@123",
  "newPassword": "NewPass@123"
}
```

**Response (204 No Content)**

---

### 3. Children Endpoints
**Authorization**: All endpoints require authentication

#### 3.1 Get All Children
**GET** `/children?pageNumber=1&pageSize=20&search=&activeOnly=false`

Retrieve paginated list of children.

**Query Parameters:**
- `pageNumber` (int, default: 1): Page number
- `pageSize` (int, default: 20): Items per page
- `search` (string, optional): Search by child or parent name
- `activeOnly` (bool, default: false): Show only active children

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "firstName": "Sarah",
      "lastName": "Johnson",
      "dateOfBirth": "2020-06-15",
      "parentName": "Michael Johnson",
      "parentEmail": "michael@email.com",
      "isActive": true,
      "enrollmentDate": "2023-01-15"
    }
  ],
  "totalCount": 120,
  "pageNumber": 1,
  "totalPages": 6
}
```

---

#### 3.2 Get Child By ID
**GET** `/children/{id}`

Retrieve detailed information about a child including emergency contacts.

**Path Parameters:**
- `id` (UUID): Child ID

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "firstName": "Sarah",
  "lastName": "Johnson",
  "dateOfBirth": "2020-06-15",
  "parentFirstName": "Michael",
  "parentLastName": "Johnson",
  "parentEmail": "michael@email.com",
  "parentPhoneNumber": "+1234567890",
  "allergies": "Peanuts, Dairy",
  "specialNeeds": "None",
  "healthInsuranceNumber": "INS123456",
  "isActive": true,
  "enrollmentDate": "2023-01-15",
  "emergencyContacts": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440003",
      "firstName": "Jane",
      "lastName": "Smith",
      "relationship": "Aunt",
      "phoneNumber": "+1111111111",
      "email": "jane@email.com"
    }
  ]
}
```

---

#### 3.3 Create Child
**POST** `/children`

Register a new child in the nursery.

**Request Body:**
```json
{
  "firstName": "Emma",
  "lastName": "Wilson",
  "dateOfBirth": "2021-03-20",
  "parentFirstName": "David",
  "parentLastName": "Wilson",
  "parentEmail": "david@email.com",
  "parentPhoneNumber": "+1555555555",
  "allergies": "None",
  "specialNeeds": "Nut-free snacks required",
  "healthInsuranceNumber": "INS654321"
}
```

**Response (201 Created):**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440004"
}
```

---

#### 3.4 Update Child
**PUT** `/children/{id}`

Update child information.

**Path Parameters:**
- `id` (UUID): Child ID

**Request Body:** Same as Create Child

**Response (204 No Content)**

---

#### 3.5 Set Child Active Status
**PUT** `/children/{id}/active`

Activate or deactivate a child's enrollment.

**Path Parameters:**
- `id` (UUID): Child ID

**Request Body:**
```json
{
  "isActive": false
}
```

**Response (204 No Content)**

---

#### 3.6 Add Emergency Contact
**POST** `/children/{childId}/emergency-contacts`

Add an emergency contact for a child.

**Path Parameters:**
- `childId` (UUID): Child ID

**Request Body:**
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "relationship": "Grandparent",
  "phoneNumber": "+1666666666",
  "email": "jane@email.com"
}
```

**Response (200 OK):**
```json
{
  "id": "880e8400-e29b-41d4-a716-446655440005"
}
```

---

#### 3.7 Remove Emergency Contact
**DELETE** `/children/{childId}/emergency-contacts/{contactId}`

Remove an emergency contact.

**Path Parameters:**
- `childId` (UUID): Child ID
- `contactId` (UUID): Emergency contact ID

**Response (204 No Content)**

---

### 4. Attendance Endpoints
**Authorization**: All endpoints require authentication

#### 4.1 Child Check-In
**POST** `/attendance/children/check-in`

Record a child's arrival time.

**Request Body:**
```json
{
  "childId": "550e8400-e29b-41d4-a716-446655440002",
  "checkInTime": "2024-01-15T08:30:00"
}
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440010",
  "childId": "550e8400-e29b-41d4-a716-446655440002",
  "childName": "Sarah Johnson",
  "date": "2024-01-15",
  "checkInTime": "2024-01-15T08:30:00",
  "checkOutTime": null,
  "durationHours": null
}
```

---

#### 4.2 Child Check-Out
**POST** `/attendance/children/check-out`

Record a child's departure time.

**Request Body:**
```json
{
  "childId": "550e8400-e29b-41d4-a716-446655440002",
  "checkOutTime": "2024-01-15T17:00:00"
}
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440010",
  "childId": "550e8400-e29b-41d4-a716-446655440002",
  "childName": "Sarah Johnson",
  "date": "2024-01-15",
  "checkInTime": "2024-01-15T08:30:00",
  "checkOutTime": "2024-01-15T17:00:00",
  "durationHours": 8.5
}
```

---

#### 4.3 Staff Check-In
**POST** `/attendance/staff/check-in`

Record staff member's arrival time.

**Request Body:**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "checkInTime": "2024-01-15T07:30:00"
}
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440011",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "staffName": "John Doe",
  "date": "2024-01-15",
  "checkInTime": "2024-01-15T07:30:00",
  "checkOutTime": null,
  "durationHours": null
}
```

---

#### 4.4 Staff Check-Out
**POST** `/attendance/staff/check-out`

Record staff member's departure time.

**Request Body:**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "checkOutTime": "2024-01-15T18:00:00"
}
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440011",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "staffName": "John Doe",
  "date": "2024-01-15",
  "checkInTime": "2024-01-15T07:30:00",
  "checkOutTime": "2024-01-15T18:00:00",
  "durationHours": 10.5
}
```

---

#### 4.5 Get Child Attendance
**GET** `/attendance/children/{childId}?from=2024-01-01&to=2024-01-31&pageNumber=1&pageSize=20`

Retrieve child attendance records within a date range.

**Path Parameters:**
- `childId` (UUID): Child ID

**Query Parameters:**
- `from` (date, optional): Start date (YYYY-MM-DD)
- `to` (date, optional): End date (YYYY-MM-DD)
- `pageNumber` (int, default: 1): Page number
- `pageSize` (int, default: 20): Items per page

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440010",
      "childId": "550e8400-e29b-41d4-a716-446655440002",
      "childName": "Sarah Johnson",
      "date": "2024-01-15",
      "checkInTime": "2024-01-15T08:30:00",
      "checkOutTime": "2024-01-15T17:00:00",
      "durationHours": 8.5
    }
  ],
  "totalCount": 22,
  "pageNumber": 1,
  "totalPages": 2
}
```

---

#### 4.6 Get Staff Attendance
**GET** `/attendance/staff?userId=&from=&to=&pageNumber=1&pageSize=20`

Retrieve staff attendance records with optional filters.

**Query Parameters:**
- `userId` (UUID, optional): Filter by staff member
- `from` (date, optional): Start date (YYYY-MM-DD)
- `to` (date, optional): End date (YYYY-MM-DD)
- `pageNumber` (int, default: 1): Page number
- `pageSize` (int, default: 20): Items per page

**Response (200 OK):** Same structure as Get Child Attendance

---

### 5. Plans Endpoints
**Authorization**: All endpoints require authentication

#### 5.1 Get All Plans
**GET** `/plans`

Retrieve all available care plans.

**Response (200 OK):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440020",
    "name": "Full Time Plan",
    "description": "Full-time nursery care from 7:00 AM to 6:00 PM",
    "monthlyFee": 1200.00,
    "operatingHours": "7:00 AM - 6:00 PM",
    "ageGroup": "1-3 years",
    "isActive": true,
    "createdAt": "2024-01-01T10:00:00Z"
  },
  {
    "id": "550e8400-e29b-41d4-a716-446655440021",
    "name": "Part Time Plan",
    "description": "Half-day nursery care",
    "monthlyFee": 600.00,
    "operatingHours": "7:00 AM - 12:00 PM",
    "ageGroup": "1-5 years",
    "isActive": true,
    "createdAt": "2024-01-01T10:00:00Z"
  }
]
```

---

#### 5.2 Get Plan By ID
**GET** `/plans/{id}`

Retrieve detailed information about a specific plan.

**Path Parameters:**
- `id` (UUID): Plan ID

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440020",
  "name": "Full Time Plan",
  "description": "Full-time nursery care from 7:00 AM to 6:00 PM",
  "monthlyFee": 1200.00,
  "operatingHours": "7:00 AM - 6:00 PM",
  "ageGroup": "1-3 years",
  "isActive": true,
  "createdAt": "2024-01-01T10:00:00Z"
}
```

---

#### 5.3 Create Plan
**POST** `/plans`

Create a new care plan.

**Authorization**: Requires **SuperAdmin** role

**Request Body:**
```json
{
  "name": "Weekend Plan",
  "description": "Weekend-only nursery care",
  "monthlyFee": 400.00,
  "operatingHours": "9:00 AM - 4:00 PM",
  "ageGroup": "2-5 years"
}
```

**Response (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440022"
}
```

---

#### 5.4 Update Plan
**PUT** `/plans/{id}`

Update a care plan.

**Authorization**: Requires **SuperAdmin** role

**Path Parameters:**
- `id` (UUID): Plan ID

**Request Body:** Same as Create Plan

**Response (204 No Content)**

---

#### 5.5 Delete Plan
**DELETE** `/plans/{id}`

Delete a care plan.

**Authorization**: Requires **SuperAdmin** role

**Path Parameters:**
- `id` (UUID): Plan ID

**Response (204 No Content)**

---

### 6. Plan Assignments Endpoints
**Authorization**: All endpoints require authentication

#### 6.1 Assign Plan
**POST** `/plan-assignments`

Assign a care plan to a child.

**Request Body:**
```json
{
  "childId": "550e8400-e29b-41d4-a716-446655440002",
  "planId": "550e8400-e29b-41d4-a716-446655440020",
  "startDate": "2024-01-15"
}
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440030"
}
```

---

#### 6.2 End Plan Assignment
**PUT** `/plan-assignments/{id}/end`

Terminate a plan assignment.

**Path Parameters:**
- `id` (UUID): Assignment ID

**Request Body:**
```json
{
  "endDate": "2024-12-31"
}
```

**Response (204 No Content)**

---

#### 6.3 Get Child Assignments
**GET** `/plan-assignments/child/{childId}`

Retrieve all plan assignments for a child.

**Path Parameters:**
- `childId` (UUID): Child ID

**Response (200 OK):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440030",
    "childId": "550e8400-e29b-41d4-a716-446655440002",
    "childName": "Sarah Johnson",
    "planId": "550e8400-e29b-41d4-a716-446655440020",
    "planName": "Full Time Plan",
    "monthlyFee": 1200.00,
    "startDate": "2024-01-15",
    "endDate": null,
    "isActive": true
  }
]
```

---

### 7. Billing Endpoints
**Authorization**: All endpoints require authentication

#### 7.1 Generate Monthly Invoices
**POST** `/billing/generate`

Generate monthly invoices for all active plan assignments.

**Request Body:**
```json
{
  "year": 2024,
  "month": 1
}
```

**Response (200 OK):**
```json
{
  "generated": 45
}
```

---

#### 7.2 Get Invoices
**GET** `/billing/invoices?childId=&status=&year=&month=&pageNumber=1&pageSize=20`

Retrieve paginated list of invoices with optional filters.

**Query Parameters:**
- `childId` (UUID, optional): Filter by child
- `status` (string, optional): Filter by status (Pending, Paid, Cancelled)
- `year` (int, optional): Filter by year
- `month` (int, optional): Filter by month (1-12)
- `pageNumber` (int, default: 1): Page number
- `pageSize` (int, default: 20): Items per page

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440040",
      "childId": "550e8400-e29b-41d4-a716-446655440002",
      "childName": "Sarah Johnson",
      "planName": "Full Time Plan",
      "amount": 1200.00,
      "month": 1,
      "year": 2024,
      "status": "Pending",
      "issuedDate": "2024-01-01T00:00:00Z",
      "dueDate": "2024-02-01T00:00:00Z",
      "paidDate": null
    }
  ],
  "totalCount": 45,
  "pageNumber": 1,
  "totalPages": 3
}
```

---

#### 7.3 Get Invoice By ID
**GET** `/billing/invoices/{id}`

Retrieve detailed information about a specific invoice.

**Path Parameters:**
- `id` (UUID): Invoice ID

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440040",
  "childId": "550e8400-e29b-41d4-a716-446655440002",
  "childName": "Sarah Johnson",
  "parentEmail": "michael@email.com",
  "planName": "Full Time Plan",
  "amount": 1200.00,
  "month": 1,
  "year": 2024,
  "status": "Pending",
  "issuedDate": "2024-01-01T00:00:00Z",
  "dueDate": "2024-02-01T00:00:00Z",
  "paidDate": null,
  "description": "Nursery care services for January 2024"
}
```

---

#### 7.4 Mark Invoice Paid
**PUT** `/billing/invoices/{id}/pay`

Mark an invoice as paid.

**Path Parameters:**
- `id` (UUID): Invoice ID

**Response (204 No Content)**

---

#### 7.5 Cancel Invoice
**PUT** `/billing/invoices/{id}/cancel`

Cancel an invoice.

**Path Parameters:**
- `id` (UUID): Invoice ID

**Response (204 No Content)**

---

### 8. Schedule Endpoints
**Authorization**: All endpoints require authentication

#### 8.1 Get Schedule
**GET** `/schedule?activeOnly=false`

Retrieve all schedule slots.

**Query Parameters:**
- `activeOnly` (bool, default: false): Show only active slots

**Response (200 OK):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440050",
    "name": "Morning Session",
    "startTime": "07:00",
    "endTime": "12:00",
    "capacity": 20,
    "currentEnrollment": 18,
    "description": "Morning nursery session",
    "isActive": true,
    "createdAt": "2024-01-01T10:00:00Z"
  },
  {
    "id": "550e8400-e29b-41d4-a716-446655440051",
    "name": "Afternoon Session",
    "startTime": "12:00",
    "endTime": "18:00",
    "capacity": 20,
    "currentEnrollment": 15,
    "description": "Afternoon nursery session",
    "isActive": true,
    "createdAt": "2024-01-01T10:00:00Z"
  }
]
```

---

#### 8.2 Create Schedule Slot
**POST** `/schedule`

Create a new schedule slot.

**Request Body:**
```json
{
  "name": "Evening Session",
  "startTime": "18:00",
  "endTime": "20:00",
  "capacity": 10,
  "description": "Evening nursery session for working parents"
}
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440052"
}
```

---

#### 8.3 Update Schedule Slot
**PUT** `/schedule/{id}`

Update a schedule slot.

**Path Parameters:**
- `id` (UUID): Schedule slot ID

**Request Body:** Same as Create Schedule Slot

**Response (204 No Content)**

---

#### 8.4 Delete Schedule Slot
**DELETE** `/schedule/{id}`

Delete a schedule slot.

**Path Parameters:**
- `id` (UUID): Schedule slot ID

**Response (204 No Content)**

---

### 9. Session Logs Endpoints
**Authorization**: Requires **SuperAdmin** role

#### 9.1 Get Session Logs
**GET** `/session-logs?userId=&pageNumber=1&pageSize=20`

Retrieve paginated list of user session logs.

**Query Parameters:**
- `userId` (UUID, optional): Filter by specific user
- `pageNumber` (int, default: 1): Page number
- `pageSize` (int, default: 20): Items per page

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440060",
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "userName": "John Doe",
      "ipAddress": "192.168.1.100",
      "loginTime": "2024-01-15T08:00:00Z",
      "logoutTime": "2024-01-15T18:00:00Z"
    }
  ],
  "totalCount": 250,
  "pageNumber": 1,
  "totalPages": 13
}
```

---

## Data Models

### User Model
```typescript
interface User {
  id: string (UUID);
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  role: "SuperAdmin" | "Manager" | "Staff";
  isActive: boolean;
  createdAt: datetime;
  updatedAt?: datetime;
}
```

### Child Model
```typescript
interface Child {
  id: string (UUID);
  firstName: string;
  lastName: string;
  dateOfBirth: date;
  parentFirstName: string;
  parentLastName: string;
  parentEmail: string;
  parentPhoneNumber: string;
  allergies?: string;
  specialNeeds?: string;
  healthInsuranceNumber?: string;
  isActive: boolean;
  enrollmentDate: date;
  emergencyContacts: EmergencyContact[];
}

interface EmergencyContact {
  id: string (UUID);
  firstName: string;
  lastName: string;
  relationship: string;
  phoneNumber: string;
  email?: string;
}
```

### Attendance Model
```typescript
interface ChildAttendance {
  id: string (UUID);
  childId: string (UUID);
  childName: string;
  date: date;
  checkInTime?: datetime;
  checkOutTime?: datetime;
  durationHours?: decimal;
}

interface StaffAttendance {
  id: string (UUID);
  userId: string (UUID);
  staffName: string;
  date: date;
  checkInTime?: datetime;
  checkOutTime?: datetime;
  durationHours?: decimal;
}
```

### Plan Model
```typescript
interface Plan {
  id: string (UUID);
  name: string;
  description: string;
  monthlyFee: decimal;
  operatingHours: string;
  ageGroup: string;
  isActive: boolean;
  createdAt: datetime;
}
```

### Invoice Model
```typescript
interface Invoice {
  id: string (UUID);
  childId: string (UUID);
  childName: string;
  planName: string;
  amount: decimal;
  month: integer (1-12);
  year: integer;
  status: "Pending" | "Paid" | "Cancelled";
  issuedDate: datetime;
  dueDate: datetime;
  paidDate?: datetime;
}
```

### Schedule Model
```typescript
interface ScheduleSlot {
  id: string (UUID);
  name: string;
  startTime: time (HH:mm);
  endTime: time (HH:mm);
  capacity: integer;
  currentEnrollment: integer;
  description?: string;
  isActive: boolean;
  createdAt: datetime;
}
```

---

## Error Handling

### Standard Error Response
All error responses follow this format:

```json
{
  "status": 400,
  "message": "Error description",
  "errors": {
    "fieldName": ["Error message for field"]
  },
  "timestamp": "2024-01-15T10:00:00Z"
}
```

### Common HTTP Status Codes

| Status | Description |
|--------|-------------|
| `200 OK` | Request successful |
| `201 Created` | Resource created successfully |
| `204 No Content` | Request successful, no content to return |
| `400 Bad Request` | Invalid request parameters or validation error |
| `401 Unauthorized` | Authentication required or token invalid |
| `403 Forbidden` | Insufficient permissions (role-based) |
| `404 Not Found` | Resource not found |
| `409 Conflict` | Resource already exists (e.g., duplicate email) |
| `422 Unprocessable Entity` | Validation error with details |
| `500 Internal Server Error` | Server error |

### Common Error Messages

#### Authentication Errors
- `Invalid credentials`: Email or password is incorrect
- `Token expired`: Access token has expired, use refresh token
- `Invalid token`: Token is malformed or invalid
- `Unauthorized`: User is not authenticated

#### Validation Errors
- `Email already exists`: Email must be unique
- `Invalid email format`: Email doesn't match valid format
- `Password too weak`: Password must meet requirements
- `Child age exceeds plan limit`: Child's age doesn't match plan's age group

#### Business Logic Errors
- `Active plan assignment exists`: Child already has active plan
- `Invoice already paid`: Cannot modify paid invoice
- `Insufficient permissions`: User role doesn't allow action
- `Resource in use`: Cannot delete resource with dependencies

---

## Getting Started

### Step 1: Set Up Postman Environment
1. Download the Postman collection: `NurseryManagementSystem.postman_collection.json`
2. Import into Postman
3. Create a new environment with variables:
   - `baseUrl`: `http://localhost:5293`
   - `accessToken`: (will be set automatically after login)
   - `refreshToken`: (will be set automatically after login)
   - `userId`: (will be set automatically after login)

### Step 2: Login
1. Navigate to the Authentication → Login request
2. Update email/password if needed (default: admin@nursery.com / Admin@123)
3. Send the request
4. Tokens will be automatically saved to environment variables

### Step 3: Test Endpoints
1. Use the saved environment variables in other requests
2. Start with GET endpoints to retrieve data
3. Test POST endpoints to create resources
4. Use PUT endpoints to update
5. Use DELETE endpoints to remove resources

### Step 4: Common Workflows

#### Create a Child and Assign Plan
1. **Create User** (Manager or Staff role)
2. **Create Child** with parent information
3. **Get Plans** to see available plans
4. **Assign Plan** to the child
5. **Generate Invoices** for the month

#### Track Daily Attendance
1. **Child Check-In** when arriving
2. **Child Check-Out** when leaving
3. **Get Child Attendance** to view records
4. **Staff Check-In/Out** for staff members

#### Manage Billing
1. **Generate Monthly Invoices** at start of month
2. **Get Invoices** to view pending payments
3. **Mark Invoice Paid** when payment received
4. **Cancel Invoice** if needed

---

## Best Practices

### Security
- Keep your API credentials secure
- Never commit tokens to version control
- Rotate refresh tokens regularly
- Use HTTPS in production
- Implement rate limiting on frontend calls

### Performance
- Use pagination for large datasets (default: 20 items)
- Filter data on the server side when possible
- Cache static data (plans, schedules)
- Implement request debouncing on the frontend

### Error Handling
- Always check response status codes
- Display user-friendly error messages
- Log errors for debugging
- Implement retry logic for failed requests

### API Usage
- Use appropriate HTTP methods
- Include required headers in all requests
- Validate input data before sending
- Handle token expiration gracefully
- Implement logout functionality

---

## Support & Contact

For API support, issues, or feature requests:
- GitHub: https://github.com/kiro314159265359/NurseryManagementSystem
- Email: support@nurserysystem.com

---

**Last Updated**: January 2024  
**API Version**: 1.0  
**.NET Version**: .NET 10
