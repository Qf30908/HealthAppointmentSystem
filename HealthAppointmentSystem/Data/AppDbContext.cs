using HealthAppointmentSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using static System.Net.WebRequestMethods;

namespace HealthAppointmentSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        // DbSet properties represent tables in the database
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }

    }
}
