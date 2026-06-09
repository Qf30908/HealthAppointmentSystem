using FluentAssertions;
using HealthAppointmentSystem.AUTH;
using HealthAppointmentSystem.Controllers;
using HealthAppointmentSystem.Data;
using HealthAppointmentSystem.DTOs.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class AdminControllerTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private AdminController CreateController(AppDbContext context, Guid adminId)
        {
            var controller = new AdminController(context);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
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
        public async Task CreateDoctor_WhenUserDoesNotExist_ShouldCreateDoctorAndReturnOk()
        {
            var context = GetDbContext();
            var adminId = Guid.NewGuid();
            var controller = CreateController(context, adminId);

            var dto = new CreateDoctorByAdminDto
            {
                FullName = "Dr. Test",
                Specialty = "Cardiology",
                Email = "doctor@test.com",
                Password = "Password123!",
                PhoneNumber = "070123456"
            };

            var result = await controller.CreateDoctor(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);

            context.Users.Count().Should().Be(1);
            context.Doctors.Count().Should().Be(1);

            var user = context.Users.First();
            var doctor = context.Doctors.First();

            user.Email.Should().Be(dto.Email);
            doctor.Email.Should().Be(dto.Email);
            doctor.FullName.Should().Be(dto.FullName);
            doctor.Specialty.Should().Be(dto.Specialty);
            doctor.PhoneNumber.Should().Be(dto.PhoneNumber);
            doctor.IsAvailable.Should().BeTrue();
            doctor.CreatedBy.Should().Be(adminId);
            doctor.UpdatedBy.Should().Be(adminId);

            BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task CreateDoctor_WhenUserAlreadyExists_ShouldReturnBadRequest()
        {
            var context = GetDbContext();

            context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = "doctor@test.com",
                PasswordHash = "hashed",
                Role = HealthAppointmentSystem.AUTH.Enums.UserRole.Doctor
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context, Guid.NewGuid());

            var dto = new CreateDoctorByAdminDto
            {
                FullName = "Dr. Test",
                Specialty = "Cardiology",
                Email = "doctor@test.com",
                Password = "Password123!",
                PhoneNumber = "070123456"
            };

            var result = await controller.CreateDoctor(dto);

            Assert.IsType<BadRequestObjectResult>(result);

            context.Users.Count().Should().Be(1);
            context.Doctors.Count().Should().Be(0);
        }

        [Fact]
        public async Task CreatePatient_WhenUserDoesNotExist_ShouldCreatePatientAndReturnOk()
        {
            var context = GetDbContext();
            var adminId = Guid.NewGuid();
            var controller = CreateController(context, adminId);

            var dto = new CreatePatientByAdminDto
            {
                FullName = "Patient Test",
                Email = "patient@test.com",
                Password = "Password123!",
                PhoneNumber = "071123456",
                DateOfBirth = new DateTime(2000, 1, 1)
            };

            var result = await controller.CreatePatient(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);

            context.Users.Count().Should().Be(1);
            context.Patients.Count().Should().Be(1);

            var user = context.Users.First();
            var patient = context.Patients.First();

            user.Email.Should().Be(dto.Email);
            patient.Email.Should().Be(dto.Email);
            patient.FullName.Should().Be(dto.FullName);
            patient.PhoneNumber.Should().Be(dto.PhoneNumber);
            patient.DateOfBirth.Should().Be(dto.DateOfBirth);
            patient.CreatedBy.Should().Be(adminId);
            patient.UpdatedBy.Should().Be(adminId);

            BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task CreatePatient_WhenUserAlreadyExists_ShouldReturnBadRequest()
        {
            var context = GetDbContext();

            context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = "patient@test.com",
                PasswordHash = "hashed",
                Role = HealthAppointmentSystem.AUTH.Enums.UserRole.Patient
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context, Guid.NewGuid());

            var dto = new CreatePatientByAdminDto
            {
                FullName = "Patient Test",
                Email = "patient@test.com",
                Password = "Password123!",
                PhoneNumber = "071123456",
                DateOfBirth = new DateTime(2000, 1, 1)
            };

            var result = await controller.CreatePatient(dto);

            Assert.IsType<BadRequestObjectResult>(result);

            context.Users.Count().Should().Be(1);
            context.Patients.Count().Should().Be(0);
        }

    }
}
