using AutoMapper;
using FluentAssertions;
using HealthAppointmentSystem.Controllers;
using HealthAppointmentSystem.DTOs.Appointment;
using HealthAppointmentSystem.Enums;
using HealthAppointmentSystem.Models;
using HealthAppointmentSystem.Repositories;
using HealthAppointmentSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace TestProject
{
    

    public class AppointmentControllerTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepo = new();
        private readonly Mock<IDoctorRepository> _doctorRepo = new();
        private readonly Mock<IPatientRepository> _patientRepo = new();
        private readonly Mock<IMapper> _mapper = new();

        private readonly Mock<IEmailService> _emailService = new();

        private AppointmentController CreateController(string role, Guid userId)
        {
            var controller = new AppointmentController(
                _appointmentRepo.Object,
                _doctorRepo.Object,
                _patientRepo.Object,
                _mapper.Object,
                _emailService.Object
            );

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
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

            var appointmentDtos = new List<AppointmentDto>
        {
            new AppointmentDto(),
            new AppointmentDto()
        };

            _appointmentRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(appointments);

            _mapper.Setup(m => m.Map<List<AppointmentDto>>(appointments))
                .Returns(appointmentDtos);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<AppointmentDto>>(okResult.Value);

            value.Count.Should().Be(2);
            _appointmentRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMyAppointments_WhenPatient_ShouldReturnPatientAppointments()
        {
            var userId = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var patient = new Patient
            {
                Id = patientId,
                UserId = userId,
                FullName = "Test Patient"
            };

            var appointments = new List<Appointment>
        {
            new Appointment { Id = Guid.NewGuid(), PatientId = patientId }
        };

            var dtos = new List<AppointmentDto>
        {
            new AppointmentDto()
        };

            _patientRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            _appointmentRepo.Setup(r => r.GetByPatientIdAsync(patientId))
                .ReturnsAsync(appointments);

            _mapper.Setup(m => m.Map<List<AppointmentDto>>(appointments))
                .Returns(dtos);

            var controller = CreateController("Patient", userId);

            var result = await controller.GetMyAppointments();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<AppointmentDto>>(okResult.Value);

            value.Count.Should().Be(1);
        }

        [Fact]
        public async Task GetMyAppointments_WhenDoctor_ShouldReturnDoctorAppointments()
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

            _doctorRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(doctor);

            _appointmentRepo.Setup(r => r.GetByDoctorIdAsync(doctorId))
                .ReturnsAsync(appointments);

            _mapper.Setup(m => m.Map<List<AppointmentDto>>(appointments))
                .Returns(dtos);

            var controller = CreateController("Doctor", userId);

            var result = await controller.GetMyAppointments();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<AppointmentDto>>(okResult.Value);

            value.Count.Should().Be(1);
        }

        [Fact]
        public async Task GetById_WhenAppointmentDoesNotExist_ShouldReturnNotFound()
        {
            var appointmentId = Guid.NewGuid();

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
                .ReturnsAsync((Appointment)null);

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

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
                .ReturnsAsync(appointment);

            _mapper.Setup(m => m.Map<AppointmentDto>(appointment))
                .Returns(dto);

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

            _mapper.Setup(m => m.Map<Appointment>(dto))
                .Returns(appointment);

            _patientRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            _appointmentRepo.Setup(r => r.AddAsync(appointment))
                .ReturnsAsync(true);

            _doctorRepo.Setup(r => r.GetByIdAsync(doctorId))
                .ReturnsAsync(doctor);

            var controller = CreateController("Patient", userId);

            var result = await controller.Create(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);

            _appointmentRepo.Verify(r => r.AddAsync(appointment), Times.Once);
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
                UserId = userId
            };

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.AppointmentDate
            };

            _mapper.Setup(m => m.Map<Appointment>(dto))
                .Returns(appointment);

            _patientRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            _appointmentRepo.Setup(r => r.AddAsync(appointment))
                .ReturnsAsync(false);

            var controller = CreateController("Patient", userId);

            var result = await controller.Create(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateStatus_WhenAppointmentDoesNotExist_ShouldReturnNotFound()
        {
            var appointmentId = Guid.NewGuid();

            var dto = new UpdateAppointmentStatusDto
            {
                Status = AppointmentStatus.Confirmed
            };

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
                .ReturnsAsync((Appointment)null);

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

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
                .ReturnsAsync(appointment);

            _appointmentRepo.Setup(r => r.UpdateStatusAsync(appointmentId, dto.Status, userId))
                .ReturnsAsync(true);

            var controller = CreateController("Admin", userId);

            var result = await controller.UpdateStatus(appointmentId, dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Delete_WhenAppointmentDoesNotExist_ShouldReturnNotFound()
        {
            var appointmentId = Guid.NewGuid();

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
                .ReturnsAsync((Appointment)null);

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

            _appointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
                .ReturnsAsync(appointment);

            _patientRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            _appointmentRepo.Setup(r => r.DeleteAsync(appointmentId))
                .ReturnsAsync(true);

            var controller = CreateController("Patient", userId);

            var result = await controller.Delete(appointmentId);

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
