using HealthAppointmentSystem.Enums;
using HealthAppointmentSystem.Models;

namespace HealthAppointmentSystem.Repositories
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<Appointment?> GetByIdAsync(Guid id);
        Task<IEnumerable<Appointment>> GetByDoctorIdAsync(Guid doctorId);
        Task<IEnumerable<Appointment>> GetByPatientIdAsync(Guid patientId);
        Task<bool> AddAsync(Appointment appointment);
        Task<bool> UpdateStatusAsync(Guid id, AppointmentStatus status, Guid updatedBy);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> IsDoctorAvailableAsync(Guid doctorId, DateTime dateTime);
    }
}
