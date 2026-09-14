using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/patient")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IPatientVisitService _patientVisitService;

        public PatientController(IPatientService patientService, IPatientVisitService patientVisitService)
        {
            _patientService = patientService;
            _patientVisitService = patientVisitService;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetLookup(CancellationToken cancellationToken)
        {
            var result = await _patientService.GetAllPatientInfo(cancellationToken);
            return Ok(result);
        }
        [HttpGet]
        [Authorize(Roles = "Administrator,Receptionist,Doctor")]
        public async Task<IActionResult> GetPage(string search = "", int page = 1, CancellationToken cancellationToken = default, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        {
            var result = await _patientService.GetPageAsync(search, page, cancellationToken, pageSize, sortBy, sortDirection);
            return Ok(result);
        }
        [HttpPost]
        [Authorize(Roles = "Administrator,Receptionist")]
        public async Task<IActionResult> Create(PatientDetailsDto input, CancellationToken cancellationToken)
        {
            var result = await _patientService.SaveAsync(Guid.Empty, input, cancellationToken);
            return Ok(result);
        }
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Administrator,Receptionist")]
        public async Task<IActionResult> Update(Guid id, PatientDetailsDto input, CancellationToken cancellationToken)
        {
            var result = await _patientService.SaveAsync(id, input, cancellationToken);
            return Ok(result);
        }
        [HttpGet("{patientId:guid}/visits")]
        [Authorize(Roles = "Administrator,Receptionist,Doctor")]
        public async Task<IActionResult> GetVisits(Guid patientId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default, string? sortBy = null, string? sortDirection = null)
        {
            var result = await _patientVisitService.GetPageAsync(patientId, page, pageSize, cancellationToken, sortBy, sortDirection);
            return Ok(result);
        }
        [HttpPut("{patientId:guid}/visits/{visitId:guid}")]
        [Authorize(Roles = "Administrator,Receptionist,Doctor")]
        public async Task<IActionResult> UpdateVisit(Guid patientId, Guid visitId, VisitNotesInput input, CancellationToken cancellationToken)
        {
            var result = await _patientVisitService.UpdateAsync(patientId, visitId, input, cancellationToken);
            return Ok(result);
        }

    }
}
