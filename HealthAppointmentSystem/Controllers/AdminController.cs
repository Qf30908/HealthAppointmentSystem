using HealthAppointmentSystem.AUTH;
using HealthAppointmentSystem.AUTH.Enums;
using HealthAppointmentSystem.Data;
using HealthAppointmentSystem.DTOs.Admin;
using HealthAppointmentSystem.Extensions;
using HealthAppointmentSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAppointmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create-doctor")]
        public async Task<IActionResult> CreateDoctor(CreateDoctorByAdminDto dto)
        {
            if (_context.Users.Any(x => x.Email == dto.Email))
                return BadRequest("User already exists");

            var adminId = User.GetUserId() ?? Guid.Empty;
            var now = DateTime.UtcNow;
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.Doctor
            };

            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = dto.FullName,
                Specialty = dto.Specialty,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                IsAvailable = true,
                CreatedBy = adminId,
                UpdatedBy = adminId,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Users.Add(user);
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Doctor created successfully",
                doctor.Id,
                user.Email
            });
        }

        [HttpPost("create-patient")]
        public async Task<IActionResult> CreatePatient(CreatePatientByAdminDto dto)
        {
            if (_context.Users.Any(x => x.Email == dto.Email))
                return BadRequest("User already exists");

            var adminId = User.GetUserId() ?? Guid.Empty;
            var now = DateTime.UtcNow;
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.Patient
            };

            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                CreatedBy = adminId,
                UpdatedBy = adminId,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Users.Add(user);
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Patient created successfully",
                patient.Id,
                user.Email
            });
        }
    }
}
