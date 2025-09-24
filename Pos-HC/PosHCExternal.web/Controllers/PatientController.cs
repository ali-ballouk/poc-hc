// Controllers/DoctorsController.cs
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.Interfaces;
namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/patient")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService service)
        {
            _patientService = service;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetAllPatientInfo(CancellationToken cancellationToken)
        {
            var result = await _patientService.GetAllPatientInfo(cancellationToken);
            return Ok(result);
        }
    }
}
