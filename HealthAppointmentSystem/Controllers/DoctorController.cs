using AutoMapper;
using HealthAppointmentSystem.DTOs.Doctor;
using HealthAppointmentSystem.Extensions;
using HealthAppointmentSystem.Models;
using HealthAppointmentSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAppointmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IMapper _mapper;

        public DoctorController(IDoctorRepository doctorRepository, IMapper mapper)
        {
            _doctorRepository = doctorRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _doctorRepository.GetAllAsync();
            return Ok(_mapper.Map<List<DoctorDto>>(doctors));
        }

        [HttpGet("available")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> GetAvailable()
        {
            var doctors = await _doctorRepository.GetAllAsync();
            var available = doctors.Where(d => d.IsAvailable);
            return Ok(_mapper.Map<List<DoctorDto>>(available));
        }

        [HttpGet("me")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var doctor = await _doctorRepository.GetByUserIdAsync(userId.Value);
            if (doctor == null)
                return NotFound("Doctor profile not found");

            return Ok(_mapper.Map<DoctorDto>(doctor));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);
            if (doctor == null)
                return NotFound("Doctor not found");

            if (User.IsInRole("Doctor"))
            {
                var userId = User.GetUserId();
                if (userId == null || doctor.UserId != userId)
                    return Forbid();
            }

            return Ok(_mapper.Map<DoctorDto>(doctor));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateDoctorDto dto)
        {
            var existing = await _doctorRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound("Doctor not found");

            if (User.IsInRole("Doctor"))
            {
                var userId = User.GetUserId();
                if (userId == null || existing.UserId != userId)
                    return Forbid();
            }

            var doctor = _mapper.Map<Doctor>(dto);
            doctor.UpdatedBy = User.GetUserId() ?? Guid.Empty;

            var result = await _doctorRepository.UpdateAsync(id, doctor);
            if (!result)
                return NotFound("Doctor not found");

            return Ok("Doctor updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _doctorRepository.DeleteAsync(id);
            if (!result)
                return NotFound("Doctor not found");

            return Ok("Doctor deleted successfully");
        }
    }
}
