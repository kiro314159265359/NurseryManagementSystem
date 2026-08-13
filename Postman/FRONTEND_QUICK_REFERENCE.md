# Frontend Developer Quick Reference Guide

## Quick Start for Frontend Development

### API Base URL
```
Development: http://localhost:5293/api
```

### Authentication Flow

#### 1. Login
```javascript
// Request
POST /auth/login
Content-Type: application/json

{
  "email": "admin@nursery.com",
  "password": "Admin@123"
}

// Response
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "d7c9e5f8-1a2b-4c5d-9e8f-7a6b5c4d3e2f",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "expiresIn": 3600
}
```

#### 2. Using Access Token
```javascript
// Add to all authenticated requests
Authorization: Bearer {accessToken}
```

#### 3. Refresh Token (Token Expiry)
```javascript
// Request
POST /auth/refresh
Content-Type: application/json

{
  "refreshToken": "d7c9e5f8-1a2b-4c5d-9e8f-7a6b5c4d3e2f"
}

// Response
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4-e5f6-7a8b-9c0d-e1f2a3b4c5d6",
  "expiresIn": 3600
}
```

#### 4. Logout/Revoke
```javascript
// Request
POST /auth/revoke
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "refreshToken": "d7c9e5f8-1a2b-4c5d-9e8f-7a6b5c4d3e2f"
}

// Response: 204 No Content
```

---

## Common API Patterns

### List/Search Endpoints (Pagination)
Pattern: `GET /resource?pageNumber=1&pageSize=20&search=optional`

**Example: Get Children**
```javascript
GET /children?pageNumber=1&pageSize=20&search=Sarah&activeOnly=true

Response:
{
  "items": [...],
  "totalCount": 120,
  "pageNumber": 1,
  "totalPages": 6
}
```

**Pagination Implementation:**
```javascript
// Frontend example (JavaScript/React)
const [pageNumber, setPageNumber] = useState(1);
const [pageSize, setPageSize] = useState(20);
const [searchTerm, setSearchTerm] = useState('');

const fetchChildren = async () => {
  const response = await fetch(
    `/api/children?pageNumber=${pageNumber}&pageSize=${pageSize}&search=${searchTerm}`,
    {
      headers: { 'Authorization': `Bearer ${accessToken}` }
    }
  );
  const data = await response.json();
  return data;
};
```

### Create Endpoint
Pattern: `POST /resource`

**Example: Create Child**
```javascript
POST /children
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "firstName": "Sarah",
  "lastName": "Johnson",
  "dateOfBirth": "2020-06-15",
  "parentFirstName": "Michael",
  "parentLastName": "Johnson",
  "parentEmail": "michael@email.com",
  "parentPhoneNumber": "+1234567890",
  "allergies": "Peanuts",
  "specialNeeds": null,
  "healthInsuranceNumber": "INS123"
}

Response: 201 Created
{
  "id": "550e8400-e29b-41d4-a716-446655440002"
}
```

### Update Endpoint
Pattern: `PUT /resource/{id}`

**Example: Update Child**
```javascript
PUT /children/550e8400-e29b-41d4-a716-446655440002
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "firstName": "Sarah",
  "lastName": "Johnson",
  "dateOfBirth": "2020-06-15",
  "parentFirstName": "Michael",
  "parentLastName": "Johnson",
  "parentEmail": "michael.new@email.com",
  "parentPhoneNumber": "+1234567890",
  "allergies": "Peanuts, Dairy",
  "specialNeeds": null,
  "healthInsuranceNumber": "INS123"
}

Response: 204 No Content
```

### Delete Endpoint
Pattern: `DELETE /resource/{id}`

**Example: Delete Child**
```javascript
DELETE /children/550e8400-e29b-41d4-a716-446655440002
Authorization: Bearer {accessToken}

Response: 204 No Content
```

---

## User Roles & Permissions

### Role Hierarchy
1. **SuperAdmin**: Full system access
2. **Manager**: Can manage children, staff, and billing
3. **Staff**: Can perform daily operations (attendance, check-ins)

