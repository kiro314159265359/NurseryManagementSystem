# Nursery Frontend Integration Guide

> Updated 2026-08-30 after the admin-app consistency audit. The contracts in the
> **Consistency upgrade** section below supersede older examples where they differ.

## Consistency upgrade

- Every paginated endpoint returns `items`, `totalCount`, `pageNumber`, `pageSize`, and `totalPages`.
- JSON enums are strings. Accepted values are:
  - `role`: `SuperAdmin`, `SubAdmin`, `Parent`
  - `scanType`: `QRCode`, `Barcode`, `Manual`
  - `accountOwner`: `Mother`, `Father`
  - `approvalStatus`: `Pending`, `Approved`, `Rejected`
  - `invoiceStatus`: `Pending`, `Paid`, `Overdue`, `Cancelled`
- Problem Details errors include a stable `code`. Supported core codes are
  `INVALID_CREDENTIALS`, `ACCOUNT_PENDING_APPROVAL`, `ACCOUNT_DISABLED`,
  `TOKEN_EXPIRED`, `INVALID_REFRESH_TOKEN`, `FORBIDDEN_ROLE`,
  `VALIDATION_FAILED`, `NOT_FOUND`, `CONFLICT`, `INVALID_SCAN_CODE`,
  `ALREADY_CHECKED_IN`, `NOT_CHECKED_IN`, and `INTERNAL_ERROR`.
- `/api/account/me`, `/api/account/password`, and profile update remain supported.
- Money is emitted as JSON numbers. Currency is returned by plan/settings/finance
  endpoints and defaults to `AED`. Timestamps are UTC ISO-8601 instants.
- `POST /api/registrations/admin` may omit `accountOwner` and `password` to create
  a walk-in child without a login. If the selected owner's email already belongs
  to a parent, the child is attached to that existing account.
- Child DTOs now expose `photoUrl`, `scanCode`, `approvalStatus`, derived `status`,
  audit timestamps, emergency-contact IDs, and `currentPlan`.
- Child status values are `Active`, `Inactive`, `Pending`, and `Rejected`.
- Printed child scan codes do not expire and work for both check-in and check-out;
  regenerating a code immediately invalidates the old value.

New/expanded endpoints:

| Area | Endpoint |
|---|---|
| Children | `PUT /api/children/{id}` |
| Children | `PUT /api/children/{id}/status` |
| Children | `POST /api/children/{id}/scan-code/regenerate` |
| Children | `POST` / `DELETE /api/children/{id}/photo` |
| Attendance | `POST /api/attendance/children/{id}/check-in` |
| Attendance | `POST /api/attendance/children/{id}/check-out` |
| Attendance | `GET /api/attendance/today` |
| Billing | `GET /api/billing/summary` |
| Billing | `GET /api/billing/revenue` |
| Billing | `PUT /api/billing/invoices/{id}/adjust` |
| Nursery | `GET /api/nursery/settings` |
| Nursery | `PUT /api/nursery/settings` (`SuperAdmin`) |
| Dashboard | `GET /api/dashboard/summary` |
| Dashboard | `GET /api/dashboard/alerts` |
| Audit | `GET /api/audit-log` (`SuperAdmin`) |

Exact request and response schemas are published at
`GET https://nursery-management-api.runasp.net/swagger/v1/swagger.json`.
Use `PUT /api/children/{id}/status`; `/active` is deprecated compatibility only.
The canonical billing rule is `overtimeHourlyRate × overtimeHours`.
`dailyOvertimeFee` is legacy and is not used for new invoices. Generated
invoices freeze their plan, parent, currency, overtime rate, and penalty rate.
Password changes return `INVALID_CURRENT_PASSWORD` for a wrong existing
password and `WEAK_PASSWORD` when the replacement fails the 8-character policy.

Invoice money fields are `baseFee`, `overtimeAmount`, and `totalDue`.
The duplicate aliases `planFee`, `totalOvertimeFee`, and `grandTotal` are not
part of the current response contract.

## Exact round-2 response contracts

The JSON below uses representative values. Nullable fields may be `null`; no
other field names should be invented by the client.

### Child list and details

`GET /api/children` is paginated and lists approved children only. `search`
matches child name or scan code. `activeOnly=false` includes active and inactive
approved children; pending registrations belong to `/api/registrations/pending`.

