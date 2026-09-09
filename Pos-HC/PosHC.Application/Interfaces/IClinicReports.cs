namespace PosHC.Application.Interfaces;
public interface IClinicReports
{
    Task<object> Summary(DateTime from, DateTime to, CancellationToken ct);
    Task<object> Export(CancellationToken ct);
}
