using System.ComponentModel.DataAnnotations;

namespace PosHC.Application.DTOs;

public record PatientVisitDto(Guid Id, long Number, DateTime VisitedAt, string DoctorName,
    string Status, string? VisitDescription, string? Diagnosis, DateTime? UpdatedAt, string? UpdatedBy);

public record VisitNotesInput(
    [MaxLength(4000)] string? VisitDescription = null,
    [MaxLength(2000)] string? Diagnosis = null);