```json
{
  "items": [{
    "id": "GUID", "fullName": "Child Name", "dateOfBirth": "2022-01-15",
    "enrollmentDate": "2026-08-30", "nationality": "Egyptian",
    "religion": "", "homeAddress": "Cairo", "allergies": null,
    "photoUrl": null, "scanCode": "CHD-...", "isActive": true,
    "approvalStatus": "Approved", "status": "Active",
    "createdAt": "2026-08-30T12:00:00Z",
    "currentPlan": { "assignmentId": "GUID", "planId": "GUID",
      "planName": "Full Day", "startDate": "2026-08-30", "durationHours": 8 }
  }],
  "totalCount": 1, "pageNumber": 1, "pageSize": 20, "totalPages": 1
}
```

`GET /api/children/{id}` adds `createdBy`, `approvedAt`, `approvedBy`,
`mother`, `father`, `agreement`, and
`emergencyContacts[] { id, name, relationship, phone }`. Both
`approvalStatus` and `status` are intentional: approvalStatus describes review;
status is the operational value `Active | Inactive | Pending | Rejected`.

Photo upload is multipart field `file`, maximum 5 MB, accepting
`image/jpeg`, `image/png`, or `image/webp`; success is
`{ "photoUrl": "https://..." }`. Scan regeneration succeeds with
`{ "scanCode": "CHD-...", "issuedAt": "...Z" }`.

### Attendance

`GET /api/attendance/today?status=All|CheckedIn|CheckedOut` returns:

```json
{
  "items": [{ "childId": "GUID", "childFullName": "Child Name",
    "photoUrl": null, "planName": "Full Day", "allowedHours": 8,
    "isCheckedIn": true, "checkedInAt": "2026-08-30T07:00:00Z",
    "checkedOutAt": null, "hoursOnSite": 3.5, "overtimeHours": 0 }],
  "totalCount": 1, "pageNumber": 1, "pageSize": 20, "totalPages": 1,
  "summary": { "checkedIn": 18, "checkedOut": 24, "totalEnrolled": 42 }
}
```

Summary counts are whole-roster and never page-scoped. Attendance history uses
the plan assignment active on each record's date; `allowedHours` and server-side
`overtimeHours` therefore do not change when the current plan changes. Manual
check-in/out records the current server time; backdating is not supported.

### Plans and assignments

Plan fields are exactly: `id`, `name`, `durationHours`, `isWeekend`,
`monthlyFee`, `dailyOvertimeFee`, `category`, `billingCycle`, `daysPerCycle`,
`isFullDay`, `badgeText`, `isFeatured`, `isActive`, `currency`, `displayOrder`,
and response-only alias `price`. Assigning a new plan atomically ends the open
assignment on the day before the new start date.

`GET /api/planassignments/child/{id}` returns full history newest-first:

```json
[{ "id": "GUID", "childId": "GUID", "planId": "GUID",
  "planName": "Full Day", "planCategory": "Monthly Packages", "price": 3000,
  "durationHours": 8, "daysPerCycle": 5, "startDate": "2026-08-30",
  "endDate": null, "isActive": true, "assignedById": "GUID",
  "assignedByName": "Admin", "assignedAt": "2026-08-30T12:00:00Z",
  "currency": "AED" }]
```

### Invoices

`GET /api/billing/invoices` accepts optional `childId`, `month`, `year`,
`status`, `search`, `pageNumber`, and `pageSize`. Search covers child, mother,
and father names. Invoice objects contain:

```json
{
  "id": "GUID", "invoiceNumber": "INV-2026-08-ABC123",
  "childId": "GUID", "childFullName": "Child Name",
  "parentFullName": "Account Owner", "parentPhone": "+201001234567",
  "billingMonth": 8, "billingYear": 2026, "planId": "GUID",
  "planName": "Full Day", "baseFee": 3000,
  "overtimeHours": 4.5, "overtimeRate": 100,
  "overtimeAmount": 450,
  "latePickupDays": 2, "latePickupFinePerDay": 50, "penaltyAmount": 100,
  "adjustmentAmount": 0, "adjustmentReason": null,
  "totalDue": 3550, "amountPaid": 0,
  "outstanding": 3550, "currency": "AED", "status": "Pending",
  "dueDate": "2026-09-05", "paidAt": null, "paidByName": null,
  "markedPaidById": null, "createdAt": "2026-09-01T00:00:00Z"
}
```

Generation returns `{ "generated": 3 }`, is idempotent per child/month, and
only creates missing invoices for active approved children. Parent means the
login account owner. Snapshot fields and rates never change after generation.

### Registration, dashboard, schedule, and audit

