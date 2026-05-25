using System.ComponentModel.DataAnnotations;

namespace HealthAppointmentSystem.DTOs.MedicalRecord
{
    public class CreateMedicalRecordDto
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        public string Prescription { get; set; } = string.Empty;

        public string DoctorNotes { get; set; } = string.Empty;
    }
}
