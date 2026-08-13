# Nursery Management System API Documentation

## Base URL

**Development**: `http://localhost:5293`  
**Production**: `https://nursery-management-api.onrender.com`

## Authentication

All API endpoints (except login) require JWT authentication.

### Authentication Flow

1. **Login**: Obtain access token and refresh token
2. **Use Access Token**: Include in Authorization header for authenticated requests
3. **Refresh Token**: Use refresh token to get new access token when expired
4. **Revoke Token**: Invalidate refresh token for logout

### Headers

```http
Authorization: Bearer <access_token>
Content-Type: application/json
```

### Default Credentials

- **Username**: `superadmin`
- **Password**: `Admin@12345`

## Response Format

All responses follow this structure:

### Success Response
```json
{
  "data": { /* response data */ },
  "message": "Success message"
}
```

### Error Response
```json
{
  "statusCode": 400,
  "message": "Error message",
  "details": "Additional error details"
}
```

## API Endpoints

---

## Authentication

### Login
**POST** `/api/auth/login`

Login with username and password.

**Request Body:**
```json
{
  "userName": "superadmin",
  "password": "Admin@12345"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123...",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "superadmin",
  "role": "SuperAdmin"
}
```

### Refresh Token
**POST** `/api/auth/refresh`

Refresh access token using refresh token.

**Request Body:**
```json
{
  "refreshToken": "abc123..."
}
```

**Response:**
```json
{
  "accessToken": "new_access_token...",
  "refreshToken": "new_refresh_token..."
}
```

### Revoke Token
**POST** `/api/auth/revoke`

Revoke refresh token (logout).

**Request Body:**
```json
{
  "refreshToken": "abc123..."
}
```

**Response:** `204 No Content`

---

## Users

### Get All Users
**GET** `/api/users`

Get list of all users (SuperAdmin only).

**Query Parameters:**
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)

**Response:**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userName": "superadmin",
      "email": "admin@example.com",
      "role": "SuperAdmin",
      "isActive": true,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

### Get User by ID
**GET** `/api/users/{id}`

Get specific user by ID.

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "superadmin",
  "email": "admin@example.com",
  "role": "SuperAdmin",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Create User
**POST** `/api/users`

Create new user (SuperAdmin only).

**Request Body:**
```json
{
  "userName": "newuser",
  "email": "user@example.com",
  "password": "SecurePass123",
  "role": "SubAdmin"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "newuser",
  "email": "user@example.com",
  "role": "SubAdmin",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Update User
**PUT** `/api/users/{id}`

Update user information.

**Request Body:**
```json
{
  "userName": "updateduser",
  "email": "updated@example.com"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "updateduser",
  "email": "updated@example.com",
  "role": "SubAdmin",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Assign Role
**POST** `/api/users/{id}/assign-role`

Assign role to user (SuperAdmin only).

**Request Body:**
```json
{
  "role": "SuperAdmin"
}
```

**Response:** `204 No Content`

### Change Password
**POST** `/api/users/{id}/change-password`

Change user password.

**Request Body:**
```json
{
  "currentPassword": "OldPass123",
  "newPassword": "NewSecurePass123"
}
```

**Response:** `204 No Content`

### Set User Active Status
**POST** `/api/users/{id}/set-active`

Set user active status (SuperAdmin only).

**Request Body:**
```json
{
  "isActive": true
}
```

**Response:** `204 No Content`

---

## Children

### Get All Children
**GET** `/api/children`

Get list of all children.

**Query Parameters:**
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)
- `isActive`: Filter by active status (optional)

**Response:**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "code": "CHD-001",
      "firstName": "John",
      "lastName": "Doe",
      "dateOfBirth": "2020-01-01",
      "gender": "Male",
      "isActive": true,
      "father": {
        "id": "father-id",
        "name": "Father Name",
        "phone": "1234567890",
        "email": "father@example.com"
      },
      "mother": {
        "id": "mother-id",
        "name": "Mother Name",
        "phone": "0987654321",
        "email": "mother@example.com"
      },
      "emergencyContacts": [],
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

### Get Child by ID
**GET** `/api/children/{id}`

Get specific child by ID.

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "code": "CHD-001",
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "2020-01-01",
  "gender": "Male",
  "isActive": true,
  "father": {
    "id": "father-id",
    "name": "Father Name",
    "phone": "1234567890",
    "email": "father@example.com"
  },
  "mother": {
    "id": "mother-id",
    "name": "Mother Name",
    "phone": "0987654321",
    "email": "mother@example.com"
  },
  "emergencyContacts": [
    {
      "id": "contact-id",
      "name": "Emergency Contact",
      "relationship": "Grandparent",
      "phone": "5555555555"
    }
  ],
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Create Child
**POST** `/api/children`

