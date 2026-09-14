using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/clinic/settings")]
    public class ClinicSettingsController : ControllerBase
    {
        private readonly IClinicSettingsService _clinicSettingsService;

        public ClinicSettingsController(IClinicSettingsService clinicSettingsService)
        {
            _clinicSettingsService = clinicSettingsService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await _clinicSettingsService.GetAsync(cancellationToken);
            return Ok(result);
        }
        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(ClinicSettingsDetailsDto input, CancellationToken cancellationToken)
        {
            var result = await _clinicSettingsService.SaveAsync(input, cancellationToken);
            return Ok(result);
        }

    }
}
