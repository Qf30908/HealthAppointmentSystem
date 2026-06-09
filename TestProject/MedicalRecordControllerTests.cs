using AutoMapper;
using FluentAssertions;
using HealthAppointmentSystem.Controllers;
using HealthAppointmentSystem.DTOs.MedicalRecord;
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
    public class MedicalRecordControllerTests
    {
        private readonly Mock<IMedicalRecordRepository> _recordRepo = new();
        private readonly Mock<IPatientRepository> _patientRepo = new();
        private readonly Mock<IMapper> _mapper = new();

        private MedicalRecordController CreateController(string role, Guid userId)
        {
            var controller = new MedicalRecordController(
                _recordRepo.Object,
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
        public async Task GetAll_ShouldReturnOk_WithRecords()
        {
            var records = new List<MedicalRecord>
        {
            new MedicalRecord { Id = Guid.NewGuid() },
            new MedicalRecord { Id = Guid.NewGuid() }
        };

            var dtos = new List<MedicalRecordDto>
        {
            new MedicalRecordDto(),
            new MedicalRecordDto()
        };

            _recordRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(records);

            _mapper.Setup(m => m.Map<List<MedicalRecordDto>>(records))
                .Returns(dtos);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<MedicalRecordDto>>(okResult.Value);

            value.Count.Should().Be(2);
            _recordRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByPatient_WhenAdmin_ShouldReturnOk()
        {
            var patientId = Guid.NewGuid();

            var records = new List<MedicalRecord>
        {
            new MedicalRecord { Id = Guid.NewGuid(), PatientId = patientId }
        };

            var dtos = new List<MedicalRecordDto>
        {
            new MedicalRecordDto()
        };

            _recordRepo.Setup(r => r.GetByPatientIdAsync(patientId))
                .ReturnsAsync(records);

            _mapper.Setup(m => m.Map<List<MedicalRecordDto>>(records))
                .Returns(dtos);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetByPatient(patientId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<MedicalRecordDto>>(okResult.Value);

            value.Count.Should().Be(1);
        }

        [Fact]
        public async Task GetByPatient_WhenPatientOwnsRecords_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var patient = new Patient
            {
                Id = patientId,
                UserId = userId
            };

            var records = new List<MedicalRecord>
        {
            new MedicalRecord { Id = Guid.NewGuid(), PatientId = patientId }
        };

            var dtos = new List<MedicalRecordDto>
        {
            new MedicalRecordDto()
        };

            _patientRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            _recordRepo.Setup(r => r.GetByPatientIdAsync(patientId))
                .ReturnsAsync(records);

            _mapper.Setup(m => m.Map<List<MedicalRecordDto>>(records))
                .Returns(dtos);

            var controller = CreateController("Patient", userId);

            var result = await controller.GetByPatient(patientId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetByPatient_WhenPatientDoesNotOwnRecords_ShouldReturnForbid()
        {
            var userId = Guid.NewGuid();
            var ownPatientId = Guid.NewGuid();
            var otherPatientId = Guid.NewGuid();

            var patient = new Patient
            {
                Id = ownPatientId,
                UserId = userId
            };

            _patientRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var controller = CreateController("Patient", userId);

            var result = await controller.GetByPatient(otherPatientId);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetById_WhenRecordDoesNotExist_ShouldReturnNotFound()
        {
            var recordId = Guid.NewGuid();

            _recordRepo.Setup(r => r.GetByIdAsync(recordId))
                .ReturnsAsync((MedicalRecord)null);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetById(recordId);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenAdmin_ShouldReturnOk()
        {
            var recordId = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var record = new MedicalRecord
            {
                Id = recordId,
                PatientId = patientId
            };

            var dto = new MedicalRecordDto();

            _recordRepo.Setup(r => r.GetByIdAsync(recordId))
                .ReturnsAsync(record);

            _mapper.Setup(m => m.Map<MedicalRecordDto>(record))
                .Returns(dto);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetById(recordId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenPatientDoesNotOwnRecord_ShouldReturnForbid()
        {
            var userId = Guid.NewGuid();
            var ownPatientId = Guid.NewGuid();
            var otherPatientId = Guid.NewGuid();
            var recordId = Guid.NewGuid();

            var patient = new Patient
            {
                Id = ownPatientId,
                UserId = userId
            };

            var record = new MedicalRecord
            {
                Id = recordId,
                PatientId = otherPatientId
            };

            _recordRepo.Setup(r => r.GetByIdAsync(recordId))
                .ReturnsAsync(record);

            _patientRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var controller = CreateController("Patient", userId);

            var result = await controller.GetById(recordId);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Create_WhenPatientDoesNotExist_ShouldReturnNotFound()
        {
            var dto = new CreateMedicalRecordDto
            {
                PatientId = Guid.NewGuid(),
                Diagnosis = "Flu",
                Prescription = "Medicine",
                DoctorNotes = "Rest"
            };

            _patientRepo.Setup(r => r.GetByIdAsync(dto.PatientId))
                .ReturnsAsync((Patient)null);

            var controller = CreateController("Doctor", Guid.NewGuid());

            var result = await controller.Create(dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Create_WhenPatientExists_ShouldCreateRecordAndReturnOk()
        {
            var userId = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var dto = new CreateMedicalRecordDto
            {
                PatientId = patientId,
                Diagnosis = "Flu",
                Prescription = "Medicine",
                DoctorNotes = "Rest"
            };

            var patient = new Patient
            {
                Id = patientId
            };

            var record = new MedicalRecord
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                Diagnosis = dto.Diagnosis,
                Prescription = dto.Prescription,
                DoctorNotes = dto.DoctorNotes
            };

            _patientRepo.Setup(r => r.GetByIdAsync(patientId))
                .ReturnsAsync(patient);

            _mapper.Setup(m => m.Map<MedicalRecord>(dto))
                .Returns(record);

            _recordRepo.Setup(r => r.AddAsync(record))
                .ReturnsAsync(true);

            var controller = CreateController("Doctor", userId);

            var result = await controller.Create(dto);

            Assert.IsType<OkObjectResult>(result);

            record.CreatedBy.Should().Be(userId);
            record.UpdatedBy.Should().Be(userId);

            _recordRepo.Verify(r => r.AddAsync(record), Times.Once);
        }

        [Fact]
        public async Task Update_WhenRecordDoesNotExist_ShouldReturnNotFound()
        {
            var recordId = Guid.NewGuid();

            var dto = new UpdateMedicalRecordDto
            {
                Diagnosis = "Updated Diagnosis",
                Prescription = "Updated Prescription",
                DoctorNotes = "Updated Notes"
            };

            _recordRepo.Setup(r => r.GetByIdAsync(recordId))
                .ReturnsAsync((MedicalRecord)null);

            var controller = CreateController("Doctor", Guid.NewGuid());

            var result = await controller.Update(recordId, dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Update_WhenRecordExists_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            var recordId = Guid.NewGuid();

            var existing = new MedicalRecord
            {
                Id = recordId
            };

            var dto = new UpdateMedicalRecordDto
            {
                Diagnosis = "Updated Diagnosis",
                Prescription = "Updated Prescription",
                DoctorNotes = "Updated Notes"
            };

            var mappedRecord = new MedicalRecord
            {
                Diagnosis = dto.Diagnosis,
                Prescription = dto.Prescription,
                DoctorNotes = dto.DoctorNotes
            };

            _recordRepo.Setup(r => r.GetByIdAsync(recordId))
                .ReturnsAsync(existing);

            _mapper.Setup(m => m.Map<MedicalRecord>(dto))
                .Returns(mappedRecord);

            _recordRepo.Setup(r => r.UpdateAsync(recordId, mappedRecord))
                .ReturnsAsync(true);

            var controller = CreateController("Doctor", userId);

            var result = await controller.Update(recordId, dto);

            Assert.IsType<OkObjectResult>(result);

            mappedRecord.UpdatedBy.Should().Be(userId);
            _recordRepo.Verify(r => r.UpdateAsync(recordId, mappedRecord), Times.Once);
        }

        [Fact]
        public async Task Update_WhenRepositoryReturnsFalse_ShouldReturnNotFound()
        {
            var recordId = Guid.NewGuid();

            var existing = new MedicalRecord
            {
                Id = recordId
            };

            var dto = new UpdateMedicalRecordDto
            {
                Diagnosis = "Updated Diagnosis",
                Prescription = "Updated Prescription",
                DoctorNotes = "Updated Notes"
            };

            var mappedRecord = new MedicalRecord();

            _recordRepo.Setup(r => r.GetByIdAsync(recordId))
                .ReturnsAsync(existing);

            _mapper.Setup(m => m.Map<MedicalRecord>(dto))
                .Returns(mappedRecord);

            _recordRepo.Setup(r => r.UpdateAsync(recordId, mappedRecord))
                .ReturnsAsync(false);

            var controller = CreateController("Doctor", Guid.NewGuid());

            var result = await controller.Update(recordId, dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_WhenRecordDoesNotExist_ShouldReturnNotFound()
        {
            var recordId = Guid.NewGuid();

            _recordRepo.Setup(r => r.DeleteAsync(recordId))
                .ReturnsAsync(false);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.Delete(recordId);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_WhenRecordExists_ShouldReturnOk()
        {
            var recordId = Guid.NewGuid();

            _recordRepo.Setup(r => r.DeleteAsync(recordId))
                .ReturnsAsync(true);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.Delete(recordId);

            Assert.IsType<OkObjectResult>(result);

            _recordRepo.Verify(r => r.DeleteAsync(recordId), Times.Once);
        }
    }
}
