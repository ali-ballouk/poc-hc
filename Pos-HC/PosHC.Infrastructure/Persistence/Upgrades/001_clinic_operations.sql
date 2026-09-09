-- Transactional upgrade preserving existing business records.
IF COL_LENGTH('poshc.Patient', 'Phone') IS NULL ALTER TABLE poshc.[Patient] ADD [Phone] nvarchar(50) NOT NULL DEFAULT '';
GO
IF COL_LENGTH('poshc.Patient', 'Email') IS NULL ALTER TABLE poshc.[Patient] ADD [Email] nvarchar(200) NOT NULL DEFAULT '';
GO
IF COL_LENGTH('poshc.Patient', 'Address') IS NULL ALTER TABLE poshc.[Patient] ADD [Address] nvarchar(500) NOT NULL DEFAULT '';
GO
IF COL_LENGTH('poshc.Patient', 'DateOfBirth') IS NULL ALTER TABLE poshc.[Patient] ADD [DateOfBirth] datetime2 NULL;
GO
IF COL_LENGTH('poshc.Patient', 'IsActive') IS NULL ALTER TABLE poshc.[Patient] ADD [IsActive] bit NOT NULL DEFAULT 1;
GO
IF COL_LENGTH('poshc.Doctor', 'Phone') IS NULL ALTER TABLE poshc.[Doctor] ADD [Phone] nvarchar(50) NOT NULL DEFAULT '';
GO
IF COL_LENGTH('poshc.Doctor', 'Specialty') IS NULL ALTER TABLE poshc.[Doctor] ADD [Specialty] nvarchar(150) NOT NULL DEFAULT '';
GO
IF COL_LENGTH('poshc.Doctor', 'IsActive') IS NULL ALTER TABLE poshc.[Doctor] ADD [IsActive] bit NOT NULL DEFAULT 1;
GO
IF COL_LENGTH('poshc.CatalogItem', 'IsActive') IS NULL ALTER TABLE poshc.[CatalogItem] ADD [IsActive] bit NOT NULL DEFAULT 1;
GO
IF COL_LENGTH('poshc.Invoice', 'Number') IS NULL ALTER TABLE poshc.[Invoice] ADD [Number] bigint NULL;
GO
IF COL_LENGTH('poshc.Invoice', 'Status') IS NULL ALTER TABLE poshc.[Invoice] ADD [Status] nvarchar(30) NOT NULL DEFAULT 'Issued';
GO
IF COL_LENGTH('poshc.Invoice', 'Currency') IS NULL ALTER TABLE poshc.[Invoice] ADD [Currency] nvarchar(3) NOT NULL DEFAULT 'USD';
GO
IF COL_LENGTH('poshc.Invoice', 'ExchangeRate') IS NULL ALTER TABLE poshc.[Invoice] ADD [ExchangeRate] decimal(20,4) NOT NULL DEFAULT 1;
GO
IF COL_LENGTH('poshc.Invoice', 'TaxRate') IS NULL ALTER TABLE poshc.[Invoice] ADD [TaxRate] decimal(20,4) NOT NULL DEFAULT 0;
GO
IF COL_LENGTH('poshc.Invoice', 'PatientName') IS NULL ALTER TABLE poshc.[Invoice] ADD [PatientName] nvarchar(300) NOT NULL DEFAULT '';
GO
IF COL_LENGTH('poshc.Invoice', 'DoctorName') IS NULL ALTER TABLE poshc.[Invoice] ADD [DoctorName] nvarchar(300) NOT NULL DEFAULT '';
GO
IF COL_LENGTH('poshc.Invoice', 'ClinicName') IS NULL ALTER TABLE poshc.[Invoice] ADD [ClinicName] nvarchar(150) NOT NULL DEFAULT 'POS HC';
GO
IF COL_LENGTH('poshc.Invoice', 'ClinicAddress') IS NULL ALTER TABLE poshc.[Invoice] ADD [ClinicAddress] nvarchar(500) NOT NULL DEFAULT '';
GO
IF COL_LENGTH('poshc.Invoice', 'ClinicPhone') IS NULL ALTER TABLE poshc.[Invoice] ADD [ClinicPhone] nvarchar(50) NOT NULL DEFAULT '';
GO
IF COL_LENGTH('poshc.Invoice', 'RequestId') IS NULL ALTER TABLE poshc.[Invoice] ADD [RequestId] uniqueidentifier NULL;
GO
IF COL_LENGTH('poshc.InvoiceItem', 'Description') IS NULL ALTER TABLE poshc.[InvoiceItem] ADD [Description] nvarchar(300) NOT NULL DEFAULT '';
GO
IF COL_LENGTH('poshc.Payment', 'Amount') IS NULL ALTER TABLE poshc.[Payment] ADD [Amount] decimal(20,4) NULL;
GO
IF COL_LENGTH('poshc.Payment', 'Currency') IS NULL ALTER TABLE poshc.[Payment] ADD [Currency] nvarchar(3) NOT NULL DEFAULT 'USD';
GO
IF COL_LENGTH('poshc.Payment', 'Kind') IS NULL ALTER TABLE poshc.[Payment] ADD [Kind] nvarchar(30) NOT NULL DEFAULT 'Payment';
GO
IF COL_LENGTH('poshc.Payment', 'Reference') IS NULL ALTER TABLE poshc.[Payment] ADD [Reference] nvarchar(500) NOT NULL DEFAULT '';
GO
IF COL_LENGTH('poshc.Payment', 'RequestId') IS NULL ALTER TABLE poshc.[Payment] ADD [RequestId] uniqueidentifier NULL;
GO
IF COL_LENGTH('poshc.Payment', 'CashShiftId') IS NULL ALTER TABLE poshc.[Payment] ADD [CashShiftId] uniqueidentifier NULL;
GO

