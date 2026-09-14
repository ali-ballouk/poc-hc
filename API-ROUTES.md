# ClinicSol API routes

Controllers now own all operations for their resource. Angular has been updated to these routes. The former management and billing routes are removed; external clients must update as well.

| Operation | Method and canonical route | Previous route |
| --- | --- | --- |
| Doctor lookup | GET `/api/doctor/lookup` | Unchanged |
| Doctor list/create/update | GET/POST `/api/doctor`, PUT `/api/doctor/{id}` | `/api/clinic/doctors` |
| Doctor availability | GET/POST `/api/doctor/availability` | `/api/clinic/availability` |
| Patient lookup | GET `/api/patient/lookup` | Unchanged |
| Patient list/create/update | GET/POST `/api/patient`, PUT `/api/patient/{id}` | `/api/clinic/patients` |
| Patient visit history | GET `/api/patient/{patientId}/visits` | `/api/clinic/patients/{patientId}/visits` |
| Update visit notes | PUT `/api/patient/{patientId}/visits/{visitId}` | `/api/clinic/patients/{patientId}/visits/{visitId}` |
| Catalog lookup | GET `/api/catalogitem/lookup` | `/api/catalogtitem` |
| Catalog list/create/update | GET/POST `/api/catalogitem`, PUT `/api/catalogitem/{id}` | `/api/clinic/catalog` |
| Paged invoice list | GET `/api/invoice` | `/api/billing/invoices` |
| Legacy invoice list response | GET `/api/invoice/lookup` | GET `/api/invoice` |
| Create invoice | POST `/api/invoice` | Unchanged |
| Invoice history and balance | GET `/api/invoice/{id}` | `/api/billing/invoices/{id}` |
| Change invoice status | POST `/api/invoice/{id}/status` | `/api/billing/invoices/{id}/status` |
| Credit note | POST `/api/invoice/{id}/credits` | `/api/billing/invoices/{id}/credits` |
| Invoice PDF | GET `/api/invoice/{id}/print?language=en` | Unchanged |
| Collect cash | POST `/api/payment` | `/api/billing/payments` and `/api/payment` |
| Refund cash for an invoice | POST `/api/payment/invoice/{invoiceId}/refunds` | `/api/billing/invoices/{id}/refunds` |
| Receipt PDF | GET `/api/payment/{id}/receipt?language=ar` | `/api/billing/payments/{id}/receipt` |
| Payment method lookup | GET `/api/paymenttype/lookup` | Unchanged |
| Appointments | GET/POST `/api/appointments`, PUT `/api/appointments/{id}` | `/api/clinic/appointments` |
| Staff | GET/POST `/api/staff`, PUT `/api/staff/{id}`, POST `/api/staff/{id}/reset` | `/api/clinic/staff` |
| Cash shifts | GET/POST `/api/shifts`, POST `/api/shifts/{id}/close` | `/api/clinic/shifts` |
| Cash movements | GET/POST `/api/shifts/{id}/movements` | `/api/clinic/shifts/{id}/movements` |
| Audit | GET `/api/audit` | `/api/clinic/audit` |
| Clinic settings | GET/PUT `/api/clinic/settings` | Unchanged |
| Authentication and sessions | `/api/auth/*` | Unchanged |
| Reports and export | `/api/reports`, `/api/reports/export` | Unchanged |

Paged responses retain `Items`, `Total`, `PageSize` and `Page`, with the existing `search`, `page`, `pageSize`, `sortBy` and `sortDirection` parameters. Invoice listing also accepts `patientId`. Doctor and patient lookup response shapes remain unchanged. The payment POST response includes `Id`, `InvoiceId`, `PaymentDate` and `PaymentTypeId`, plus the payment amount, currency and reference.

Existing role restrictions, cookie authentication and CSRF requirements are preserved. Removed or unknown API routes return HTTP 404 instead of the Angular page.

Deploy backend assemblies and the Angular build together, then restart the application. This change does not reset clinic data or require schema migration. `Deploy.cmd` creates a new instance; it is not an updater for an existing instance.