### Permission Matrix

| Feature | SuperAdmin | Manager | Staff |
|---------|-----------|---------|-------|
| User Management | ✅ | ❌ | ❌ |
| Create/Edit Plans | ✅ | ❌ | ❌ |
| Manage Children | ✅ | ✅ | ❌ |
| View Children | ✅ | ✅ | ✅ |
| Attendance (Check-in/out) | ✅ | ✅ | ✅ |
| Billing Management | ✅ | ✅ | ❌ |
| View Reports | ✅ | ✅ | ✅ |
| Session Logs | ✅ | ❌ | ❌ |

---

## Common Screens & Required Endpoints

### Dashboard
- **GET** `/children` - Total children stats
- **GET** `/attendance/children?from=today` - Today's attendance
- **GET** `/attendance/staff?from=today` - Staff attendance
- **GET** `/billing/invoices?status=Pending` - Pending invoices

### Children Management
- **GET** `/children` - List children
- **POST** `/children` - Create child
- **GET** `/children/{id}` - View child details
- **PUT** `/children/{id}` - Edit child
- **POST** `/children/{id}/emergency-contacts` - Add emergency contact
- **DELETE** `/children/{id}/emergency-contacts/{contactId}` - Remove contact
- **PUT** `/children/{id}/active` - Activate/deactivate

### User Management
- **GET** `/users` - List users
- **POST** `/users` - Create user
- **PUT** `/users/{id}` - Edit user
- **PUT** `/users/{id}/role` - Change role
- **PUT** `/users/{id}/active` - Activate/deactivate

### Attendance Tracking
- **POST** `/attendance/children/check-in` - Child arrival
- **POST** `/attendance/children/check-out` - Child departure
- **POST** `/attendance/staff/check-in` - Staff arrival
- **POST** `/attendance/staff/check-out` - Staff departure
- **GET** `/attendance/children/{childId}` - Child attendance history
- **GET** `/attendance/staff` - Staff attendance history

### Billing
- **POST** `/billing/generate` - Generate invoices
- **GET** `/billing/invoices` - List invoices
- **GET** `/billing/invoices/{id}` - Invoice details
- **PUT** `/billing/invoices/{id}/pay` - Mark paid
- **PUT** `/billing/invoices/{id}/cancel` - Cancel

### Plans & Assignments
- **GET** `/plans` - Available plans
- **POST** `/plan-assignments` - Assign plan to child
- **PUT** `/plan-assignments/{id}/end` - End assignment
- **GET** `/plan-assignments/child/{childId}` - Child's plans

---

## Error Handling

### Response Status Codes
```javascript
if (response.status === 200 || response.status === 201) {
  // Success
} else if (response.status === 204) {
  // Success, no content
} else if (response.status === 400) {
  // Bad request - validation error
} else if (response.status === 401) {
  // Unauthorized - need to login
} else if (response.status === 403) {
  // Forbidden - insufficient permissions
} else if (response.status === 404) {
  // Not found
} else if (response.status === 409) {
  // Conflict - resource already exists
} else if (response.status === 500) {
  // Server error
}
```

### Example Error Response
```json
{
  "status": 400,
  "message": "Validation failed",
  "errors": {
    "email": ["Email already exists"],
    "password": ["Password must be at least 8 characters"]
  },
  "timestamp": "2024-01-15T10:00:00Z"
}
```

### Frontend Error Handling Example
```javascript
const handleApiCall = async (apiFunction) => {
  try {
    const response = await apiFunction();
    if (response.ok) {
      return await response.json();
    } else if (response.status === 401) {
      // Redirect to login
      window.location.href = '/login';
    } else if (response.status === 403) {
      // Show permission denied
      showError('You do not have permission to perform this action');
    } else {
      const errorData = await response.json();
      showError(errorData.message || 'An error occurred');
    }
  } catch (error) {
    console.error('API Error:', error);
    showError('Network error. Please try again.');
  }
};
```

---

## Date/Time Formats

