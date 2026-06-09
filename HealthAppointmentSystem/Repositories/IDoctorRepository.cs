using HealthAppointmentSystem.Models;

namespace HealthAppointmentSystem.Repositories
{
    public interface IDoctorRepository
    {
        Task<IEnumerable<Doctor>> GetAllAsync();
        Task<Doctor?> GetByIdAsync(Guid id);
        Task<Doctor?> GetByUserIdAsync(Guid userId);
        Task<bool> AddAsync(Doctor doctor);
        Task<bool> UpdateAsync(Guid id, Doctor doctor);
        Task<bool> DeleteAsync(Guid id);
        Task<List<Doctor>> SearchAsync(int pageNumber, int pageSize,  string? specialty, DateTime? fromDate, DateTime? toDate, string? searchTest, string? sortField, bool? sortOrder);
        Task<int> TotalCountDoctors(string? specialty, DateTime? fromDate,DateTime? toDate, string? searchTest);
    }
}
