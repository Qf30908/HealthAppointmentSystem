using AutoMapper;
using HealthAppointmentSystem.DTOs.MedicalRecord;
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
    public class MedicalRecordController : ControllerBase
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;

        public MedicalRecordController(
            IMedicalRecordRepository medicalRecordRepository,
            IPatientRepository patientRepository,
            IMapper mapper)
        {
            _medicalRecordRepository = medicalRecordRepository;
            _patientRepository = patientRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAll()
        {
            var records = await _medicalRecordRepository.GetAllAsync();
            return Ok(_mapper.Map<List<MedicalRecordDto>>(records));
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetByPatient(Guid patientId)
        {
            if (!await CanAccessPatientRecords(patientId))
                return Forbid();

            var records = await _medicalRecordRepository.GetByPatientIdAsync(patientId);
            return Ok(_mapper.Map<List<MedicalRecordDto>>(records));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _medicalRecordRepository.GetByIdAsync(id);
            if (record == null)
                return NotFound("Medical record not found");

            if (!await CanAccessPatientRecords(record.PatientId))
                return Forbid();

            return Ok(_mapper.Map<MedicalRecordDto>(record));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create([FromBody] CreateMedicalRecordDto dto)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var patient = await _patientRepository.GetByIdAsync(dto.PatientId);
            if (patient == null)
                return NotFound("Patient not found");

            var record = _mapper.Map<MedicalRecord>(dto);
            record.CreatedBy = userId.Value;
            record.UpdatedBy = userId.Value;

            await _medicalRecordRepository.AddAsync(record);

            return Ok(new
            {
                message = "Medical record created successfully",
                record.Id
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicalRecordDto dto)
        {
            var existing = await _medicalRecordRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound("Medical record not found");

            var record = _mapper.Map<MedicalRecord>(dto);
            record.UpdatedBy = User.GetUserId() ?? Guid.Empty;

            var result = await _medicalRecordRepository.UpdateAsync(id, record);
            if (!result)
                return NotFound("Medical record not found");

            return Ok("Medical record updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _medicalRecordRepository.DeleteAsync(id);
            if (!result)
                return NotFound("Medical record not found");

            return Ok("Medical record deleted successfully");
        }

        private async Task<bool> CanAccessPatientRecords(Guid patientId)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Doctor"))
                return true;

            if (User.IsInRole("Patient"))
            {
                var userId = User.GetUserId();
                if (userId == null)
                    return false;

                var patient = await _patientRepository.GetByUserIdAsync(userId.Value);
                return patient != null && patient.Id == patientId;
            }

            return false;
        }
    }
}
