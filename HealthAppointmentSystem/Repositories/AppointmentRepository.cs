using HealthAppointmentSystem.Data;
using HealthAppointmentSystem.Enums;
using HealthAppointmentSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthAppointmentSystem.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _context;

        public AppointmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .ToListAsync();
        }

        public async Task<Appointment> GetByIdAsync(Guid id)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Include(a => a.Patient)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(Guid patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Include(a => a.Doctor)
                .ToListAsync();
        }

        public async Task<bool> AddAsync(Appointment appointment)
        {
            var isAvailable = await IsDoctorAvailableAsync(appointment.DoctorId, appointment.AppointmentDate);

            if (!isAvailable)
                return false;

            appointment.Status = AppointmentStatus.Pending;
            appointment.CreatedAt = DateTime.UtcNow;

            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateStatusAsync(Guid id, AppointmentStatus status)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
                return false;

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
                return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsDoctorAvailableAsync(Guid doctorId, DateTime dateTime)
        {
            var conflict = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate == dateTime &&
                a.Status != AppointmentStatus.Cancelled
            );

            return !conflict;
        }
    }
}