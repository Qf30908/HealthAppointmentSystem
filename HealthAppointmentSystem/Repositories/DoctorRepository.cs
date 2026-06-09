using HealthAppointmentSystem.Data;
using HealthAppointmentSystem.Enums;
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
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return false;

            var user = await _context.Users.FindAsync(doctor.UserId);
            _context.Doctors.Remove(doctor);

            if (user != null)
                _context.Users.Remove(user);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Doctor>> GetAllAsync()
        {
            return await _context.Doctors.AsNoTracking().ToListAsync();
        }

        public async Task<Doctor?> GetByIdAsync(Guid id)
        {
            return await _context.Doctors.FindAsync(id);
        }

        public async Task<Doctor?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<bool> UpdateAsync(Guid id, Doctor doctor)
        {
            var existingDoctor = await _context.Doctors.FindAsync(id);
            if (existingDoctor == null)
                return false;

            existingDoctor.FullName = doctor.FullName;
            existingDoctor.Specialty = doctor.Specialty;
            existingDoctor.Email = doctor.Email;
            existingDoctor.PhoneNumber = doctor.PhoneNumber;
            existingDoctor.IsAvailable = doctor.IsAvailable;
            existingDoctor.UpdatedBy = doctor.UpdatedBy;
            existingDoctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<Doctor>> SearchAsync(int pageNumber, int pageSize, string? specialty, DateTime? fromDate, DateTime? toDate, string? searchTest, string? sortField, bool? sortOrder)
        {
            var doctor = await _context.Doctors
                .Where(doctor => ((specialty == null) || (doctor.Specialty == specialty)) &&
                ((fromDate == null || toDate == null) ||
                (!_context.Appointments.Any(a =>
                    a.DoctorId == doctor.Id &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.AppointmentDate >= fromDate &&
                    a.AppointmentDate < toDate)))
                && ((string.IsNullOrEmpty(searchTest) || (doctor.FullName.Contains(searchTest) || doctor.Email.Contains(searchTest)))))
                .ToListAsync();

            switch (sortField)
            {
                case "FullName":
                    doctor = sortOrder == true ? doctor.OrderBy(doctor => doctor.FullName).ToList() : doctor.OrderByDescending(doctor => doctor.FullName).ToList();
                    break;
                case "Specialty":
                    doctor = sortOrder == true ? doctor.OrderBy(doctor => doctor.Specialty).ToList() : doctor.OrderByDescending(doctor => doctor.Specialty).ToList();
                    break;
                default:
                    doctor = sortOrder == true ? doctor.OrderBy(doctor => doctor.FullName).ToList() : doctor.OrderByDescending(doctor => doctor.FullName).ToList();
                    break;

            }

            doctor.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();


            return doctor;
        }


        public async Task<int> TotalCountDoctors(string? specialty, DateTime? fromDate, DateTime? toDate, string? searchTest)
        {
            var doctor = await _context.Doctors
                .Where(doctor => ((specialty == null) || (doctor.Specialty == specialty)) &&
                ((fromDate == null || toDate == null) ||
                (!_context.Appointments.Any(a =>
                    a.DoctorId == doctor.Id &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.AppointmentDate >= fromDate &&
                    a.AppointmentDate < toDate))) 
                && ((string.IsNullOrEmpty(searchTest) || (doctor.FullName.Contains(searchTest) || doctor.Email.Contains(searchTest)))))
                .CountAsync();
            return doctor;
        }
    }
}
