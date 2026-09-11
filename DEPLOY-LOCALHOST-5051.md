# Local HTTP deployment on port 5051

Prepared on 2026-09-10. Updated to port 5051 because port 5050 belongs to the existing AppLearing IIS site. Installed the Microsoft-signed .NET 8.0.31 Hosting Bundle and successfully activated IIS. The existing deployment directory and fresh database are retained.

## Deployment

- URL: `http://localhost:5051`
- IIS website and application pool: `PosHC-Local5051`
- Physical path: `C:\git\poc-hc\artifacts\http-localhost-5050`
- SQL Server: local default instance
- Fresh database: `PosHC_Local5050_20260910_185200`
- Runtime SQL login: `PosHC_Local5050_20260910_185200_App`

## Steps completed

1. Inspected the backend, Angular build configuration, publishing script, IIS installation, runtimes, and SQL connectivity.
2. Added the opt-in `Hosting:AllowHttp` setting in `Program.cs`. When enabled, HTTPS redirection/HSTS are skipped and authentication cookies use SameAsRequest. HTTPS remains the default for other production deployments.
3. Published the backend in Release mode into the deployment directory.
4. The original frontend dependency directory was locked. Copied frontend sources into `artifacts/http5050-build`, installed the locked dependency versions there using `npm ci`, and built Angular in production mode.
5. Copied `dist/ClientApp/browser` contents into the deployed backend's `wwwroot`.
6. Set Production environment explicitly in the deployed `web.config`.
7. Verified a unique database name did not exist, then ran the published application's `--migrate` command to create the database/schema. Existing databases were not replaced or imported.
8. Created a dedicated SQL login with data reader/writer permissions and UPDATE permission on the invoice-number sequence. Maintenance credentials were used only during initialization and were not saved in the deployment.
9. Created server-only `appsettings.Production.json` with the dedicated connection, a random first-administrator setup token, `Hosting.AllowHttp=true`, and localhost-only allowed hosts. Cleared the development connection from the published base configuration.
10. Restricted the production configuration file to the current Windows user, SYSTEM, and Administrators. The IIS installer grants the new application pool read access.
11. Queried the new database using the runtime login. Patients, doctors, staff, invoices, payments, appointments, cash records, and audit entries are empty. Only ClinicSettings (1), PaymentType (4), and SchemaVersion (1) contain required seed rows.
12. Started the combined published application temporarily on `http://127.0.0.1:15050` and verified HTTP 200 for `/`, `/health/live`, `/api/auth/session`, `/patients`, and the Angular JavaScript bundle. Session reported `SetupRequired=true` and `User=null`. Stopped this temporary process afterward.
13. The first IIS attempt found port 5050 occupied by AppLearing and stopped without modifying that site. Updated the new site to port 5051, installed the verified Microsoft .NET 8.0.31 Hosting Bundle, and created the PosHC-Local5051 IIS website and application pool.

## IIS activation completed

The following installer has already completed successfully; do not rerun it for an existing site:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\git\poc-hc\artifacts\Install-Http5051-IIS.ps1
```

The script refuses to replace an existing site/pool or a conflicting port-5051 binding. It creates a No Managed Code x64 pool, enables its user profile, grants read/execute access to the application files, and creates IPv4/IPv6 loopback HTTP bindings for localhost. No external firewall rule is needed.

Inspect `C:\git\poc-hc\artifacts\http5051-iis-install.log`, then verify:

```powershell
Invoke-WebRequest http://localhost:5051/health/live
Invoke-RestMethod http://localhost:5051/api/auth/session
```

Open the site and use `Setup.Token` from the restricted deployed `appsettings.Production.json` to create the first administrator. No user account was created during deployment. Remove the setup token after completing setup.

## Verification limits and build findings

The combined application was verified under IIS at http://localhost:5051: root page, health endpoint, Angular route, JavaScript bundle, and database-backed auth session returned HTTP 200. The server response identifies Microsoft-IIS/10.0. Session reports SetupRequired=true. Full end-to-end login was not performed, keeping the fresh database free of users. The builds succeeded with existing backend nullability warnings and Angular bundle/CommonJS warnings. npm reported 17 dependency vulnerabilities (5 moderate, 10 high, 2 critical); dependency versions were not changed in this deployment.

Keep the deployment directory in place while IIS points to it. The artifacts directory is ignored by Git and contains server-specific configuration; it is not a source-controlled backup. Do not rerun the fresh-database preparation script to restart the site—it creates another database.


