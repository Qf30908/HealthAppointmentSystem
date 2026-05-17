using HealthAppointmentSystem.Data;
using HealthAppointmentSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthAppointmentSystem.Repositories
{
    public class MedicalRecordRepository : IMedicalRecordRepository
    {
        private readonly AppDbContext _context;

        public MedicalRecordRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MedicalRecord>> GetAllAsync()
        {
            return await _context.MedicalRecords
                .Include(m => m.Patient)
                .ToListAsync();
        }

        public async Task<MedicalRecord> GetByIdAsync(Guid id)
        {
            return await _context.MedicalRecords
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(Guid patientId)
        {
            return await _context.MedicalRecords
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> AddAsync(MedicalRecord record)
        {
            record.CreatedAt = DateTime.UtcNow;

            await _context.MedicalRecords.AddAsync(record);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(MedicalRecord record)
        {
            var existing = await _context.MedicalRecords.FindAsync(record.Id);

            if (existing == null)
                return false;

            existing.Diagnosis = record.Diagnosis;
            existing.Prescription = record.Prescription;
            existing.DoctorNotes = record.DoctorNotes;

            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = record.UpdatedBy;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);

            if (record == null)
                return false;

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}