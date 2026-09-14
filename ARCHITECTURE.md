# Coding conventions

Keep code readable: use descriptive names, one statement per line, and short methods with one responsibility. Format C# with `dotnet format` and Angular source with `npm run format`.

Every production Angular component has its own HTML file and uses `templateUrl`. The Angular component generator is configured to create external templates. Inline templates are reserved for small test fixtures.

## Dependency direction

`Domain` contains entities and their calculations. It has no references to Application, ASP.NET Core, Entity Framework, PDF libraries, or JSON serialization.

`Application` references Domain. It owns workflows, validation, DTOs, explicit mapping, and the interfaces for persistence, reports, current staff, and document rendering. Services use ordinary constructors and private readonly fields. Keep the existing service pattern; a mediator or a generic service framework is not required.

`Infrastructure` implements those interfaces using SQL Server, Entity Framework, password hashing, and QuestPDF. Entity Framework configuration lives here, including field lengths previously declared on domain entities. The PDF logo is an embedded Infrastructure asset.

`Web` is the HTTP adapter and composition root. Controllers handle routes, authorization, HTTP responses, cookies, and CSRF. They call application service interfaces and accept application DTOs. They do not query repositories, assemble report data, or pass tracked entities to the client. Startup may reference Infrastructure to register adapters and perform database maintenance.

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
| Invoice creation, history, status, credits and printing | `IInvoiceService` |
| Payment collection, refunds and receipt printing | `IPaymentService` |
| Shared transactional billing rules | `IBillingService` |
| Patient visit history and clinical notes | `IPatientVisitService` |
| Report validation, calculations and export auditing | `IReportsService` |

Place new operations in the service that owns the affected domain. Do not add an aggregate clinic service to route unrelated workflows. Shared validation belongs in `BusinessRules`, and shared paging belongs in `PagedQuery`.

## Controller ownership and API routes

Each resource has one controller. DoctorController includes doctor lookups, management and availability. PatientController includes patient lookups, management and visit history. CatalogItemController includes catalog lookups and management. InvoiceController owns invoice actions and PaymentController owns payment and refund actions. Authentication and reporting have their own controllers.

The redundant management and billing controllers have been removed. Angular uses the canonical routes documented in [API-ROUTES.md](API-ROUTES.md); management modules declare their endpoint instead of constructing it from the screen name. Update frontend and backend together.

## Read models and private data

Application DTOs are detached snapshots. Updating a DTO does not save an entity: use the owning service's save method. Financial invoice DTOs exclude clinical notes; patient visit DTOs expose them only through the authorized patient routes. Staff DTOs exclude password hashes, security stamps and reset tokens.

Invoice and receipt print services read the current clinic name. They preserve stored financial amounts and the original invoice snapshot. PDF adapters receive dedicated print DTOs and never load settings or database records themselves.

InvoiceReader projects invoice totals and payment balances in SQL, sorts the full result, and applies pagination before materialization. Report readers retrieve data; ReportsService validates the period and assembles the report. Keep SQL total projections covered by tests against the Domain calculations, including USD and LBP rounding.

## Verification

ArchitectureTests enforce inward dependencies, service-only controller constructors, DTO request boundaries, resource route ownership, and private-field exclusion. ArchitectureWorkflowTests exercise the HTTP workflow, SQL sorting, settings, cash payments and refunds, session invalidation, reporting and PDF downloads. Existing workflow, paging, localization and Angular tests remain part of the regression suite.

This refactor does not require a database schema change or a fresh database. Publish the rebuilt backend and Angular files together; existing clinic data and deployment configuration remain in place.
