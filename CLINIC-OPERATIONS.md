# Clinic operations release

This release targets one clinic per installation in Lebanon. It preserves the Domain → Application → Infrastructure → Web architecture. New Angular management pages use the existing generic grid, generic selector and Bootstrap dialog shell.

## Included workflows

- First-administrator setup; login/logout; administrator, receptionist, cashier and doctor roles; administrator-issued password reset tokens; lockout and session revocation.
- Patient registration/contact details and billing history; doctor fees; services/products; deactivation instead of destructive deletion.
- USD/LBP invoices, drafts, issuing and voiding; immutable names/prices and exchange-rate snapshots; tax and discount validation; partial payments, credits and refunds.
- Cash, external card-terminal and transfer recording. On-account means leaving an outstanding balance, not collecting money. The application does not charge cards or initiate bank transfers.
- Separate USD/LBP cash shifts, cash movements, closing count and variance.
- Doctor availability, bookings, overlap prevention, check-in, in-progress/completed/cancelled/no-show states.
- Financial summaries, doctor billing, service sales, payment-method reports and CSV exports. USD and LBP totals remain separate. Current outstanding amounts are labeled separately from period activity.
- Invoice and receipt PDFs, audit history, JSON data export, verified SQL backup command, deployment script and CI regression suite.

## Installation and upgrade

Development startup now checks the schema version before serving requests. When an upgrade is pending, it creates and verifies a backup of an existing database and applies the versioned upgrades, including the single-doctor settings and visit-note columns. The development connection therefore needs backup/schema permissions. An up-to-date database does not trigger another backup. Production upgrades remain explicit through `--migrate`; rebuilding files alone does not upgrade an existing production database.

Use .NET 8 and a supported Node version from ClientApp/package.json (Node 24 recommended). Configure `ConnectionStrings__DefaultConnection` in the service environment. Do not commit production credentials. The database identity needs schema permissions only during installation/upgrade; normal operation should use a restricted identity.

Stop the API, then run from the repository root:

```powershell
dotnet build Pos-HC/PosHC.sln
dotnet run --no-build --project Pos-HC/PosHCExternal.web -- --migrate
dotnet run --no-build --project Pos-HC/PosHCExternal.web --launch-profile http
```

For an existing database, `--migrate` creates a COPY_ONLY backup in the SQL Server default backup directory and runs RESTORE VERIFYONLY before the versioned upgrade. Existing records are retained. Legacy invoices are treated as USD, matching the previous UI, and prior on-account entries become zero-value deferrals. Review imported legacy balances before customer use. New empty databases receive the current schema and payment types. Production migrations are explicit. Development startup applies pending upgrades after a verified backup.

In development, the first-start setup token is written to `Pos-HC/PosHCExternal.web/.local/setup-token.txt`. Open the frontend, enter that token and create your own administrator username and password. No default login is installed. In production, provide a random `Setup__Token` of at least 32 characters through secure configuration. Setup closes after the first user is created.

After setup: enter the clinic identity, confirm the agreed LBP per USD rate, configure an applicable tax rate if needed, add staff and master data, and open a cash shift before cash collection. Base prices are USD; invoice currency determines conversion, rounding and settlement. USD has two decimal places; LBP uses whole lira. Mixed-currency settlement of the same invoice is not included.

## Deployment and backups

`scripts/Publish-Clinic.ps1` builds both apps into `artifacts/clinic`, serving Angular and `/api` from the same origin. Set the production database connection and setup token outside source control. Host behind HTTPS with the ASP.NET Core hosting bundle/reverse proxy. Preserve ASP.NET Data Protection keys for the service account. The frontend no longer embeds localhost API URLs.

Run `dotnet PosHCExternal.web.dll --backup` from the deployment directory for a verified SQL backup. `scripts/Install-DailyBackup.ps1` registers a daily Windows task when supplied a deployment directory and service credential. This installer script is provided but is not automatically registered. Its identity needs database backup permission; SQL Server's service identity needs access to its backup directory. Monitor scheduled-task failures and copy backups to separately protected storage with a defined retention policy.

RESTORE VERIFYONLY is not a full recovery drill. Use `scripts/Restore-Drill.sql` to restore a backup into a separate database and verify login, invoices and balances before production rollout. JSON export is for portability, not a substitute for full database recovery.

`/health/live` checks the process; authenticated `/health/ready` checks database connectivity. API errors are logged through ASP.NET logging, while business mutations produce database audit entries. Configure persistent log collection and alert delivery on the deployment host.

## Verification and boundaries

`dotnet test Pos-HC/PosHC.sln` creates and removes a uniquely named `PosHC_Test_*` database. Supply `POSHC_TEST_SQL` for a test-capable SQL connection; otherwise local appsettings provide the server connection. Never point test database creation at an account lacking appropriate isolated test permissions. Angular tests run with `ng test --watch=false --browsers=ChromeHeadless`.

Clinical records, insurance, inventory purchasing/stock control and multi-tenant SaaS were explicitly deferred. Country-specific tax/invoicing policy, SMS/email delivery, external payment integration, managed hosting and an actual backup schedule require deployment/customer configuration. This is a functional core release for acceptance testing, not certification of regulatory compliance or production operations.
