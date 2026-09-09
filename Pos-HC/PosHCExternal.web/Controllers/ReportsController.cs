using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.Interfaces;
using PosHC.Application.Services;

namespace PosHCExternal.web.Controllers;
[ApiController, Route("api/reports"), Authorize(Roles = "Administrator")]
public class ReportsController(IClinicReports reports, IAuditService auditService) : ControllerBase
{
    [HttpGet] public Task<object> Summary(DateTime from, DateTime to, CancellationToken ct) => reports.Summary(from, to, ct);
    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        await auditService.RecordAndSaveAsync("Export", "Clinic", 1, ct);
        var data = await reports.Export(ct);
        return File(JsonSerializer.SerializeToUtf8Bytes(data, new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles }), "application/json", $"Clinic-export-{DateTime.UtcNow:yyyyMMdd}.json");
    }
}
