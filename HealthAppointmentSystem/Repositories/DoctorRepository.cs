using HealthAppointmentSystem.Data;
using HealthAppointmentSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthAppointmentSystem.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly AppDbContext _context;
        public DoctorRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> AddAsync(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existingDoctor = await _context.Doctors.FindAsync(id);
            if (existingDoctor == null)
            {
                return await Task.FromResult(false);
            }

            _context.Doctors.Remove(existingDoctor);
            return await Task.FromResult(true);
        }


        public async Task<IEnumerable<Doctor>> GetAllAsync()
        {
            return await _context.Doctors.ToListAsync();
        }

        public async Task<Doctor> GetByIdAsync(Guid id)
        {
            return await _context.Doctors.FindAsync(id);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Guid id, Doctor doctor)
        {
            var existingDoctor = await _context.Doctors.FindAsync(id);
            if (existingDoctor == null)
            {
                return await Task.FromResult(false);
            }
            existingDoctor.FullName = doctor.FullName;
            existingDoctor.Specialty = doctor.Specialty;
            existingDoctor.Email = doctor.Email;
            existingDoctor.PhoneNumber = doctor.PhoneNumber;
            existingDoctor.IsAvailable = doctor.IsAvailable;

            existingDoctor.UpdatedBy = doctor.UpdatedBy;
            existingDoctor.UpdatedAt = DateTime.UtcNow;

            _context.Doctors.Update(existingDoctor);
            await _context.SaveChangesAsync();

            return await Task.FromResult(true);
        }
    }
}
