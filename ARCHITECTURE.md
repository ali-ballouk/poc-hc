# Coding conventions

Keep code readable: use descriptive names, one statement per line, and short methods with one responsibility. Format C# with `dotnet format` and Angular source with `npm run format`.

Every production Angular component has its own HTML file and uses `templateUrl`. The Angular component generator is configured to create external templates. Inline templates are reserved for small test fixtures.

## Service ownership

Controllers handle HTTP routing and authorization. Application services own validation and workflows; persistence stays behind repository interfaces.

| Responsibility | Application service |
| --- | --- |
| Doctors and doctor availability | `IDoctorService` |
| Patients | `IPatientService` |
| Services and products | `ICatalogItemService` |
| Appointments and scheduling checks | `IAppointmentService` |
| Clinic identity, currency settings and tax settings | `IClinicSettingsService` |
| Cash shifts, reconciliation and cash movements | `ICashShiftService` |
| Staff accounts, authentication and password resets | `IStaffService` |
| Audit recording and searching | `IAuditService` |
| Invoices, payments, credits and refunds | `BillingService` |

Place new operations in the service that owns the affected domain. Do not add an aggregate clinic service to route unrelated workflows. Shared validation belongs in `BusinessRules`, and shared paging belongs in `PagedQuery`.

API URLs remain compatible with existing clients, including the `/api/clinic` prefix. That prefix does not determine which service owns an operation.