Pending registrations are an unpaginated array and include `childId`,
`childFullName`, `dateOfBirth`, `enrollmentDate`, `approvalStatus`,
`parentUserId`, `parentFullName`, `parentEmail`, `parentPhone`, `accountOwner`,
`requestedPlanId`, `requestedPlanName`, `isFirstChild`, `rejectionReason`, and
`submittedAt`. Approve/reject return HTTP 204. Reject reason is required and
limited to 500 characters. Approved children become operational immediately;
attendance works without a plan, while plan-based allowed/overtime values are
null/zero until one is assigned.

Dashboard summary fields: `date`, `checkedInNow`, `capacity`, `totalEnrolled`,
`attendedToday`, `childHoursToday`, `overtimeHoursToday`, `revenueToday`,
`outstandingTotal`, `unpaidInvoiceCount`, `pendingRegistrationsCount`, and
`currency`. Dashboard alerts currently return
`{ "items": [{ "kind": "OvertimeLive", "childId", "childFullName",
"parentFullName", "parentPhone", "hours", "amount", "isUrgent" }] }`.

Schedule times are nursery-local wall-clock values interpreted with
`nursery.settings.timeZone`. The current schedule is one shared daily routine,
not weekday-specific. Audit filters are `from`, `to`, `userId`, `action`,
`pageNumber`, and `pageSize`.

## Frontend completion checklist

- Import the Postman collection and keep `baseUrl` unchanged for production.
- Generate or verify models against `/swagger/v1/swagger.json`.
- Send enum names, never their numeric values.
- Treat all HTTP 204 responses as success without decoding JSON.
- Use one single-flight refresh on HTTP 401, then retry the original request once.
- Never refresh an approval-related HTTP 403.
- Use server-computed money, overtime, status, totals, and snapshot fields.
- Render UTC timestamps in the nursery timezone; keep date-only values unchanged.
- Use multipart key `file` for photos and server `scanCode` for QR/barcodes.
- Do not call deprecated `/children/{id}/active`; use `/status`.
- Do not expose admin routes to Parent users.
- Read Problem Details `code`, then `errors`, `detail`, and `title`.

The plan contract now includes `category`, `billingCycle`, `daysPerCycle`,
`isFullDay`, `badgeText`, `isFeatured`, `isActive`, `currency`, `displayOrder`,
and a `price` alias for `monthlyFee`. Deleting a plan retires it (`isActive=false`)
instead of deleting historical data.

This is the authoritative handoff for connecting the admin and parent Flutter apps to the Nursery Management System backend.

## Environment

- API host: `https://nursery-management-api.runasp.net`
- Health check: `GET /health`
- Backend: `https://github.com/kiro314159265359/NurseryManagementSystem`
- Admin frontend: `https://github.com/soutAhmedTayseer/Nursery-Management-System` (`develop` is the newest source branch)
- Parent frontend: `https://github.com/soutAhmedTayseer/Nursery-Parents-System`

Use either the host plus `/api/...` paths, or a base URL ending in `/api` plus paths without `/api`. Never produce `/api/api/...`.

## Authentication

Login is `POST /api/auth/login`:

```json
{
  "userName": "parent@example.com",
  "password": "StrongPassword123!"
}
```

For parents, the username is always the email of the selected account owner:

- `accountOwner: "Mother"` uses `child.mother.email`.
- `accountOwner: "Father"` uses `child.father.email`.

Self-registration does not return tokens. A pending parent receives HTTP 403 when attempting login. After an admin approves the first child, normal login succeeds.

Send authenticated requests as `Authorization: Bearer <accessToken>`.

- Refresh: `POST /api/auth/refresh` with `{ "refreshToken": "..." }`
- Revoke/sign out: `POST /api/auth/revoke` with `{ "refreshToken": "..." }`
- Access token lifetime: 60 minutes
- Refresh token lifetime: 7 days; refresh rotates both tokens, so store both replacements.

## Five-step registration payload

Submit all five screens once from the Agreement screen. Do not create the mother, father, or emergency contact with separate calls.

