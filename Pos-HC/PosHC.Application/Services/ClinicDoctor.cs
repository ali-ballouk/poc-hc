using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Services;

internal static class ClinicDoctor
{
    public static Guid Resolve(ClinicSettings settings, Guid requested)
    {
        if (!settings.SingleDoctorMode) return requested;
        var doctorId = settings.DefaultDoctorId ?? throw new BusinessException("Choose an active doctor for single-doctor mode.");
        if (requested != Guid.Empty && requested != doctorId)
            throw new BusinessException("Single-doctor mode is enabled. Refresh and use the configured doctor.");
        return doctorId;
    }

    public static async Task<Guid> ResolveAsync(IClinicStore store, Guid requested, CancellationToken ct)
    {
        var settings = await store.Find<ClinicSettings>(s => s.Id == 1, ct)
            ?? throw new BusinessException("Run database migrations first.", 503);
        return Resolve(settings, requested);
    }
}
