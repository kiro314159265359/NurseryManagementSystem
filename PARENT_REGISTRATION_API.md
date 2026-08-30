# Parent Registration and Child Approval API

Parent logins are linked to child registrations. The account owner is selected with `Mother` or `Father`; the matching parent's email is the login username.

## Routes

| Method | Route | Access | Result |
|---|---|---|---|
| POST | `/api/registrations/admin` | SuperAdmin, SubAdmin | Creates an approved parent account and approved child |
| POST | `/api/registrations/self` | Public | Creates a pending parent account and pending first child |
| POST | `/api/registrations/children` | Approved Parent | Adds another pending child to the signed-in parent |
| GET | `/api/registrations/mine` | Parent | Lists that parent's approved, pending, and rejected children |
| GET | `/api/registrations/pending` | SuperAdmin, SubAdmin | Lists submissions awaiting review |
| PUT | `/api/registrations/{childId}/approve` | SuperAdmin, SubAdmin | Approves one child and, when needed, the parent account |
| PUT | `/api/registrations/{childId}/reject` | SuperAdmin, SubAdmin | Rejects one child; body is `{ "reason": "..." }` |

## New family payload

Both `/admin` and `/self` accept:

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
      "allergies": null,
      "requestedPlanId": null,
      "mother": {
        "phone": "+201001234567",
        "email": "mother@example.com",
        "occupation": "Engineer",
        "jobTitle": "Engineer",
        "companyName": "Example",
        "workPhone": "+20212345678",
        "address": "Work address",
        "fullName": "Mother Full Name"
      },
      "father": {
        "phone": "+201009876543",
        "email": "father@example.com",
        "occupation": "Accountant",
        "jobTitle": "Accountant",
        "companyName": "Example",
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
      "emergencyContacts": []
    }
  }
}
```

Set `accountOwner` to `Father` to use `father.email`, `father.fullName`, and `father.phone` for the account. An additional-child request uses `{ "child": { ... } }` without a password or account owner.

Pending parents cannot log in. Login returns HTTP 403 until the first child is approved. Each later child requires a separate approval.
