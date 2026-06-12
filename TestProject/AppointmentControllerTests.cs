using AutoMapper;
using FluentAssertions;
using HealthAppointmentSystem.Controllers;
using HealthAppointmentSystem.DTOs.Appointment;
using HealthAppointmentSystem.Enums;
using HealthAppointmentSystem.Models;
using HealthAppointmentSystem.Repositories;
using HealthAppointmentSystem.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;



namespace TestProject
{
    public class AppointmentControllerTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepo;
        private readonly Mock<IDoctorRepository> _doctorRepo;
        private readonly Mock<IPatientRepository> _patientRepo;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IEmailService> _emailService;
        private readonly Mock<ILogger<AppointmentController>> _logger;
        private readonly TelemetryClient? _telemetery;

        public AppointmentControllerTests()
        {
            _appointmentRepo = new Mock<IAppointmentRepository>();
            _doctorRepo = new Mock<IDoctorRepository>();
            _patientRepo = new Mock<IPatientRepository>();
            _mapper = new Mock<IMapper>();
            _emailService = new Mock<IEmailService>();
            _logger = new Mock<ILogger<AppointmentController>>();

 
        }

        private AppointmentController CreateController(string role, Guid userId)
        {
            var controller = new AppointmentController(
                _appointmentRepo.Object,
                _doctorRepo.Object,
                _patientRepo.Object,
                _mapper.Object,
                _emailService.Object,
                _logger.Object,
                null
            );

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };

