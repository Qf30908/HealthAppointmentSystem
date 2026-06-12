# Health Appointment System

## Overview

Health Appointment System is a RESTful Web API developed using .NET 8 that enables healthcare appointment management between patients and doctors. The system provides authentication and authorization, appointment scheduling, medical record management, email notifications, and advanced search functionality.

## Features

### Authentication & Authorization

* JWT Authentication
* Role-based Authorization
* Supported Roles:

  * Admin
  * Doctor
  * Patient

### Doctor Management

* Create Doctor
* Update Doctor
* Delete Doctor
* Get Doctor Details
* Search Doctors
* Pagination, Sorting and Filtering

### Patient Management

* Create Patient
* Update Patient
* Delete Patient
* Get Patient Details

### Appointment Management

* Create Appointment
* Update Appointment Status
* Cancel Appointment
* View Appointments
* Doctor Availability Validation
* 30-minute Appointment Slots

### Medical Records

* Create Medical Record
* Update Medical Record
* Delete Medical Record
* View Patient Medical History

### Additional Features

* Email Notifications using Gmail SMTP
* Serilog Logging
* Unit Testing
* Repository Pattern
* AutoMapper
* DTO Pattern
* Dependency Injection

---

## Technologies Used

| Technology            | Version |
| --------------------- | ------- |
| .NET                  | 8.0     |
| ASP.NET Core Web API  | 8.0     |
| Entity Framework Core | 8.0     |
| SQL Server            | 8.0     |
| AutoMapper            | 12.0    |
| JWT Authentication    | 8.0     |
| Serilog               | 8.0     |
| Swagger/OpenAPI       | Latest  |
| xUnit                 | 2.5     |
| Moq                   | Latest  |

---

## System Architecture

The project follows a layered architecture:

* Controllers
* Services
* Repositories
* Data Layer
* DTOs
* AutoMapper Profiles
* Entity Models

---

## Database

Main entities:

* User
* Doctor
* Patient
* Appointment
* MedicalRecord

Relationships:

* One Doctor → Many Appointments
* One Patient → Many Appointments
* One Patient → Many Medical Records

---

## Installation

### 1. Clone Repository

```bash
git clone https://github.com/Qf30908/HealthAppointmentSystem.git
cd HealthAppointmentSystem
```

### 2. Configure User Secrets

Initialize secrets:

```bash
dotnet user-secrets init
```

Add JWT key:

```bash
dotnet user-secrets set "Jwt:Key" "YOUR_SECRET_KEY"
```

Add Email Settings:

```bash
dotnet user-secrets set "EmailSettings:Email" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "your-app-password"
```

### 3. Configure Database

Update connection string inside:

```json
ConnectionStrings:DefaultConnection
```

### 4. Apply Migrations

```bash
dotnet ef database update
```

### 5. Run Application

```bash
dotnet run
```

---

## Swagger

After running the application:

```
https://localhost:7131/swagger
```

Use Swagger UI to test all available endpoints.

---

## Logging

The application uses Serilog to log:

* API Requests
* API Responses
* Warnings
* Errors
* Exceptions

---

## Email Notifications

When a patient successfully creates an appointment, an email notification is sent containing:

* Doctor Name
* Appointment Date
* Appointment Time
* Appointment Duration

---

## Unit Tests

Unit tests are implemented using:

* xUnit
* Moq

Covered functionality includes:

* Repository Methods
* Controller Endpoints
* Business Logic Validation

Run tests:

```bash
dotnet test
```

---

## Project Structure

```text
HealthAppointmentSystem
│
├── Controllers
├── DTOs
├── Models
├── Repositories
├── Services
├── Data
├── Mappings
├── Middleware
├── Migrations
├── Tests
└── README.md
```

---

## Future Improvements

* SMS Notifications
* Mobile Application
* Online Payments
* Doctor Schedules
* Video Consultations
* Dashboard and Analytics

---

## Author

Qemal Fejzullai

South East European University

Service Oriented Architecture – Final Project