Create new child.

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "2020-01-01",
  "gender": "Male",
  "father": {
    "name": "Father Name",
    "phone": "1234567890",
    "email": "father@example.com"
  },
  "mother": {
    "name": "Mother Name",
    "phone": "0987654321",
    "email": "mother@example.com"
  }
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "code": "CHD-001",
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "2020-01-01",
  "gender": "Male",
  "isActive": true,
  "father": {
    "id": "father-id",
    "name": "Father Name",
    "phone": "1234567890",
    "email": "father@example.com"
  },
  "mother": {
    "id": "mother-id",
    "name": "Mother Name",
    "phone": "0987654321",
    "email": "mother@example.com"
  },
  "emergencyContacts": [],
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Update Child
**PUT** `/api/children/{id}`

Update child information.

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Smith",
  "dateOfBirth": "2020-01-01",
  "gender": "Male",
  "father": {
    "name": "Father Name",
    "phone": "1234567890",
    "email": "father@example.com"
  },
  "mother": {
    "name": "Mother Name",
    "phone": "0987654321",
    "email": "mother@example.com"
  }
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "code": "CHD-001",
  "firstName": "John",
  "lastName": "Smith",
  "dateOfBirth": "2020-01-01",
  "gender": "Male",
  "isActive": true,
  "father": {
    "id": "father-id",
    "name": "Father Name",
    "phone": "1234567890",
    "email": "father@example.com"
  },
  "mother": {
    "id": "mother-id",
    "name": "Mother Name",
    "phone": "0987654321",
    "email": "mother@example.com"
  },
  "emergencyContacts": [],
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Set Child Active Status
**POST** `/api/children/{id}/set-active`

Set child active status.

**Request Body:**
```json
{
  "isActive": true
}
```

**Response:** `204 No Content`

### Add Emergency Contact
**POST** `/api/children/{id}/emergency-contacts`

Add emergency contact for child.

**Request Body:**
```json
{
  "name": "Emergency Contact",
  "relationship": "Grandparent",
  "phone": "5555555555"
}
```

**Response:** `204 No Content`

### Remove Emergency Contact
**DELETE** `/api/children/{id}/emergency-contacts/{contactId}`

Remove emergency contact for child.

**Response:** `204 No Content`

---

## Plans

### Get All Plans
**GET** `/api/plans`

Get list of all subscription plans.

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Basic Plan",
    "description": "Basic childcare plan",
    "durationHours": 8,
    "dailyFee": 50.00,
    "dailyOvertimeFee": 10.00,
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

### Get Plan by ID
**GET** `/api/plans/{id}`

Get specific plan by ID.

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Basic Plan",
  "description": "Basic childcare plan",
  "durationHours": 8,
  "dailyFee": 50.00,
  "dailyOvertimeFee": 10.00,
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Create Plan
**POST** `/api/plans`

Create new subscription plan (SuperAdmin only).

