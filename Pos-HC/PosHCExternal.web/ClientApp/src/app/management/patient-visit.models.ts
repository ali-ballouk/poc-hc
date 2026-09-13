export interface PatientVisit {
  Id: string;
  Number: number;
  VisitedAt: string;
  DoctorName: string;
  Status: string;
  VisitDescription: string | null;
  Diagnosis: string | null;
  UpdatedAt: string | null;
  UpdatedBy: string | null;
}
