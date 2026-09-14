using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.Interfaces;

namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Administrator")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [HttpGet]
        public async Task<IActionResult> Summary(DateTime from, DateTime to, CancellationToken cancellationToken)
        {
            var result = await _reportsService.SummaryAsync(from, to, cancellationToken);
            return Ok(result);
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export(CancellationToken cancellationToken)
        {
            var data = await _reportsService.ExportAsync(cancellationToken);
            return File(JsonSerializer.SerializeToUtf8Bytes(data), "application/json", $"Clinic-export-{data.ExportedAt:yyyyMMdd}.json");
        }
    }
}