### Date Format
```
YYYY-MM-DD (e.g., 2024-01-15)

// JavaScript conversion
const date = new Date('2024-01-15');
```

### DateTime Format (ISO 8601)
```
YYYY-MM-DDTHH:mm:ss (e.g., 2024-01-15T10:30:00)

// JavaScript conversion
const dateTime = new Date('2024-01-15T10:30:00');
```

### Time Format
```
HH:mm (e.g., 14:30)

// JavaScript conversion
const time = '14:30';
const [hours, minutes] = time.split(':');
```

---

## Field Validations

### Email
- Must be a valid email format
- Must be unique across the system
- Required field

### Password
- Minimum 8 characters
- Must contain uppercase letter
- Must contain lowercase letter
- Must contain digit
- Must contain special character (@, #, $, %, etc.)

### Phone Number
- Valid phone format (example: +1234567890)
- Required field

### Age Validation for Plans
- Child's age must match plan's age group
- Example plans:
  - "1-3 years": Child age 1-3
  - "2-5 years": Child age 2-5
  - "1-5 years": Child age 1-5

### Billing Rules
- Can only generate invoices for active plan assignments
- Invoice amount = Plan's monthly fee
- Invoice due date = 30 days from issued date

---

## Code Examples

### JavaScript/Fetch API

#### Login
```javascript
async function login(email, password) {
  const response = await fetch('http://localhost:5293/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });

  if (response.ok) {
    const data = await response.json();
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('userId', data.userId);
    return data;
  }
  throw new Error('Login failed');
}
```

#### Get Children with Pagination
```javascript
async function getChildren(pageNumber = 1, pageSize = 20, search = '') {
  const token = localStorage.getItem('accessToken');
  const response = await fetch(
    `http://localhost:5293/api/children?pageNumber=${pageNumber}&pageSize=${pageSize}&search=${search}`,
    {
      headers: { 'Authorization': `Bearer ${token}` }
    }
  );

  if (response.ok) {
    return await response.json();
  }
  if (response.status === 401) {
    // Token expired, redirect to login
    window.location.href = '/login';
  }
  throw new Error('Failed to fetch children');
}
```

#### Create Child
```javascript
async function createChild(childData) {
  const token = localStorage.getItem('accessToken');
  const response = await fetch('http://localhost:5293/api/children', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(childData)
  });

  if (response.status === 201) {
    return await response.json();
  }

  const error = await response.json();
  throw new Error(error.message);
}
```

### React Hook Example

```javascript
import { useState, useEffect } from 'react';

function ChildrenList() {
  const [children, setChildren] = useState([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    fetchChildren();
  }, [pageNumber]);

  const fetchChildren = async () => {
    setLoading(true);
    setError(null);

    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(
        `http://localhost:5293/api/children?pageNumber=${pageNumber}&pageSize=20`,
        {
          headers: { 'Authorization': `Bearer ${token}` }
        }
      );

      if (response.status === 401) {
        // Redirect to login
        window.location.href = '/login';
        return;
      }

      if (!response.ok) {
        throw new Error('Failed to fetch children');
      }

      const data = await response.json();
      setChildren(data.items);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div>
      {children.map(child => (
        <div key={child.id}>
          {child.firstName} {child.lastName}
        </div>
      ))}
    </div>
  );
}
```

### Axios Example

```javascript
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5293/api';

// Create axios instance with default headers
const apiClient = axios.create({
  baseURL: API_BASE_URL
});