```json
{
  "registration": {
    "accountOwner": "Mother",
    "password": "StrongPassword123!",
    "child": {
      "fullName": "Child Full Name",
      "dateOfBirth": "2021-05-10",
      "enrollmentDate": "2026-08-30",
      "nationality": "Egyptian",
      "religion": "Christian",
      "homeAddress": "Home address",
      "allergies": "Peanuts",
      "requestedPlanId": null,
      "mother": {
        "phone": "+201001234567",
        "email": "mother@example.com",
        "occupation": "Engineer",
        "jobTitle": "Software Engineer",
        "companyName": "Example Company",
        "workPhone": "+20212345678",
        "address": "Work address",
        "fullName": "Mother Full Name"
      },
      "father": {
        "phone": "+201009876543",
        "email": "father@example.com",
        "occupation": "Accountant",
        "jobTitle": "Senior Accountant",
        "companyName": "Example Company",
        "workPhone": "+20287654321",
        "address": "Work address",
        "fullName": "Father Full Name"
      },
      "agreement": {
        "mediaPermission": true,
        "parentSignature": "Parent Full Name",
        "signedDate": "2026-08-30",
        "acceptedTerms": true
      },
      "emergencyContacts": [
        {
          "name": "Emergency Contact Name",
          "relationship": "Grandparent",
          "phone": "+201112345678"
        }
      ]
    }
  }
}
```

`requestedPlanId` may be `null`. Load selectable plans with `GET /api/plans`. Passwords require at least eight characters.

## Registration flows

### Admin creates parent and child

`POST /api/registrations/admin` with an admin bearer token and the complete payload above.

The parent and child are immediately `Approved`. The parent can log in immediately. Put password and confirm-password controls on the selected mother or father screen, but send only `registration.password`.

### Parent creates their first account and child

`POST /api/registrations/self` is public and uses the same complete payload.

The HTTP 201 response is:

```json
{
  "parentUserId": "GUID",
  "childId": "GUID",
  "approvalStatus": "Pending"
}
```

Show a “Waiting for nursery approval” screen. Do not attempt to log the parent in automatically.

### Approved parent adds another child

`POST /api/registrations/children` requires a Parent token. Its body is `{ "child": { ... } }`, using exactly the same `child` object above but omitting `registration`, `accountOwner`, and `password`.

Every additional child starts as `Pending`, even though the parent account is already approved.

### Status and admin review

- Parent status list: `GET /api/registrations/mine` (Parent)
- Admin pending list: `GET /api/registrations/pending` (SuperAdmin or SubAdmin)
- Approve: `PUT /api/registrations/{childId}/approve` with no body
- Reject: `PUT /api/registrations/{childId}/reject` with `{ "reason": "Reason shown to parent" }`

Approving the first child also approves the parent account. Approving a later child affects that child only. Rejected children remain visible in `/mine` with `rejectionReason` but are excluded from normal operational child lists.

## Current endpoint map

| Area | Route | Access |
|---|---|---|
| Health | `GET /health` | Public |
| Auth | `POST /api/auth/login`, `/refresh` | Public |
| Auth | `POST /api/auth/revoke` | Signed in |
| Registration | `POST /api/registrations/self` | Public |
| Registration | `POST /api/registrations/admin` | Admin roles |
| Registration | `POST /api/registrations/children` | Parent |
| Registration | `GET /api/registrations/mine` | Parent |
| Registration | `GET /api/registrations/pending` | Admin roles |
| Registration | `PUT /api/registrations/{childId}/approve` | Admin roles |
| Registration | `PUT /api/registrations/{childId}/reject` | Admin roles |
| Children | `/api/children...` | Admin roles |
| Attendance | `/api/attendance...` | Admin roles |
| Plans read | `GET /api/plans`, `GET /api/plans/{id}` | Any signed-in user |
| Plan management | `POST`, `PUT`, `DELETE /api/plans...` | SuperAdmin |
| Assignments | `/api/planassignments...` | Admin roles |
| Billing | `/api/billing...` | Admin roles |
| Schedule | `/api/schedule...` | Admin roles |
| Users | `/api/users...` | SuperAdmin |
| Audit | `/api/sessionlogs...` | SuperAdmin |

Do not give Parent tokens access to admin children, attendance, billing, assignments, or schedule endpoints. Parent-specific read dashboards can be added separately; never expose unscoped admin lists to the Parent role.

## Integration rules

- JSON uses camelCase; enum values are strings exactly as shown.
- IDs are GUIDs. Dates use `YYYY-MM-DD`.
- HTTP 204 has no body.
- Errors use ASP.NET Problem Details: read `status`, `title`, `detail`, and optional `errors`.
- Store tokens in secure storage and never log passwords or tokens.
- On HTTP 401, perform one single-flight refresh and retry once. Do not refresh on the pending-account HTTP 403 response.
- Keep the selected account owner in form state. Show password fields on that selected parent’s step.
- Submit once at step 5, then clear sensitive form state.
