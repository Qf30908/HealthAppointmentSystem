using HealthAppointmentSystem.Enums;

namespace HealthAppointmentSystem.DTOs.Appointment
{
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; }
    }
}