CREATE SEQUENCE poshc.InvoiceNumber AS bigint START WITH 1001 INCREMENT BY 1;
GO
UPDATE poshc.Invoice SET Number = NEXT VALUE FOR poshc.InvoiceNumber WHERE Number IS NULL;
ALTER TABLE poshc.Invoice ALTER COLUMN Number bigint NOT NULL;
UPDATE i SET PatientName=p.FirstName+' '+p.LastName,DoctorName=d.FirstName+' '+d.LastName
FROM poshc.Invoice i JOIN poshc.Patient p ON p.Id=i.PatientId JOIN poshc.Doctor d ON d.Id=i.DoctorId WHERE i.PatientName='';
UPDATE i SET Description=c.Name FROM poshc.InvoiceItem i JOIN poshc.CatalogItem c ON c.Id=i.CatalogItemId WHERE i.Description='';
UPDATE p SET Amount=CASE WHEN p.PaymentTypeId=4 THEN 0 ELSE i.DoctorFee-COALESCE(i.Discount,0)+COALESCE((SELECT SUM(ii.Quantity*ii.UnitPrice) FROM poshc.InvoiceItem ii WHERE ii.InvoiceId=i.Id),0) END,
Kind=CASE WHEN p.PaymentTypeId=4 THEN 'Deferred' ELSE 'Payment' END FROM poshc.Payment p JOIN poshc.Invoice i ON p.InvoiceId=i.Id WHERE p.Amount IS NULL;
ALTER TABLE poshc.Payment ALTER COLUMN Amount decimal(20,4) NOT NULL;
CREATE UNIQUE INDEX IX_Invoice_Number ON poshc.Invoice(Number);
CREATE UNIQUE INDEX IX_Invoice_RequestId ON poshc.Invoice(RequestId) WHERE RequestId IS NOT NULL;
CREATE UNIQUE INDEX IX_Payment_RequestId ON poshc.Payment(RequestId) WHERE RequestId IS NOT NULL;
GO
CREATE TABLE poshc.StaffUser (Id uniqueidentifier NOT NULL PRIMARY KEY,Username nvarchar(100) NOT NULL,DisplayName nvarchar(150) NOT NULL,PasswordHash nvarchar(max) NOT NULL,Role nvarchar(30) NOT NULL,IsActive bit NOT NULL,SecurityStamp nvarchar(max) NOT NULL,FailedAttempts int NOT NULL,LockedUntil datetime2 NULL,ResetTokenHash nvarchar(100) NULL,ResetExpires datetime2 NULL);
CREATE UNIQUE INDEX IX_StaffUser_Username ON poshc.StaffUser(Username);
CREATE TABLE poshc.ClinicSettings (Id int NOT NULL PRIMARY KEY,Name nvarchar(150) NOT NULL,Address nvarchar(500) NOT NULL,Phone nvarchar(50) NOT NULL,TimeZone nvarchar(100) NOT NULL,LbpPerUsd decimal(20,4) NOT NULL,ExchangeRateConfirmed bit NOT NULL,TaxRate decimal(20,4) NOT NULL,TaxRegistrationNumber nvarchar(100) NOT NULL);
INSERT INTO poshc.ClinicSettings VALUES(1,'POS HC','','','Asia/Beirut',1,0,0,'');
CREATE TABLE poshc.AuditEntry (Id uniqueidentifier NOT NULL PRIMARY KEY,CreatedAt datetime2 NOT NULL,Actor nvarchar(150) NOT NULL,Action nvarchar(100) NOT NULL,Entity nvarchar(100) NOT NULL,EntityId nvarchar(100) NOT NULL,Details nvarchar(max) NOT NULL);
CREATE INDEX IX_AuditEntry_CreatedAt ON poshc.AuditEntry(CreatedAt);
CREATE TABLE poshc.Appointment (Id uniqueidentifier NOT NULL PRIMARY KEY,PatientId uniqueidentifier NOT NULL REFERENCES poshc.Patient(Id),DoctorId uniqueidentifier NOT NULL REFERENCES poshc.Doctor(Id),StartsAt datetime2 NOT NULL,EndsAt datetime2 NOT NULL,Status nvarchar(30) NOT NULL,Reason nvarchar(500) NOT NULL);
CREATE INDEX IX_Appointment_DoctorId_StartsAt ON poshc.Appointment(DoctorId,StartsAt);
CREATE TABLE poshc.DoctorAvailability (Id uniqueidentifier NOT NULL PRIMARY KEY,DoctorId uniqueidentifier NOT NULL REFERENCES poshc.Doctor(Id),StartsAt datetime2 NOT NULL,EndsAt datetime2 NOT NULL);
CREATE TABLE poshc.CashShift (Id uniqueidentifier NOT NULL PRIMARY KEY,UserId uniqueidentifier NOT NULL REFERENCES poshc.StaffUser(Id),Currency nvarchar(3) NOT NULL,OpenedAt datetime2 NOT NULL,ClosedAt datetime2 NULL,OpeningAmount decimal(20,4) NOT NULL,CountedAmount decimal(20,4) NULL,ExpectedAmount decimal(20,4) NULL);
CREATE UNIQUE INDEX IX_CashShift_Open ON poshc.CashShift(UserId,Currency) WHERE ClosedAt IS NULL;
CREATE TABLE poshc.CashMovement (Id uniqueidentifier NOT NULL PRIMARY KEY,CashShiftId uniqueidentifier NOT NULL REFERENCES poshc.CashShift(Id),Amount decimal(20,4) NOT NULL,Reason nvarchar(500) NOT NULL,CreatedAt datetime2 NOT NULL);
CREATE TABLE poshc.CreditNote (Id uniqueidentifier NOT NULL PRIMARY KEY,InvoiceId uniqueidentifier NOT NULL REFERENCES poshc.Invoice(Id),Amount decimal(20,4) NOT NULL,Reason nvarchar(500) NOT NULL,CreatedAt datetime2 NOT NULL,RequestId uniqueidentifier NOT NULL);
CREATE UNIQUE INDEX IX_CreditNote_RequestId ON poshc.CreditNote(RequestId);
ALTER TABLE poshc.Payment ADD CONSTRAINT FK_Payment_CashShift FOREIGN KEY(CashShiftId) REFERENCES poshc.CashShift(Id);

