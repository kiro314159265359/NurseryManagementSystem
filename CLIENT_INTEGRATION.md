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
| Nursery | `GET /api/nursery/settings` |
| Nursery | `PUT /api/nursery/settings` (`SuperAdmin`) |
| Dashboard | `GET /api/dashboard/summary` |
| Dashboard | `GET /api/dashboard/alerts` |
| Audit | `GET /api/audit-log` (`SuperAdmin`) |

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
