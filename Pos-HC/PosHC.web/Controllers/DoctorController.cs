// Controllers/DoctorsController.cs
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.Interfaces;
namespace PosHC.web.Controllers
{
    [ApiController]
    [Route("api/doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService service)
        {
            _doctorService = service;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetLookup(CancellationToken cancellationToken)
        {
            var result = await _doctorService.GetAllDoctorInfo(cancellationToken);
            return Ok(result);
        }
    }
}
