IF COL_LENGTH('poshc.ClinicSettings', 'SingleDoctorMode') IS NULL
    ALTER TABLE poshc.ClinicSettings ADD SingleDoctorMode bit NOT NULL CONSTRAINT DF_ClinicSettings_SingleDoctorMode DEFAULT 0;
IF COL_LENGTH('poshc.ClinicSettings', 'DefaultDoctorId') IS NULL
    ALTER TABLE poshc.ClinicSettings ADD DefaultDoctorId uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ClinicSettings_Doctor_DefaultDoctorId' AND parent_object_id = OBJECT_ID('poshc.ClinicSettings'))
    ALTER TABLE poshc.ClinicSettings ADD CONSTRAINT FK_ClinicSettings_Doctor_DefaultDoctorId FOREIGN KEY (DefaultDoctorId) REFERENCES poshc.Doctor(Id);
IF COL_LENGTH('poshc.Invoice', 'VisitDescription') IS NULL
    ALTER TABLE poshc.Invoice ADD VisitDescription nvarchar(4000) NULL;
IF COL_LENGTH('poshc.Invoice', 'Diagnosis') IS NULL
    ALTER TABLE poshc.Invoice ADD Diagnosis nvarchar(2000) NULL;
IF COL_LENGTH('poshc.Invoice', 'VisitNotesUpdatedAt') IS NULL
    ALTER TABLE poshc.Invoice ADD VisitNotesUpdatedAt datetime2 NULL;
IF COL_LENGTH('poshc.Invoice', 'VisitNotesUpdatedBy') IS NULL
    ALTER TABLE poshc.Invoice ADD VisitNotesUpdatedBy nvarchar(max) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Invoice_PatientId_CreatedAt_Id' AND object_id = OBJECT_ID('poshc.Invoice'))
    CREATE INDEX IX_Invoice_PatientId_CreatedAt_Id ON poshc.Invoice(PatientId, CreatedAt, Id);
