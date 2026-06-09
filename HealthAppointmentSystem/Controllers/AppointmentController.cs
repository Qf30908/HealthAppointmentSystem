using AutoMapper;
using HealthAppointmentSystem.AUTH;
using HealthAppointmentSystem.DTOs.Appointment;
using HealthAppointmentSystem.Extensions;
using HealthAppointmentSystem.Models;
using HealthAppointmentSystem.Repositories;
using HealthAppointmentSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace HealthAppointmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;


        public AppointmentController(
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IMapper mapper, IEmailService emailService)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _mapper = mapper;
            _emailService = emailService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _appointmentRepository.GetAllAsync();
            return Ok(_mapper.Map<List<AppointmentDto>>(appointments));
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            if (User.IsInRole("Doctor"))
            {
                var doctor = await _doctorRepository.GetByUserIdAsync(userId.Value);
                if (doctor == null)
                    return NotFound("Doctor profile not found");

                var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctor.Id);
                return Ok(_mapper.Map<List<AppointmentDto>>(appointments));
            }

            if (User.IsInRole("Patient"))
            {
                var patient = await _patientRepository.GetByUserIdAsync(userId.Value);
                if (patient == null)
                    return NotFound("Patient profile not found");

                var appointments = await _appointmentRepository.GetByPatientIdAsync(patient.Id);
                return Ok(_mapper.Map<List<AppointmentDto>>(appointments));
            }

            return Forbid();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return NotFound("Appointment not found");

            if (!await CanAccessAppointment(appointment))
                return Forbid();

            return Ok(_mapper.Map<AppointmentDto>(appointment));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var appointment = _mapper.Map<Appointment>(dto);
            appointment.CreatedBy = userId.Value;
            appointment.UpdatedBy = userId.Value;

            Patient patient = null;

            if (User.IsInRole("Patient"))
            {
                patient = await _patientRepository.GetByUserIdAsync(userId.Value);
                if (patient == null)
                    return NotFound("Patient profile not found");

                appointment.PatientId = patient.Id;
            }
            else
            {
                patient = await _patientRepository.GetByIdAsync(dto.PatientId);
                if (patient == null)
                    return NotFound("Patient not found");
            }

            var created = await _appointmentRepository.AddAsync(appointment);
            if (!created)
                return BadRequest("Doctor is not available at the selected time / You can not make appointment at the past");

            var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId);
            if (doctor == null)
                return NotFound("Doctor not found");

            await _emailService.SendEmailAsync(
                patient.Email,
                "Appointment Confirmation",
                $"Your appointment with Dr. {doctor.FullName} has been scheduled for {appointment.AppointmentDate:dd/MM/yyyy HH:mm} until {appointment.AppointmentDate.AddMinutes(30):HH:mm}");

            return Ok(new
            {
                message = "Appointment created successfully",
                appointment.Id
            });
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAppointmentStatusDto dto)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return NotFound("Appointment not found");

            if (User.IsInRole("Doctor"))
            {
                var userId = User.GetUserId();
                var doctor = userId == null
                    ? null
                    : await _doctorRepository.GetByUserIdAsync(userId.Value);

                if (doctor == null || appointment.DoctorId != doctor.Id)
                    return Forbid();
            }

            var updatedBy = User.GetUserId() ?? Guid.Empty;
            var result = await _appointmentRepository.UpdateStatusAsync(id, dto.Status, updatedBy);
            if (!result)
                return NotFound("Appointment not found");

            return Ok("Appointment status updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return NotFound("Appointment not found");

            if (User.IsInRole("Patient"))
            {
                var userId = User.GetUserId();
                var patient = userId == null
                    ? null
                    : await _patientRepository.GetByUserIdAsync(userId.Value);

                if (patient == null || appointment.PatientId != patient.Id)
                    return Forbid();
            }

            var result = await _appointmentRepository.DeleteAsync(id);
            if (!result)
                return NotFound("Appointment not found");

            return Ok("Appointment deleted successfully");
        }

        private async Task<bool> CanAccessAppointment(Appointment appointment)
        {
            if (User.IsInRole("Admin"))
                return true;

            var userId = User.GetUserId();
            if (userId == null)
                return false;

            if (User.IsInRole("Doctor"))
            {
                var doctor = await _doctorRepository.GetByUserIdAsync(userId.Value);
                return doctor != null && appointment.DoctorId == doctor.Id;
            }

            if (User.IsInRole("Patient"))
            {
                var patient = await _patientRepository.GetByUserIdAsync(userId.Value);
                return patient != null && appointment.PatientId == patient.Id;
            }

            return false;
        }
    }
}
