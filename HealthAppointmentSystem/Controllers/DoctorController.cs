using AutoMapper;
using HealthAppointmentSystem.DTOs.Doctor;
using HealthAppointmentSystem.Extensions;
using HealthAppointmentSystem.Helpers;
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
        private readonly ILogger<DoctorController> _logger;

        public DoctorController(IDoctorRepository doctorRepository, IMapper mapper, ILogger<DoctorController> logger)
        {
            _doctorRepository = doctorRepository;
            _mapper = mapper;
            _logger = logger;
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
            try
            {
                _logger.LogInformation("Action for GetAvailable");
                var doctors = await _doctorRepository.GetAllAsync();
                _logger.LogInformation("Repository is Successful!");
                var available = doctors.Where(d => d.IsAvailable);
                var doctorDto = _mapper.Map<List<DoctorDto>>(available);
                _logger.LogInformation("Mapper is OK");
                return Ok(doctorDto);
            }

            catch (Exception ex)
            {

                _logger.LogError(ex, "Error occurred");
                return BadRequest(ex.Message);
            }
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

        [HttpGet("Search")]
        [AllowAnonymous]
        public async Task<ActionResult> Search(int? pageNumber, int? pageSize, string? specialty, DateTime? fromDate, DateTime? toDate, string? searchTest, string? sortField, bool? sortOrder)
        {
            pageNumber = pageNumber ?? 0;
            pageSize = pageSize ?? 10;

            var doctorList = await _doctorRepository.SearchAsync(pageNumber.Value, pageSize.Value, specialty, fromDate, toDate, searchTest, sortField, sortOrder);
            var totalItems = await _doctorRepository.TotalCountDoctors(specialty, fromDate, toDate, searchTest);
            var totalPages = (double)totalItems / pageSize;

            var resultModel = new PageModel<DoctorDto>();
            resultModel.Items = _mapper.Map<List<DoctorDto>>(doctorList);
            resultModel.PageNumber = pageNumber.Value;
            resultModel.PageSize = pageSize.Value;
            resultModel.TotalItems = totalItems;
            resultModel.TotalPages = (int)Math.Ceiling(totalPages.Value);
            return Ok(resultModel);
        }
    }
}
