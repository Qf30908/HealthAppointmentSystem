using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAppointmentSystem.Models
{
    public class MedicalRecord
    {
        public Guid Id { get; set; }
        public string Diagnosis { get; set; }
        public string Prescription { get; set; }
        public string DoctorNotes { get; set; }

        public Guid PatientId { get; set; }
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