// Add token to requests
apiClient.interceptors.request.use(config => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle token expiry
apiClient.interceptors.response.use(
  response => response,
  async error => {
    if (error.response.status === 401) {
      const refreshToken = localStorage.getItem('refreshToken');
      try {
        const response = await axios.post(`${API_BASE_URL}/auth/refresh`, {
          refreshToken
        });
        localStorage.setItem('accessToken', response.data.accessToken);
        // Retry original request
        return apiClient(error.config);
      } catch {
        // Redirect to login
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

// API functions
export const childrenAPI = {
  getAll: (pageNumber = 1, pageSize = 20) =>
    apiClient.get('/children', { params: { pageNumber, pageSize } }),

  getById: (id) =>
    apiClient.get(`/children/${id}`),

  create: (data) =>
    apiClient.post('/children', data),

  update: (id, data) =>
    apiClient.put(`/children/${id}`, data),

  delete: (id) =>
    apiClient.delete(`/children/${id}`)
};

// Usage
childrenAPI.getAll(1, 20).then(response => {
  console.log(response.data);
});
```

---

## Testing with Postman

### Steps to Import Collection
1. Open Postman
2. Click "File" → "Import"
3. Select `NurseryManagementSystem.postman_collection.json`
4. Create new environment with variables:
   - `baseUrl`: `http://localhost:5293`
   - `accessToken`: (auto-filled after login)
   - `refreshToken`: (auto-filled after login)
   - `userId`: (auto-filled after login)

### Testing Workflow
1. Run Login request first
2. Tokens automatically saved to environment
3. Use other requests with auto-populated tokens
4. Modify request parameters as needed

---

## Performance Tips

### Pagination Best Practices
- Default page size is 20 items
- Use appropriate page sizes (10-50 items)
- Implement lazy loading for lists
- Cache list data when appropriate

### Search Optimization
- Implement debouncing (300-500ms) for search input
- Don't search on every keystroke
- Use server-side search/filter

### Request Optimization
- Reuse API responses when possible
- Implement request cancellation for previous searches
- Cache static data (plans, roles)
- Use conditional requests (ETag, Last-Modified)

### Example: Search with Debounce
```javascript
import { useState, useEffect } from 'react';
import { debounce } from 'lodash';

function SearchChildren() {
  const [searchTerm, setSearchTerm] = useState('');
  const [results, setResults] = useState([]);

  const debouncedSearch = debounce(async (term) => {
    if (term.length < 2) return;

    const token = localStorage.getItem('accessToken');
    const response = await fetch(
      `http://localhost:5293/api/children?search=${term}`,
      {
        headers: { 'Authorization': `Bearer ${token}` }
      }
    );

    const data = await response.json();
    setResults(data.items);
  }, 300);

  useEffect(() => {
    debouncedSearch(searchTerm);
  }, [searchTerm]);

  return (
    <div>
      <input 
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        placeholder="Search children..."
      />
      {results.map(child => (
        <div key={child.id}>{child.firstName} {child.lastName}</div>
      ))}
    </div>
  );
}
```

---

## Common Issues & Solutions

### Issue: "Token Expired" (401)
**Solution**: Implement automatic token refresh
```javascript
const refreshAccessToken = async () => {
  const refreshToken = localStorage.getItem('refreshToken');
  const response = await fetch('/api/auth/refresh', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken })
  });

  if (response.ok) {
    const data = await response.json();
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    return data.accessToken;
  }
  // Redirect to login if refresh fails
  window.location.href = '/login';
};
```

### Issue: CORS Errors
**Solution**: Check API CORS configuration
- Ensure frontend URL is in allowed origins
- Use relative URLs if frontend and API on same domain
- Add appropriate headers in API response

### Issue: Validation Errors (400)
**Solution**: Validate data before sending
```javascript
const validateChild = (data) => {
  const errors = {};

  if (!data.firstName || data.firstName.trim().length === 0) {
    errors.firstName = 'First name is required';
  }
  if (!data.email || !isValidEmail(data.email)) {
    errors.email = 'Valid email is required';
  }

  return Object.keys(errors).length === 0 ? null : errors;
};

const handleSubmit = async (childData) => {
  const errors = validateChild(childData);
  if (errors) {
    // Show validation errors to user
    return;
  }

  // Send to API
  createChild(childData);
};
```

---

## Resources

- **Postman Collection**: `NurseryManagementSystem.postman_collection.json`
- **Full API Documentation**: `API_DOCUMENTATION.md`
- **GitHub Repository**: https://github.com/kiro314159265359/NurseryManagementSystem
- **.NET 10 API**: Running on http://localhost:5293

---

**Last Updated**: January 2024  
**Version**: 1.0
