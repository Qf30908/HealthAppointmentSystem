using HealthAppointmentSystem.Models;

namespace HealthAppointmentSystem.Repositories
{
    public interface IMedicalRecordRepository
    {
        Task<IEnumerable<MedicalRecord>> GetAllAsync();

        Task<MedicalRecord> GetByIdAsync(Guid id);

        Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(Guid patientId);

        Task<bool> AddAsync(MedicalRecord record);

        Task<bool> UpdateAsync(MedicalRecord record);

        Task<bool> DeleteAsync(Guid id);
    }
}