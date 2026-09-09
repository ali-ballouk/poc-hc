using Microsoft.EntityFrameworkCore;

namespace PosHC.Infrastructure.Persistence;

public static class DatabaseMaintenance
{
    // Paths refer to the SQL Server machine. The SQL service account needs write access.
    public static async Task<string> Backup(ApplicationDbContext db)
    {
        await db.Database.OpenConnectionAsync();
        var connection = db.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000))";
        var directory = (string?)await command.ExecuteScalarAsync();
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException("Configure the SQL Server default backup directory first.");
        }

        var separator = directory.Contains('\\') ? "\\" : "/";
        var path = directory.TrimEnd('\\', '/') + separator + "PosHC-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-ffff") + ".bak";
        var database = "[" + connection.Database.Replace("]", "]]") + "]";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@path";
        parameter.Value = path;
        command.Parameters.Add(parameter);
        command.CommandTimeout = 600;
        command.CommandText = $"BACKUP DATABASE {database} TO DISK=@path WITH COPY_ONLY, CHECKSUM";
        await command.ExecuteNonQueryAsync();
        command.CommandText = "RESTORE VERIFYONLY FROM DISK=@path WITH CHECKSUM";
        await command.ExecuteNonQueryAsync();
        return path;
    }
}
