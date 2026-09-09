-- Run in SSMS with a dedicated NEW database name, never the live clinic database.
-- Replace the backup path and logical file names from RESTORE FILELISTONLY.
-- SQL Server service identity must be able to read the backup and write the data paths.
RESTORE VERIFYONLY FROM DISK = N'C:\Backups\PosHC-backup.bak' WITH CHECKSUM;
RESTORE FILELISTONLY FROM DISK = N'C:\Backups\PosHC-backup.bak';
-- Example only: change every placeholder before running.
-- RESTORE DATABASE [PosHC_RestoreDrill]
-- FROM DISK = N'C:\Backups\PosHC-backup.bak'
-- WITH MOVE N'LogicalDataName' TO N'C:\SQLData\PosHC_RestoreDrill.mdf',
--      MOVE N'LogicalLogName' TO N'C:\SQLData\PosHC_RestoreDrill_log.ldf', RECOVERY;
-- Validate invoices, payment balances, accounts and application login against the restored copy.
-- Do not use WITH REPLACE against the live database.
