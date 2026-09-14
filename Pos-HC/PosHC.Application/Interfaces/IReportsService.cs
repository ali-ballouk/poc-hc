using PosHC.Application.DTOs;

namespace PosHC.Application.Interfaces
{
    public interface IReportsService
    {
        Task<ClinicReportDto> SummaryAsync(DateTime from, DateTime to, CancellationToken cancellationToken);
        Task<ClinicExportDto> ExportAsync(CancellationToken cancellationToken);
    }
}
