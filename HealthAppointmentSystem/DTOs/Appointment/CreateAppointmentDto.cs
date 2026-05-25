using System.ComponentModel.DataAnnotations;

namespace HealthAppointmentSystem.DTOs.Appointment
{
    public class CreateAppointmentDto
    {
        [Required]
        public Guid DoctorId { get; set; }

        public Guid PatientId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}
