using HealthAppointmentSystem.Models;

namespace HealthAppointmentSystem.Repositories
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetAllAsync();
        Task<Patient> GetByIdAsync(Guid id);
        Task<bool> AddAsync(Patient patient);
        Task<bool> UpdateAsync(Guid id, Patient patient);
        Task<bool> DeleteAsync(Guid id);
        Task SaveChanges();
    }
}