**Request Body:**
```json
{
  "name": "Premium Plan",
  "description": "Premium childcare plan",
  "durationHours": 10,
  "dailyFee": 75.00,
  "dailyOvertimeFee": 15.00
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Premium Plan",
  "description": "Premium childcare plan",
  "durationHours": 10,
  "dailyFee": 75.00,
  "dailyOvertimeFee": 15.00,
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Update Plan
**PUT** `/api/plans/{id}`

Update subscription plan (SuperAdmin only).

**Request Body:**
```json
{
  "name": "Premium Plan",
  "description": "Updated premium childcare plan",
  "durationHours": 12,
  "dailyFee": 85.00,
  "dailyOvertimeFee": 20.00
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Premium Plan",
  "description": "Updated premium childcare plan",
  "durationHours": 12,
  "dailyFee": 85.00,
  "dailyOvertimeFee": 20.00,
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Delete Plan
**DELETE** `/api/plans/{id}`

Delete subscription plan (SuperAdmin only).

**Response:** `204 No Content`

---

## Plan Assignments

### Get Child Plan Assignments
**GET** `/api/planassignments/child/{childId}`

Get plan assignments for a specific child.

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "childId": "child-id",
    "planId": "plan-id",
    "startDate": "2024-01-01",
    "endDate": "2024-12-31",
    "isActive": true,
    "plan": {
      "id": "plan-id",
      "name": "Basic Plan",
      "dailyFee": 50.00
    }
  }
]
```

### Assign Plan to Child
**POST** `/api/planassignments`

Assign subscription plan to child.

**Request Body:**
```json
{
  "childId": "child-id",
  "planId": "plan-id",
  "startDate": "2024-01-01"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "childId": "child-id",
  "planId": "plan-id",
  "startDate": "2024-01-01",
  "endDate": null,
  "isActive": true
}
```

### End Plan Assignment
**POST** `/api/planassignments/end`

End active plan assignment for child.

**Request Body:**
```json
{
  "childId": "child-id",
  "endDate": "2024-06-30"
}
```

**Response:** `204 No Content`

---

## Attendance

### Child Check-In
**POST** `/api/attendance/child/check-in`

Check in a child.

**Request Body:**
```json
{
  "childCode": "CHD-001",
  "scanType": "QRCode"
}
```

**Response:**
```json
{
  "id": "attendance-id",
  "childId": "child-id",
  "checkInTime": "2024-01-01T08:00:00Z",
  "checkOutTime": null,
  "date": "2024-01-01"
}
```

### Child Check-Out
**POST** `/api/attendance/child/check-out`

Check out a child.

**Request Body:**
```json
{
  "childCode": "CHD-001",
  "scanType": "QRCode"
}
```

**Response:**
```json
{
  "id": "attendance-id",
  "childId": "child-id",
  "checkInTime": "2024-01-01T08:00:00Z",
  "checkOutTime": "2024-01-01T17:00:00Z",
  "date": "2024-01-01",
  "overtimeHours": 1.0,
  "overtimeFee": 10.00
}
```

### Staff Check-In
**POST** `/api/attendance/staff/check-in`

Check in a staff member.

**Request Body:**
```json
{
  "staffCode": "STF-001",
  "scanType": "QRCode"
}
```

**Response:**
```json
{
  "id": "attendance-id",
  "staffId": "staff-id",
  "checkInTime": "2024-01-01T07:30:00Z",
  "checkOutTime": null,
  "date": "2024-01-01"
}
```

### Staff Check-Out
**POST** `/api/attendance/staff/check-out`

Check out a staff member.

**Request Body:**
```json
{
  "staffCode": "STF-001",
  "scanType": "QRCode"
}
```

**Response:**
```json
{
  "id": "attendance-id",
  "staffId": "staff-id",
  "checkInTime": "2024-01-01T07:30:00Z",
  "checkOutTime": "2024-01-01T16:30:00Z",
  "date": "2024-01-01"
}
```

### Get Child Attendance History
**GET** `/api/attendance/child/{childId}`

Get attendance history for a specific child.

**Query Parameters:**
- `startDate`: Start date (optional)
- `endDate`: End date (optional)

**Response:**
```json
[
  {
    "id": "attendance-id",
    "childId": "child-id",
    "checkInTime": "2024-01-01T08:00:00Z",
    "checkOutTime": "2024-01-01T17:00:00Z",
    "date": "2024-01-01",
    "overtimeHours": 1.0,
    "overtimeFee": 10.00
  }
]
```

### Get Staff Attendance History
**GET** `/api/attendance/staff/{staffId}`

Get attendance history for a specific staff member.

**Query Parameters:**
- `startDate`: Start date (optional)
- `endDate`: End date (optional)

**Response:**
```json
[
  {
    "id": "attendance-id",
    "staffId": "staff-id",
    "checkInTime": "2024-01-01T07:30:00Z",
    "checkOutTime": "2024-01-01T16:30:00Z",
    "date": "2024-01-01"
  }
]
```

---

## Schedule

### Get Schedule
**GET** `/api/schedule`

Get daily schedule.

**Query Parameters:**
- `date`: Date to get schedule for (optional, defaults to today)

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "dayOfWeek": "Monday",
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "activity": "Breakfast",
    "isActive": true
  }
]
```

### Create Schedule Slot
**POST** `/api/schedule`

Create new schedule slot.

**Request Body:**
```json
{
  "dayOfWeek": "Monday",
  "startTime": "08:00:00",
  "endTime": "09:00:00",
  "activity": "Breakfast"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "dayOfWeek": "Monday",
  "startTime": "08:00:00",
  "endTime": "09:00:00",
  "activity": "Breakfast",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Update Schedule Slot
**PUT** `/api/schedule/{id}`

Update schedule slot.

**Request Body:**
```json
{
  "dayOfWeek": "Monday",
  "startTime": "08:30:00",
  "endTime": "09:30:00",
  "activity": "Breakfast"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "dayOfWeek": "Monday",
  "startTime": "08:30:00",
  "endTime": "09:30:00",
  "activity": "Breakfast",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Delete Schedule Slot
**DELETE** `/api/schedule/{id}`

Delete schedule slot.

**Response:** `204 No Content`

---

## Billing

### Get All Invoices
**GET** `/api/billing`

Get list of all invoices.

**Query Parameters:**
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)
- `status`: Filter by status (optional: Pending, Paid, Cancelled)

