# Single-doctor mode and patient visits

In **Clinic settings**, enable **Single-doctor mode**, choose an active **Clinic
doctor**, and save. New visits, appointments and availability use that doctor.
Turning the setting off restores doctor selection. Existing records retain their
original doctor. Change the setting before deactivating its configured doctor.

**Create new visit** includes optional **Visit description** (up to 4000
characters) and **Diagnosis** (up to 2000 characters). Saving a visit creates its
invoice and notes together. Both fields may be blank; repeated submissions with
the same request ID reuse the original visit.

Open **Patients → Visit history**, or use **Patient visit history** after choosing
a patient in the visit form. History shows saved invoice-backed visits newest
first with backend paging. Draft and void records are labelled by status.
Appointments alone are scheduling records and do not create a saved visit.
**View / edit notes** opens a separate dialog. Notes may be edited or cleared
without changing invoice amounts; the last editor and time are retained and edits
are audited. Administrators, receptionists and doctors can access visit notes.
Clinical notes are available through patient history and are omitted from
financial invoice responses, PDFs and the existing JSON export.

## Database upgrade

The existing `--migrate` maintenance command runs `SchemaUpgrade.Apply` and applies
version 2 from `002_single_doctor_visit_notes.sql`. It adds optional notes and doctor
settings without rewriting existing invoices. Older visits appear in history with
blank notes. The command verifies a database backup before upgrading an existing
database. Normal API startup does not apply this upgrade automatically.

For the workspace's configured database, run from the repository root:

```powershell
dotnet run --project Pos-HC/PosHCExternal.web/PosHCExternal.web.csproj -c Release --no-launch-profile -- --migrate
```

Then rebuild/restart the API and refresh the frontend. A deployed environment must
use its own configuration and maintenance credentials with the same `--migrate`
argument. Stop a running Debug session before rebuilding Debug if it holds the old
assemblies open. The migration was validated on isolated test databases; it has not
been applied to the running clinic database by this change.

Validation uses Angular tests plus .NET integration tests against an isolated
temporary SQL database. `ClinicFeatureTests` covers doctor assignment, optional
notes, history paging, and repeatable upgrades from the previous schema.