            return controller;
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithAppointments()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { Id = Guid.NewGuid() },
                new Appointment { Id = Guid.NewGuid() }
            };

            var dtos = new List<AppointmentDto>
            {
                new AppointmentDto(),
                new AppointmentDto()
            };

            _appointmentRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(appointments);
            _mapper.Setup(m => m.Map<List<AppointmentDto>>(It.IsAny<List<Appointment>>())).Returns(dtos);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsAssignableFrom<List<AppointmentDto>>(okResult.Value);

            value.Should().HaveCount(2);
            _appointmentRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMyAppointments_WhenPatient_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var patient = new Patient
            {
                Id = patientId,
                UserId = userId,
                FullName = "Patient Test"
            };

            var appointments = new List<Appointment>
            {
                new Appointment { Id = Guid.NewGuid(), PatientId = patientId }
            };

            var dtos = new List<AppointmentDto>
            {
                new AppointmentDto()
            };

            _patientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(patient);
            _appointmentRepo.Setup(r => r.GetByPatientIdAsync(patientId)).ReturnsAsync(appointments);
            _mapper.Setup(m => m.Map<List<AppointmentDto>>(It.IsAny<List<Appointment>>())).Returns(dtos);

            var controller = CreateController("Patient", userId);

            var result = await controller.GetMyAppointments();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsAssignableFrom<List<AppointmentDto>>(okResult.Value);

            value.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetMyAppointments_WhenDoctor_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();

            var doctor = new Doctor
            {
                Id = doctorId,
                UserId = userId,
                FullName = "Dr. Test"
            };

            var appointments = new List<Appointment>
            {
                new Appointment { Id = Guid.NewGuid(), DoctorId = doctorId }
            };

            var dtos = new List<AppointmentDto>
            {
                new AppointmentDto()
            };

            _doctorRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(doctor);
            _appointmentRepo.Setup(r => r.GetByDoctorIdAsync(doctorId)).ReturnsAsync(appointments);
            _mapper.Setup(m => m.Map<List<AppointmentDto>>(It.IsAny<List<Appointment>>())).Returns(dtos);

            var controller = CreateController("Doctor", userId);

            var result = await controller.GetMyAppointments();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsAssignableFrom<List<AppointmentDto>>(okResult.Value);

            value.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetMyAppointments_WhenPatientProfileNotFound_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();

            _patientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Patient?)null);

            var controller = CreateController("Patient", userId);

            var result = await controller.GetMyAppointments();

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetMyAppointments_WhenDoctorProfileNotFound_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();

            _doctorRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Doctor?)null);

            var controller = CreateController("Doctor", userId);

            var result = await controller.GetMyAppointments();

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenAppointmentDoesNotExist_ShouldReturnNotFound()
        {
            var appointmentId = Guid.NewGuid();

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync((Appointment?)null);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetById(appointmentId);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenAdmin_ShouldReturnOk()
        {
            var appointmentId = Guid.NewGuid();

            var appointment = new Appointment
            {
                Id = appointmentId
            };

            var dto = new AppointmentDto();

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);
            _mapper.Setup(m => m.Map<AppointmentDto>(It.IsAny<Appointment>())).Returns(dto);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetById(appointmentId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Create_WhenPatient_ShouldCreateAppointment_AndReturnOk()
        {
            var userId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();

            var dto = new CreateAppointmentDto
            {
                DoctorId = doctorId,
                AppointmentDate = DateTime.UtcNow.AddDays(1)
            };

            var patient = new Patient
            {
                Id = patientId,
                UserId = userId,
                Email = "patient@test.com",
                FullName = "Patient Test"
            };

            var doctor = new Doctor
            {
                Id = doctorId,
                FullName = "Doctor Test"
            };

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                PatientId = patientId,
                AppointmentDate = dto.AppointmentDate
            };

            _mapper.Setup(m => m.Map<Appointment>(It.IsAny<CreateAppointmentDto>())).Returns(appointment);
            _patientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(patient);
            _appointmentRepo.Setup(r => r.AddAsync(It.IsAny<Appointment>())).ReturnsAsync(true);
            _doctorRepo.Setup(r => r.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
            _emailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var controller = CreateController("Patient", userId);

            var result = await controller.Create(dto);

            Assert.IsType<OkObjectResult>(result);
            _appointmentRepo.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Once);
            _emailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Create_WhenPatientProfileNotFound_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();

            var dto = new CreateAppointmentDto
            {
                DoctorId = Guid.NewGuid(),
                AppointmentDate = DateTime.UtcNow.AddDays(1)
            };

            _mapper.Setup(m => m.Map<Appointment>(It.IsAny<CreateAppointmentDto>()))
                .Returns(new Appointment());

            _patientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Patient?)null);

            var controller = CreateController("Patient", userId);

            var result = await controller.Create(dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Create_WhenRepositoryReturnsFalse_ShouldReturnBadRequest()
        {
            var userId = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var dto = new CreateAppointmentDto
            {
                DoctorId = Guid.NewGuid(),
                AppointmentDate = DateTime.UtcNow.AddDays(1)
            };

            var patient = new Patient
            {
                Id = patientId,
                UserId = userId,
                Email = "patient@test.com",
                FullName = "Patient Test"
            };

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.AppointmentDate
            };

            _mapper.Setup(m => m.Map<Appointment>(It.IsAny<CreateAppointmentDto>())).Returns(appointment);
            _patientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(patient);
            _appointmentRepo.Setup(r => r.AddAsync(It.IsAny<Appointment>())).ReturnsAsync(false);

            var controller = CreateController("Patient", userId);

            var result = await controller.Create(dto);

            Assert.IsType<BadRequestObjectResult>(result);
            _emailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Create_WhenDoctorNotFound_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();

            var dto = new CreateAppointmentDto
            {
                DoctorId = doctorId,
                AppointmentDate = DateTime.UtcNow.AddDays(1)
            };

            var patient = new Patient
            {
                Id = patientId,
                UserId = userId,
                Email = "patient@test.com",
                FullName = "Patient Test"
            };

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                AppointmentDate = dto.AppointmentDate
            };

            _mapper.Setup(m => m.Map<Appointment>(It.IsAny<CreateAppointmentDto>())).Returns(appointment);
            _patientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(patient);
            _appointmentRepo.Setup(r => r.AddAsync(It.IsAny<Appointment>())).ReturnsAsync(true);
            _doctorRepo.Setup(r => r.GetByIdAsync(doctorId)).ReturnsAsync((Doctor?)null);

            var controller = CreateController("Patient", userId);

            var result = await controller.Create(dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateStatus_WhenAppointmentDoesNotExist_ShouldReturnNotFound()
        {
            var appointmentId = Guid.NewGuid();

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Confirmed
            };

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync((Appointment?)null);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.UpdateStatus(appointmentId, dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateStatus_WhenAdmin_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            var appointmentId = Guid.NewGuid();

            var appointment = new Appointment
            {
                Id = appointmentId
            };

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Confirmed
            };

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);
            _appointmentRepo.Setup(r => r.UpdateStatusAsync(appointmentId, dto.Status, userId)).ReturnsAsync(true);

            var controller = CreateController("Admin", userId);

            var result = await controller.UpdateStatus(appointmentId, dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateStatus_WhenDoctorOwnsAppointment_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();
            var appointmentId = Guid.NewGuid();

            var doctor = new Doctor
            {
                Id = doctorId,
                UserId = userId
            };

            var appointment = new Appointment
            {
                Id = appointmentId,
                DoctorId = doctorId
            };

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Confirmed
            };

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);
            _doctorRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(doctor);
            _appointmentRepo.Setup(r => r.UpdateStatusAsync(appointmentId, dto.Status, userId)).ReturnsAsync(true);

            var controller = CreateController("Doctor", userId);

            var result = await controller.UpdateStatus(appointmentId, dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Delete_WhenAppointmentDoesNotExist_ShouldReturnNotFound()
        {
            var appointmentId = Guid.NewGuid();

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync((Appointment?)null);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.Delete(appointmentId);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_WhenPatientOwnsAppointment_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var appointmentId = Guid.NewGuid();

            var patient = new Patient
            {
                Id = patientId,
                UserId = userId
            };

            var appointment = new Appointment
            {
                Id = appointmentId,
                PatientId = patientId
            };

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);
            _patientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(patient);
            _appointmentRepo.Setup(r => r.DeleteAsync(appointmentId)).ReturnsAsync(true);

            var controller = CreateController("Patient", userId);

            var result = await controller.Delete(appointmentId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Delete_WhenAdmin_ShouldReturnOk()
        {
            var appointmentId = Guid.NewGuid();

            var appointment = new Appointment
            {
                Id = appointmentId
            };

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);
            _appointmentRepo.Setup(r => r.DeleteAsync(appointmentId)).ReturnsAsync(true);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.Delete(appointmentId);

            Assert.IsType<OkObjectResult>(result);
        }
    }
}