**Response:**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "childId": "child-id",
      "childName": "John Doe",
      "month": "2024-01",
      "planFee": 1500.00,
      "totalOvertimeFee": 100.00,
      "grandTotal": 1600.00,
      "status": "Pending",
      "generatedDate": "2024-01-31T00:00:00Z",
      "paidDate": null
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

### Get Invoice by ID
**GET** `/api/billing/{id}`

Get specific invoice by ID.

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "childId": "child-id",
  "childName": "John Doe",
  "month": "2024-01",
  "planFee": 1500.00,
  "totalOvertimeFee": 100.00,
  "grandTotal": 1600.00,
  "status": "Pending",
  "generatedDate": "2024-01-31T00:00:00Z",
  "paidDate": null
}
```

### Generate Monthly Invoices
**POST** `/api/billing/generate-monthly`

Generate monthly invoices for all children.

**Request Body:**
```json
{
  "year": 2024,
  "month": 1
}
```

**Response:** `204 No Content`

### Mark Invoice as Paid
**POST** `/api/billing/{id}/mark-paid`

Mark invoice as paid.

**Response:** `204 No Content`

### Cancel Invoice
**POST** `/api/billing/{id}/cancel`

Cancel invoice.

**Response:** `204 No Content`

---

## Session Logs

### Get Session Logs
**GET** `/api/sessionlogs`

Get session logs (SuperAdmin only).

**Query Parameters:**
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)
- `startDate`: Start date (optional)
- `endDate`: End date (optional)

**Response:**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "childId": "child-id",
      "childName": "John Doe",
      "checkInTime": "2024-01-01T08:00:00Z",
      "checkOutTime": "2024-01-01T17:00:00Z",
      "durationHours": 9.0,
      "notes": "Normal day"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

### Create Session Log
**POST** `/api/sessionlogs`

Create session log (SuperAdmin only).

**Request Body:**
```json
{
  "childId": "child-id",
  "checkInTime": "2024-01-01T08:00:00Z",
  "checkOutTime": "2024-01-01T17:00:00Z",
  "notes": "Normal day"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "childId": "child-id",
  "childName": "John Doe",
  "checkInTime": "2024-01-01T08:00:00Z",
  "checkOutTime": "2024-01-01T17:00:00Z",
  "durationHours": 9.0,
  "notes": "Normal day",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

---

## Enums

### User Roles
- `SuperAdmin` = 1
- `SubAdmin` = 2

### Invoice Status
- `Pending` = 1
- `Paid` = 2
- `Cancelled` = 3

### Scan Type
- `QRCode` = 1
- `Manual` = 2

### Gender
- `Male`
- `Female`
- `Other`

---

## Error Codes

| Status Code | Description |
|-------------|-------------|
| 200 | Success |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 409 | Conflict |
| 500 | Internal Server Error |

---

## Rate Limiting

Currently, there are no rate limits implemented. Consider implementing rate limiting for production use.

---

## Pagination

For endpoints that support pagination, use these query parameters:
- `page`: Page number (1-based)
- `pageSize`: Number of items per page

Response includes:
- `items`: Array of items
- `totalCount`: Total number of items
- `page`: Current page number
- `pageSize`: Items per page
- `totalPages`: Total number of pages

---

## CORS

The API supports CORS for all origins. In production, you should restrict this to specific frontend domains.

---

## WebSocket Support

Currently, WebSocket support is not implemented. Consider adding real-time features using SignalR for live updates.

---

## Data Validation

All requests are validated using FluentValidation. Common validation errors:
- Required fields missing
- Invalid email format
- Password too short (minimum 8 characters)
- Invalid date formats
- Invalid enum values

---

## Example Usage

### Frontend Integration Example

```javascript
// Login
const login = async (username, password) => {
  const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ userName: username, password })
  });
  const data = await response.json();
  localStorage.setItem('accessToken', data.accessToken);
  localStorage.setItem('refreshToken', data.refreshToken);
  return data;
};

// Authenticated Request
const getChildren = async () => {
  const token = localStorage.getItem('accessToken');
  const response = await fetch(`${API_BASE_URL}/api/children`, {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  return response.json();
};

// Token Refresh
const refreshToken = async () => {
  const refreshToken = localStorage.getItem('refreshToken');
  const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken })
  });
  const data = await response.json();
  localStorage.setItem('accessToken', data.accessToken);
  localStorage.setItem('refreshToken', data.refreshToken);
  return data;
};
```

---

## Testing

Use the provided examples to test each endpoint. Always ensure you have a valid access token before making authenticated requests.

---

## Support

For API-related issues, please contact the development team or create an issue in the GitHub repository.
