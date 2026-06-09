using AutoMapper;
using FluentAssertions;
using HealthAppointmentSystem.Controllers;
using HealthAppointmentSystem.DTOs.Patient;
using HealthAppointmentSystem.Models;
using HealthAppointmentSystem.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class PatientControllerTests
    {
        private readonly Mock<IPatientRepository> _patientRepo = new();
        private readonly Mock<IMapper> _mapper = new();

        private PatientController CreateController(string role, Guid userId)
        {
            var controller = new PatientController(
                _patientRepo.Object,
                _mapper.Object
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
        public async Task GetAll_ShouldReturnOk_WithPatients()
        {
            var patients = new List<Patient>
        {
            new Patient { Id = Guid.NewGuid(), FullName = "Patient 1" },
            new Patient { Id = Guid.NewGuid(), FullName = "Patient 2" }
        };

            var patientDtos = new List<PatientDto>
        {
            new PatientDto(),
            new PatientDto()
        };

            _patientRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(patients);

            _mapper.Setup(m => m.Map<List<PatientDto>>(patients))
                .Returns(patientDtos);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<PatientDto>>(okResult.Value);

            value.Count.Should().Be(2);
            _patientRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMe_WhenPatientExists_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();

            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = "Patient Test"
            };

            var dto = new PatientDto();

            _patientRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            _mapper.Setup(m => m.Map<PatientDto>(patient))
                .Returns(dto);

            var controller = CreateController("Patient", userId);

            var result = await controller.GetMe();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMe_WhenPatientDoesNotExist_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();

            _patientRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync((Patient)null);

            var controller = CreateController("Patient", userId);

            var result = await controller.GetMe();

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenPatientDoesNotExist_ShouldReturnNotFound()
        {
            var patientId = Guid.NewGuid();

            _patientRepo.Setup(r => r.GetByIdAsync(patientId))
                .ReturnsAsync((Patient)null);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetById(patientId);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenAdmin_ShouldReturnOk()
        {
            var patientId = Guid.NewGuid();

            var patient = new Patient
            {
                Id = patientId,
                UserId = Guid.NewGuid(),
                FullName = "Patient Test"
            };

            var dto = new PatientDto();

            _patientRepo.Setup(r => r.GetByIdAsync(patientId))
                .ReturnsAsync(patient);

            _mapper.Setup(m => m.Map<PatientDto>(patient))
                .Returns(dto);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetById(patientId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenPatientAccessesOwnProfile_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var patient = new Patient
            {
                Id = patientId,
                UserId = userId,
                FullName = "Own Patient"
            };

            var dto = new PatientDto();

            _patientRepo.Setup(r => r.GetByIdAsync(patientId))
                .ReturnsAsync(patient);

            _mapper.Setup(m => m.Map<PatientDto>(patient))
                .Returns(dto);

            var controller = CreateController("Patient", userId);

            var result = await controller.GetById(patientId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenPatientAccessesOtherProfile_ShouldReturnForbid()
        {
            var loggedUserId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var patient = new Patient
            {
                Id = patientId,
                UserId = otherUserId,
                FullName = "Other Patient"
            };

            _patientRepo.Setup(r => r.GetByIdAsync(patientId))
                .ReturnsAsync(patient);

            var controller = CreateController("Patient", loggedUserId);

            var result = await controller.GetById(patientId);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Update_WhenPatientDoesNotExist_ShouldReturnNotFound()
        {
            var patientId = Guid.NewGuid();

            var dto = new CreatePatientDto
            {
                FullName = "Updated Patient",
                Email = "patient@test.com",
                PhoneNumber = "070123456",
                DateOfBirth = new DateTime(2000, 1, 1)
            };

            _patientRepo.Setup(r => r.GetByIdAsync(patientId))
                .ReturnsAsync((Patient)null);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.Update(patientId, dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Update_WhenPatientUpdatesOtherProfile_ShouldReturnForbid()
        {
            var loggedUserId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var existingPatient = new Patient
            {
                Id = patientId,
                UserId = otherUserId
            };

            var dto = new CreatePatientDto
            {
                FullName = "Updated Patient",
                Email = "patient@test.com",
                PhoneNumber = "070123456",
                DateOfBirth = new DateTime(2000, 1, 1)
            };

            _patientRepo.Setup(r => r.GetByIdAsync(patientId))
                .ReturnsAsync(existingPatient);

            var controller = CreateController("Patient", loggedUserId);

            var result = await controller.Update(patientId, dto);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Update_WhenAdminUpdatesPatient_ShouldReturnOk()
        {
            var adminId = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var existingPatient = new Patient
            {
                Id = patientId,
                UserId = Guid.NewGuid()
            };

            var dto = new CreatePatientDto
            {
                FullName = "Updated Patient",
                Email = "patient@test.com",
                PhoneNumber = "070123456",
                DateOfBirth = new DateTime(2000, 1, 1)
            };

            var mappedPatient = new Patient
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth
            };

            _patientRepo.Setup(r => r.GetByIdAsync(patientId))
                .ReturnsAsync(existingPatient);

            _mapper.Setup(m => m.Map<Patient>(dto))
                .Returns(mappedPatient);

            _patientRepo.Setup(r => r.UpdateAsync(patientId, mappedPatient))
                .ReturnsAsync(true);

            var controller = CreateController("Admin", adminId);

            var result = await controller.Update(patientId, dto);

            Assert.IsType<OkObjectResult>(result);
            mappedPatient.UpdatedBy.Should().Be(adminId);
        }

        [Fact]
        public async Task Update_WhenPatientUpdatesOwnProfile_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var existingPatient = new Patient
            {
                Id = patientId,
                UserId = userId
            };

            var dto = new CreatePatientDto
            {
                FullName = "Updated Patient",
                Email = "patient@test.com",
                PhoneNumber = "070123456",
                DateOfBirth = new DateTime(2000, 1, 1)
            };

            var mappedPatient = new Patient
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth
            };

            _patientRepo.Setup(r => r.GetByIdAsync(patientId))
                .ReturnsAsync(existingPatient);

            _mapper.Setup(m => m.Map<Patient>(dto))
                .Returns(mappedPatient);

            _patientRepo.Setup(r => r.UpdateAsync(patientId, mappedPatient))
                .ReturnsAsync(true);

            var controller = CreateController("Patient", userId);

            var result = await controller.Update(patientId, dto);

            Assert.IsType<OkObjectResult>(result);
            mappedPatient.UpdatedBy.Should().Be(userId);
        }

        [Fact]
        public async Task Update_WhenRepositoryReturnsFalse_ShouldReturnNotFound()
        {
            var patientId = Guid.NewGuid();

            var existingPatient = new Patient
            {
                Id = patientId
            };

            var dto = new CreatePatientDto
            {
                FullName = "Updated Patient",
                Email = "patient@test.com",
                PhoneNumber = "070123456",
                DateOfBirth = new DateTime(2000, 1, 1)
            };

            var mappedPatient = new Patient();

            _patientRepo.Setup(r => r.GetByIdAsync(patientId))
                .ReturnsAsync(existingPatient);

            _mapper.Setup(m => m.Map<Patient>(dto))
                .Returns(mappedPatient);

            _patientRepo.Setup(r => r.UpdateAsync(patientId, mappedPatient))
                .ReturnsAsync(false);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.Update(patientId, dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_WhenPatientDoesNotExist_ShouldReturnNotFound()
        {
            var patientId = Guid.NewGuid();

            _patientRepo.Setup(r => r.DeleteAsync(patientId))
                .ReturnsAsync(false);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.Delete(patientId);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_WhenPatientExists_ShouldReturnOk()
        {
            var patientId = Guid.NewGuid();

            _patientRepo.Setup(r => r.DeleteAsync(patientId))
                .ReturnsAsync(true);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.Delete(patientId);

            Assert.IsType<OkObjectResult>(result);
            _patientRepo.Verify(r => r.DeleteAsync(patientId), Times.Once);
        }
    }
}
