using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace PosHC.Infrastructure.Persistence;

public static class SchemaUpgrade
{
    public static async Task Apply(ApplicationDbContext db, CancellationToken ct = default)
    {
        var fresh = await db.Database.EnsureCreatedAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlRawAsync("EXEC sp_getapplock @Resource='PosHC.SchemaUpgrade', @LockMode='Exclusive', @LockOwner='Transaction'; IF OBJECT_ID('poshc.SchemaVersion') IS NULL CREATE TABLE poshc.SchemaVersion (Version int NOT NULL PRIMARY KEY, AppliedAt datetime2 NOT NULL);", ct);
        var applied = await db.Database.SqlQueryRaw<int>("SELECT Version AS Value FROM poshc.SchemaVersion WHERE Version=1").ToListAsync(ct);
        if (applied.Count == 0)
        {
            if (fresh)
            {
                await db.Database.ExecuteSqlRawAsync("CREATE SEQUENCE poshc.InvoiceNumber AS bigint START WITH 1001 INCREMENT BY 1", ct);
                db.ClinicSettings.Add(new PosHC.Domain.Entities.ClinicSettings());
                foreach (var name in new[] { "Cash", "Card", "Transfer", "On account" })
                {
                    db.PaymentType.Add(new PosHC.Domain.Entities.PaymentType { Name = name });
                }

                await db.SaveChangesAsync(ct);
            }
            else
            {
                var assembly = typeof(SchemaUpgrade).Assembly;
                var resource = assembly.GetManifestResourceNames().Single(x => x.EndsWith("001_clinic_operations.sql"));
                using var reader = new StreamReader(assembly.GetManifestResourceStream(resource)!);
                foreach (var batch in Regex.Split(await reader.ReadToEndAsync(ct), @"^GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(batch))
                    {
                        await db.Database.ExecuteSqlRawAsync(batch, ct);
                    }
                }
            }
            await db.Database.ExecuteSqlRawAsync("INSERT INTO poshc.SchemaVersion VALUES (1,SYSUTCDATETIME())", ct);
        }
        await tx.CommitAsync(ct);
    }
}
