using HealthAppointmentSystem.Data;
using HealthAppointmentSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthAppointmentSystem.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly AppDbContext _context;

        public PatientRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(Patient patient)
        {
            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
                return false;

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            return await _context.Patients
                .Include(p => p.Appointments) 
                .ToListAsync();
        }

        public async Task<Patient> GetByIdAsync(Guid id)
        {
            return await _context.Patients
                .Include(p => p.Appointments)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> UpdateAsync(Guid id, Patient patient)
        {
            var existingPatient = await _context.Patients.FindAsync(id);

            if (existingPatient == null)
                return await Task.FromResult(false);

            existingPatient.FullName = patient.FullName;
            existingPatient.Email = patient.Email;
            existingPatient.PhoneNumber = patient.PhoneNumber;
            existingPatient.DateOfBirth = patient.DateOfBirth;

            existingPatient.UpdatedBy = patient.UpdatedBy;
            existingPatient.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await Task.FromResult(true);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}