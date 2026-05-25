using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAppointmentSystem.Models
{
    public class MedicalRecord
    {
        public Guid Id { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Prescription { get; set; } = string.Empty;
        public string DoctorNotes { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
