namespace HealthAppointmentSystem.DTOs.MedicalRecord
{
    public class UpdateMedicalRecordDto
    {
        public string Diagnosis { get; set; } = string.Empty;
        public string Prescription { get; set; } = string.Empty;
        public string DoctorNotes { get; set; } = string.Empty;
    }
}
