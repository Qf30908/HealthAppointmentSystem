namespace HealthAppointmentSystem.DTOs.MedicalRecord
{
    public class MedicalRecordDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Prescription { get; set; } = string.Empty;
        public string DoctorNotes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
