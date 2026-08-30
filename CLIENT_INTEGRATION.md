# Unified admin and parent API

The API at `https://nursery-management-api.runasp.net` is the single backend
for both Flutter clients:

- Admin: `soutAhmedTayseer/Nursery-Management-System`
- Parent/mobile: `soutAhmedTayseer/Nursery-Parents-System`

## Review findings

The admin client currently uses fake/in-memory repositories for authentication,
sessions, attendance, plans, assignments, finance, and schedule data. It also
references a missing sibling package at `../packages/nursery_shared`, so a fresh
clone is not self-contained. Its password-change, historical-attendance,
dashboard-summary, and enrollment-approval flows were not fully backed by API
contracts.

The parent client currently contains presentation-only demo data and has no HTTP
client, token storage, or API repositories.

## Authentication and account

| Method | Route | Role | Purpose |
| --- | --- | --- | --- |
| POST | `/api/auth/register-parent` | Anonymous | Create a parent account |
| POST | `/api/auth/login` | Anonymous | Sign in (email is the parent username) |
| POST | `/api/auth/refresh` | Anonymous | Rotate an access/refresh token pair |
| POST | `/api/auth/revoke` | Any signed-in user | Sign out/revoke a refresh token |
| GET | `/api/account/me` | Any signed-in user | Load the current profile |
| PUT | `/api/account/me` | Any signed-in user | Update name and phone |
| PUT | `/api/account/password` | Any signed-in user | Change own password |

## Parent/mobile

| Method | Route | Purpose |
| --- | --- | --- |
| GET | `/api/parent/children` | List the signed-in parent's children and approval states |
| POST | `/api/parent/children` | Submit a child enrollment for admin approval |
| GET | `/api/parent/children/{childId}/dashboard` | Home data: live attendance, plan, balance, and schedule |
| GET | `/api/parent/children/{childId}/attendance` | Paginated attendance/history |
| GET | `/api/parent/children/{childId}/invoices` | Paginated billing history |
| POST | `/api/parent/children/{childId}/plans/{planId}` | Select or change a plan |
| GET | `/api/plans` | Read available plans |

Every parent route verifies the child-parent link server-side. Parent tokens
cannot call the admin children, attendance, billing, schedule, or assignment
routes.

## Admin additions

| Method | Route | Purpose |
| --- | --- | --- |
| GET | `/api/adminDashboard` | Dashboard counts, attendance, revenue, and balances |
| GET | `/api/adminDashboard/pending-enrollments` | Review parent enrollment submissions |
| PUT | `/api/adminDashboard/pending-enrollments/{childId}/approve` | Approve enrollment |

Existing admin routes remain available for children, attendance, plans,
assignments, invoices, schedules, staff accounts, and audit/session logs.

## Client integration order

1. Add an HTTP client with bearer-token and refresh-token handling.
2. Replace the fake authentication repositories.
3. Replace session/attendance repositories.
4. Connect plans, assignments, finance, schedule, and dashboard.
5. Connect parent registration, enrollment, home, history, billing, and profile.

No database, JWT, admin, or deployment credentials belong in either Flutter
repository. The production API URL is non-secret and may be configured at build
time.
