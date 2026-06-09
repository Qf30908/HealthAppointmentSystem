using AutoMapper;
using FluentAssertions;
using HealthAppointmentSystem.Controllers;
using HealthAppointmentSystem.DTOs.Doctor;
using HealthAppointmentSystem.Helpers;
using HealthAppointmentSystem.Models;
using HealthAppointmentSystem.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class DoctorControllerTests
    {
        private readonly Mock<IDoctorRepository> _doctorRepo = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<ILogger<DoctorController>> _logger = new();

        private DoctorController CreateController(string role, Guid userId)
        {
            var controller = new DoctorController(
                _doctorRepo.Object,
                _mapper.Object,
                _logger.Object
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
        public async Task GetAll_ShouldReturnOk_WithDoctors()
        {
            var doctors = new List<Doctor>
        {
            new Doctor { Id = Guid.NewGuid(), FullName = "Doctor 1" },
            new Doctor { Id = Guid.NewGuid(), FullName = "Doctor 2" }
        };

            var doctorDtos = new List<DoctorDto>
        {
            new DoctorDto(),
            new DoctorDto()
        };

            _doctorRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(doctors);

            _mapper.Setup(m => m.Map<List<DoctorDto>>(doctors))
                .Returns(doctorDtos);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<DoctorDto>>(okResult.Value);

            value.Count.Should().Be(2);
            _doctorRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAvailable_ShouldReturnOnlyAvailableDoctors()
        {
            var doctors = new List<Doctor>
        {
            new Doctor { Id = Guid.NewGuid(), FullName = "Doctor 1", IsAvailable = true },
            new Doctor { Id = Guid.NewGuid(), FullName = "Doctor 2", IsAvailable = false },
            new Doctor { Id = Guid.NewGuid(), FullName = "Doctor 3", IsAvailable = true }
        };

            var availableDtos = new List<DoctorDto>
        {
            new DoctorDto(),
            new DoctorDto()
        };

            _doctorRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(doctors);

            _mapper.Setup(m => m.Map<List<DoctorDto>>(It.IsAny<IEnumerable<Doctor>>()))
                .Returns(availableDtos);

            var controller = CreateController("Patient", Guid.NewGuid());

            var result = await controller.GetAvailable();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<DoctorDto>>(okResult.Value);

            value.Count.Should().Be(2);
            _doctorRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAvailable_WhenRepositoryThrowsException_ShouldReturnBadRequest()
        {
            _doctorRepo.Setup(r => r.GetAllAsync())
                .ThrowsAsync(new Exception("Database error"));

            var controller = CreateController("Patient", Guid.NewGuid());

            var result = await controller.GetAvailable();

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Database error");
        }

        [Fact]
        public async Task GetMe_WhenDoctorExists_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();

            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = "Dr. Test"
            };

            var dto = new DoctorDto();

            _doctorRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(doctor);

            _mapper.Setup(m => m.Map<DoctorDto>(doctor))
                .Returns(dto);

            var controller = CreateController("Doctor", userId);

            var result = await controller.GetMe();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMe_WhenDoctorDoesNotExist_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();

            _doctorRepo.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync((Doctor)null);

            var controller = CreateController("Doctor", userId);

            var result = await controller.GetMe();

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenDoctorDoesNotExist_ShouldReturnNotFound()
        {
            var doctorId = Guid.NewGuid();

            _doctorRepo.Setup(r => r.GetByIdAsync(doctorId))
                .ReturnsAsync((Doctor)null);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetById(doctorId);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenAdmin_ShouldReturnOk()
        {
            var doctorId = Guid.NewGuid();

            var doctor = new Doctor
            {
                Id = doctorId,
                FullName = "Dr. Test"
            };

            var dto = new DoctorDto();

            _doctorRepo.Setup(r => r.GetByIdAsync(doctorId))
                .ReturnsAsync(doctor);

            _mapper.Setup(m => m.Map<DoctorDto>(doctor))
                .Returns(dto);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.GetById(doctorId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenDoctorAccessesOtherDoctor_ShouldReturnForbid()
        {
            var loggedUserId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();

            var doctor = new Doctor
            {
                Id = doctorId,
                UserId = otherUserId,
                FullName = "Other Doctor"
            };

            _doctorRepo.Setup(r => r.GetByIdAsync(doctorId))
                .ReturnsAsync(doctor);

            var controller = CreateController("Doctor", loggedUserId);

            var result = await controller.GetById(doctorId);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetById_WhenDoctorAccessesOwnProfile_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();

            var doctor = new Doctor
            {
                Id = doctorId,
                UserId = userId,
                FullName = "Dr. Own"
            };

            var dto = new DoctorDto();

            _doctorRepo.Setup(r => r.GetByIdAsync(doctorId))
                .ReturnsAsync(doctor);

            _mapper.Setup(m => m.Map<DoctorDto>(doctor))
                .Returns(dto);

            var controller = CreateController("Doctor", userId);

            var result = await controller.GetById(doctorId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Update_WhenDoctorDoesNotExist_ShouldReturnNotFound()
        {
            var doctorId = Guid.NewGuid();

            var dto = new CreateDoctorDto
            {
                FullName = "Updated Doctor",
                Specialty = "Cardiology",
                Email = "doctor@test.com",
                PhoneNumber = "070123456"
            };

            _doctorRepo.Setup(r => r.GetByIdAsync(doctorId))
                .ReturnsAsync((Doctor)null);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.Update(doctorId, dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Update_WhenDoctorUpdatesOtherDoctor_ShouldReturnForbid()
        {
            var loggedUserId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();

            var existingDoctor = new Doctor
            {
                Id = doctorId,
                UserId = otherUserId
            };

            var dto = new CreateDoctorDto
            {
                FullName = "Updated Doctor",
                Specialty = "Cardiology",
                Email = "doctor@test.com",
                PhoneNumber = "070123456"
            };

            _doctorRepo.Setup(r => r.GetByIdAsync(doctorId))
                .ReturnsAsync(existingDoctor);

            var controller = CreateController("Doctor", loggedUserId);

            var result = await controller.Update(doctorId, dto);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Update_WhenAdminUpdatesDoctor_ShouldReturnOk()
        {
            var adminId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();

            var existingDoctor = new Doctor
            {
                Id = doctorId,
                UserId = Guid.NewGuid()
            };

            var dto = new CreateDoctorDto
            {
                FullName = "Updated Doctor",
                Specialty = "Cardiology",
                Email = "doctor@test.com",
                PhoneNumber = "070123456"
            };

            var mappedDoctor = new Doctor
            {
                FullName = dto.FullName,
                Specialty = dto.Specialty,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber
            };

            _doctorRepo.Setup(r => r.GetByIdAsync(doctorId))
                .ReturnsAsync(existingDoctor);

            _mapper.Setup(m => m.Map<Doctor>(dto))
                .Returns(mappedDoctor);

            _doctorRepo.Setup(r => r.UpdateAsync(doctorId, mappedDoctor))
                .ReturnsAsync(true);

            var controller = CreateController("Admin", adminId);

            var result = await controller.Update(doctorId, dto);

            Assert.IsType<OkObjectResult>(result);
            mappedDoctor.UpdatedBy.Should().Be(adminId);
        }

        [Fact]
        public async Task Update_WhenRepositoryReturnsFalse_ShouldReturnNotFound()
        {
            var adminId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();

            var existingDoctor = new Doctor
            {
                Id = doctorId
            };

            var dto = new CreateDoctorDto
            {
                FullName = "Updated Doctor",
                Specialty = "Cardiology",
                Email = "doctor@test.com",
                PhoneNumber = "070123456"
            };

            var mappedDoctor = new Doctor();

            _doctorRepo.Setup(r => r.GetByIdAsync(doctorId))
                .ReturnsAsync(existingDoctor);

            _mapper.Setup(m => m.Map<Doctor>(dto))
                .Returns(mappedDoctor);

            _doctorRepo.Setup(r => r.UpdateAsync(doctorId, mappedDoctor))
                .ReturnsAsync(false);

            var controller = CreateController("Admin", adminId);

            var result = await controller.Update(doctorId, dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_WhenDoctorDoesNotExist_ShouldReturnNotFound()
        {
            var doctorId = Guid.NewGuid();

            _doctorRepo.Setup(r => r.DeleteAsync(doctorId))
                .ReturnsAsync(false);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.Delete(doctorId);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_WhenDoctorExists_ShouldReturnOk()
        {
            var doctorId = Guid.NewGuid();

            _doctorRepo.Setup(r => r.DeleteAsync(doctorId))
                .ReturnsAsync(true);

            var controller = CreateController("Admin", Guid.NewGuid());

            var result = await controller.Delete(doctorId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Search_ShouldReturnPagedDoctors()
        {
            int pageNumber = 0;
            int pageSize = 10;
            string specialty = "Cardiology";
            string searchText = "test";
            string sortField = "FullName";
            bool sortOrder = true;

            var doctors = new List<Doctor>
        {
            new Doctor { Id = Guid.NewGuid(), FullName = "Doctor 1" },
            new Doctor { Id = Guid.NewGuid(), FullName = "Doctor 2" }
        };

            var doctorDtos = new List<DoctorDto>
        {
            new DoctorDto(),
            new DoctorDto()
        };

            _doctorRepo.Setup(r => r.SearchAsync(
                    pageNumber,
                    pageSize,
                    specialty,
                    null,
                    null,
                    searchText,
                    sortField,
                    sortOrder))
                .ReturnsAsync(doctors);

            _doctorRepo.Setup(r => r.TotalCountDoctors(
                    specialty,
                    null,
                    null,
                    searchText))
                .ReturnsAsync(2);

            _mapper.Setup(m => m.Map<List<DoctorDto>>(doctors))
                .Returns(doctorDtos);

            var controller = CreateController("Patient", Guid.NewGuid());

            var result = await controller.Search(
                pageNumber,
                pageSize,
                specialty,
                null,
                null,
                searchText,
                sortField,
                sortOrder);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var pageModel = Assert.IsType<PageModel<DoctorDto>>(okResult.Value);

            pageModel.Items.Count.Should().Be(2);
            pageModel.PageNumber.Should().Be(0);
            pageModel.PageSize.Should().Be(10);
            pageModel.TotalItems.Should().Be(2);
            pageModel.TotalPages.Should().Be(1);
        }
    }
}
