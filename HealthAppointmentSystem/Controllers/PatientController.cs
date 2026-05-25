using AutoMapper;
using HealthAppointmentSystem.DTOs.Patient;
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
    public class PatientController : ControllerBase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;

        public PatientController(IPatientRepository patientRepository, IMapper mapper)
        {
            _patientRepository = patientRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _patientRepository.GetAllAsync();
            return Ok(_mapper.Map<List<PatientDto>>(patients));
        }

        [HttpGet("me")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var patient = await _patientRepository.GetByUserIdAsync(userId.Value);
            if (patient == null)
                return NotFound("Patient profile not found");

            return Ok(_mapper.Map<PatientDto>(patient));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
                return NotFound("Patient not found");

            if (User.IsInRole("Patient"))
            {
                var userId = User.GetUserId();
                if (userId == null || patient.UserId != userId)
                    return Forbid();
            }

            return Ok(_mapper.Map<PatientDto>(patient));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreatePatientDto dto)
        {
            var existing = await _patientRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound("Patient not found");

            if (User.IsInRole("Patient"))
            {
                var userId = User.GetUserId();
                if (userId == null || existing.UserId != userId)
                    return Forbid();
            }

            var patient = _mapper.Map<Patient>(dto);
            patient.UpdatedBy = User.GetUserId() ?? Guid.Empty;

            var result = await _patientRepository.UpdateAsync(id, patient);
            if (!result)
                return NotFound("Patient not found");

            return Ok("Patient updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _patientRepository.DeleteAsync(id);
            if (!result)
                return NotFound("Patient not found");

            return Ok("Patient deleted successfully");
        }
    }
}
