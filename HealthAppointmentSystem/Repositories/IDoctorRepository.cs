using HealthAppointmentSystem.Models;

namespace HealthAppointmentSystem.Repositories
{
    public interface IDoctorRepository
    {
        Task<IEnumerable<Doctor>> GetAllAsync();
        
        // Search: Pagination, Filtering, Sorting, Search

        Task<Doctor> GetByIdAsync(Guid id);
        Task<bool> AddAsync(Doctor doctor);
        Task<bool> UpdateAsync(Guid id, Doctor doctor);
        Task<bool> DeleteAsync(Guid id);

        Task SaveChanges();
    }
}
