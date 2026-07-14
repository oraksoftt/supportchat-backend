
# 🚀 SupportChat SaaS - Backend

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoft-sql-server)](https://www.microsoft.com/en-us/sql-server/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-512BD4?logo=dotnet)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Features](#-features)
- [Project Structure](#-project-structure)
- [Database Schema](#-database-schema)
- [API Documentation](#-api-documentation)
- [SignalR Hub](#-signalr-hub)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Running the Application](#-running-the-application)
- [Security](#-security)
- [Deployment](#-deployment)
- [Contributing](#-contributing)
- [License](#-license)

---

## 📖 Overview

**SupportChat SaaS** is a production‑grade, multi‑tenant customer support chat system built with **.NET 9**, **ASP.NET Core Minimal APIs**, **Dapper**, and **SignalR**. It provides real‑time chat capabilities with a complete agent dashboard, customer widget, and admin management features.

### 🎯 Key Highlights

- ✅ **Multi‑Tenant Architecture** – Complete data isolation per company
- ✅ **Real‑Time Communication** – SignalR for instant messaging, typing indicators, and online status
- ✅ **JWT Authentication** – Secure token‑based authentication with refresh tokens
- ✅ **Role‑Based Access Control** – Admin, Agent, and Customer roles
- ✅ **Stored Procedure First** – All database operations via stored procedures for performance and security
- ✅ **API Versioning** – Future‑proof endpoints
- ✅ **Multi‑Language Support** – English and Arabic (RTL) ready
- ✅ **Email Notifications** – SMTP integration for offline alerts
- ✅ **Audit Logging** – Track all critical actions
- ✅ **Enterprise Ready** – Scalable, maintainable, and testable

---

## 🏗️ Architecture
┌─────────────────────────────────────────────────────────────────────────┐
│ Frontend (Next.js) │
└─────────────────────────────────────────────────────────────────────────┘
│
HTTPS + SignalR (WebSockets)
│
┌─────────────────────────────────────────────────────────────────────────┐
│ .NET 9 Backend │
│ ┌─────────────┐ ┌─────────────┐ ┌──────────────┐ ┌─────────────┐ │
│ │ Minimal │ │ SignalR │ │ Middleware │ │ Filters │ │
│ │ APIs │ │ Hub │ │ (Auth, │ │ & │ │
│ │ (Endpoints)│ │ (Real-time)│ │ Tenant) │ │ Validators│ │
│ └──────┬──────┘ └──────┬──────┘ └──────┬───────┘ └──────┬──────┘ │
│ │ │ │ │ │
│ ┌──────▼────────────────▼────────────────▼──────────────────▼──────┐ │
│ │ Service Layer (Business Logic) │ │
│ │ AuthService, ChatService, CompanyService, CustomerService, │ │
│ │ MessageService, NotificationService, DashboardService, etc. │ │
│ └────────────────────────────────────────────────────────────────────┘ │
│ │ │
│ ┌─────────────────────────────────▼────────────────────────────────┐ │
│ │ Repository Layer (Dapper) │ │
│ │ Execute stored procedures with parameterized queries │ │
│ └────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
│
TCP/IP (SQL Server)
│
┌─────────────────────────────────────────────────────────────────────────┐
│ SQL Server 2022 Database │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐ │
│ │ auth │ │ chat │ │ company │ │ customer │ │ system │ │
│ │ schema │ │ schema │ │ schema │ │ schema │ │ schema │ │
│ └──────────┘ └──────────┘ └──────────┘ └──────────┘ └────────┘ │
└─────────────────────────────────────────────────────────────────────────┘


### 🧩 Layered Architecture

| Layer | Responsibility |
|-------|---------------|
| **Endpoints (Minimal APIs)** | HTTP request handling, validation, response formatting |
| **SignalR Hub** | Real‑time WebSocket communication, group management |
| **Services** | Business logic, orchestration, validation, audit logging |
| **Repositories** | Data access via Dapper, stored procedure execution |
| **Database** | SQL Server 2022 with schemas and stored procedures |

---

## 🛠️ Tech Stack

### Backend

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 9.0 | Framework |
| **ASP.NET Core** | 9.0 | Web API & Minimal APIs |
| **Dapper** | 2.1.35 | Micro‑ORM |
| **SQL Server** | 2022 | Database |
| **SignalR** | 8.0.7 | Real‑time communication |
| **JWT** | - | Authentication |
| **BCrypt** | - | Password hashing |
| **Serilog** | 8.0.3 | Logging |

### Infrastructure

- **Database**: SQL Server 2022 (Stored Procedures)
- **Real‑time**: SignalR with WebSockets (scalable with Redis backplane)
- **CI/CD**: GitHub Actions
- **Hosting**: Azure / AWS / On‑Premise

---

## ✨ Features

### 🔐 Authentication & Authorization
- **JWT Access Token** – 30 minutes expiry
- **Refresh Token** – 30 days expiry with rotation
- **BCrypt Password Hashing** – Secure password storage
- **Role‑Based Access** – Admin, Agent, Customer
- **Permission‑Based Authorization** – Fine‑grained access control
- **Secure Logout** – Token revocation

### 💬 Real‑Time Chat System
- **Instant Messaging** – SignalR WebSockets
- **Typing Indicators** – Real‑time typing status
- **Chat Assignment** – Agent routing and assignment
- **Chat Transfer** – Transfer between agents
- **Chat History** – Persistent message storage
- **Rich Messages** – Text, system messages (file support ready)
- **Message Status** – Delivered, Seen (read receipts)

### 👥 Multi‑Tenancy
- **Complete Isolation** – CompanyId filtering in all queries
- **Company Management** – CRUD operations
- **Department Management** – Organizational structure
- **Agent Management** – User roles and permissions
- **Customer Management** – Block/unblock customers

### 📧 Notifications
- **Email Alerts** – Offline agent notifications
- **New Chat Alerts** – Unassigned chat notifications
- **Chat Assignment** – Agent assignment notifications
- **Email Templates** – Database‑driven templates
- **Queue System** – Background email processing

### 🌍 Localization
- **Multi‑Language** – English (default), Arabic
- **RTL Support** – Right‑to‑left layout ready
- **Resource Management** – Database‑driven translations
- **Extensible** – Easily add new languages

### 📊 Dashboard Analytics
- **Active Chats** – Real‑time count
- **Waiting Chats** – Queue length
- **Agent Performance** – Response time metrics
- **Customer Statistics** – Total customers, activity

### 🔍 Audit Logging
- **Action Tracking** – All CRUD operations
- **Entity Tracking** – Old/New value comparison
- **User Tracking** – Who performed each action
- **IP Address** – Request source tracking

---

## 📁 Project Structure
SupportChat.Backend/
├── 📄 Program.cs # Application entry point
├── 📄 appsettings.json # Configuration
├── 📄 appsettings.Development.json # Development config
├── 📄 SupportChat.Backend.csproj # Project file
│
├── 📁 Constants/
│ └── 📄 Enums.cs # ChatStatus, SenderType, Priority, etc.
│
├── 📁 Helpers/
│ ├── 📄 JwtHelper.cs # JWT generation & validation
│ ├── 📄 PasswordHelper.cs # BCrypt hashing
│ └── 📄 LocalizationHelper.cs # Translation helpers
│
├── 📁 Extensions/
│ ├── 📄 ServiceExtensions.cs # DI registration
│ ├── 📄 RepositoryExtensions.cs # Repository registration
│ └── 📄 EndpointExtensions.cs # Endpoint mapping
│
├── 📁 Middleware/
│ ├── 📄 ErrorHandlingMiddleware.cs # Global exception handling
│ ├── 📄 JwtMiddleware.cs # JWT validation
│ └── 📄 TenantMiddleware.cs # Tenant identification
│
├── 📁 Models/
│ ├── 📁 Domain/ # Database entities
│ │ ├── 📄 User.cs
│ │ ├── 📄 Company.cs
│ │ ├── 📄 Chat.cs
│ │ ├── 📄 Message.cs
│ │ ├── 📄 Customer.cs
│ │ ├── 📄 Department.cs
│ │ ├── 📄 Attachment.cs
│ │ └── 📄 Notification.cs
│ ├── 📁 Requests/ # API request DTOs
│ │ ├── 📄 AuthRequests.cs
│ │ ├── 📄 ChatRequests.cs
│ │ ├── 📄 MessageRequests.cs
│ │ └── 📄 CustomerRequests.cs
│ └── 📁 Responses/ # API response DTOs
│ ├── 📄 AuthResponses.cs
│ ├── 📄 ChatResponses.cs
│ └── 📄 MessageResponses.cs
│
├── 📁 Repositories/ # Data access layer
│ ├── 📁 Interfaces/
│ │ ├── 📄 IAuthRepository.cs
│ │ ├── 📄 ICompanyRepository.cs
│ │ ├── 📄 IChatRepository.cs
│ │ ├── 📄 IMessageRepository.cs
│ │ ├── 📄 ICustomerRepository.cs
│ │ ├── 📄 IDepartmentRepository.cs
│ │ ├── 📄 INotificationRepository.cs
│ │ └── 📄 IAuditRepository.cs
│ ├── 📄 BaseRepository.cs # Connection factory
│ ├── 📄 AuthRepository.cs
│ ├── 📄 CompanyRepository.cs
│ ├── 📄 ChatRepository.cs
│ ├── 📄 MessageRepository.cs
│ ├── 📄 CustomerRepository.cs
│ ├── 📄 DepartmentRepository.cs
│ ├── 📄 NotificationRepository.cs
│ └── 📄 AuditRepository.cs
│
├── 📁 Services/ # Business logic layer
│ ├── 📁 Interfaces/
│ │ ├── 📄 IAuthService.cs
│ │ ├── 📄 ICompanyService.cs
│ │ ├── 📄 IChatService.cs
│ │ ├── 📄 IMessageService.cs
│ │ ├── 📄 ICustomerService.cs
│ │ ├── 📄 IDepartmentService.cs
│ │ ├── 📄 INotificationService.cs
│ │ ├── 📄 IEmailService.cs
│ │ └── 📄 IDashboardService.cs
│ ├── 📄 AuthService.cs
│ ├── 📄 CompanyService.cs
│ ├── 📄 ChatService.cs
│ ├── 📄 MessageService.cs
│ ├── 📄 CustomerService.cs
│ ├── 📄 DepartmentService.cs
│ ├── 📄 NotificationService.cs
│ ├── 📄 EmailService.cs
│ └── 📄 DashboardService.cs
│
├── 📁 Hubs/
│ └── 📄 ChatHub.cs # SignalR hub
│
├── 📁 Endpoints/ # Minimal API endpoints
│ ├── 📄 AuthEndpoints.cs
│ ├── 📄 CompanyEndpoints.cs
│ ├── 📄 UserEndpoints.cs
│ ├── 📄 ChatEndpoints.cs
│ ├── 📄 MessageEndpoints.cs
│ ├── 📄 CustomerEndpoints.cs
│ ├── 📄 DepartmentEndpoints.cs
│ ├── 📄 NotificationEndpoints.cs
│ └── 📄 DashboardEndpoints.cs
│
├── 📁 BackgroundServices/ # Background workers
│ └── 📄 EmailQueueService.cs # Email processing
│
└── 📁 Properties/
└── 📄 launchSettings.json # Launch profiles

---

## 🗄️ Database Schema

### Schemas

| Schema | Purpose |
|--------|---------|
| `auth` | Authentication, Users, Roles, Permissions |
| `chat` | Chats, Messages, Attachments, Assignments |
| `company` | Companies, Departments, Settings |
| `customer` | Customers, Blocked Visitors |
| `notification` | Notifications, Email Templates, Queue |
| `system` | Languages, Localization, Audit Logs, System Settings |

### Key Tables

| Table | Description |
|-------|-------------|
| `auth.Users` | User accounts (agents, admins) |
| `auth.Roles` | Role definitions |
| `auth.Permissions` | Permission definitions |
| `auth.RefreshTokens` | Refresh token storage |
| `company.Companies` | Tenant/Company records |
| `company.Departments` | Department structure |
| `chat.Chats` | Chat sessions |
| `chat.Messages` | Chat messages |
| `chat.ChatAssignments` | Agent assignment history |
| `customer.Customers` | Customer profiles |
| `notification.Notifications` | User notifications |
| `system.AuditLogs` | Audit trail |
| `system.LocalizationResources` | Translation resources |

### Stored Procedures

All data access is through **stored procedures** for:
- ✅ Performance optimization
- ✅ Security (SQL injection prevention)
- ✅ Maintainability (database logic centralization)
- ✅ Transaction management



---

## 📡 API Documentation

### Base URL
https://api.yourdomain.com/api/v1

### Authentication Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/auth/login` | Authenticate and get JWT tokens |
| POST | `/auth/refresh` | Refresh access token |
| POST | `/auth/logout` | Logout and revoke refresh token |
| POST | `/auth/register` | Register new user (Admin only) |

### Chat Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/chats` | Get all chats for company |
| GET | `/chats/{id}` | Get chat details with messages |
| POST | `/chats` | Create a new chat |
| POST | `/chats/{id}/assign` | Assign agent to chat |
| POST | `/chats/{id}/close` | Close chat |
| POST | `/chats/{id}/transfer` | Transfer to another agent |

### Message Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/messages/chat/{chatId}` | Get chat messages |
| POST | `/messages` | Send a message |
| POST | `/messages/{id}/seen` | Mark message as seen |
| POST | `/messages/attachment` | Add attachment |

### Customer Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/customers` | Get company customers |
| GET | `/customers/{id}` | Get customer details |
| PUT | `/customers/{id}` | Update customer |
| POST | `/customers/{id}/block` | Block customer |
| POST | `/customers/{id}/unblock` | Unblock customer |

### Company Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/companies` | Get all companies (Admin) |
| GET | `/companies/{id}` | Get company details |
| POST | `/companies` | Create company |
| PUT | `/companies/{id}` | Update company |
| DELETE | `/companies/{id}` | Delete company |

### Department Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/departments` | Get company departments |
| GET | `/departments/{id}` | Get department details |
| POST | `/departments` | Create department |
| PUT | `/departments/{id}` | Update department |
| DELETE | `/departments/{id}` | Delete department |

### Dashboard Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/dashboard/stats` | Get dashboard statistics |

### Notification Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/notifications` | Get user notifications |
| POST | `/notifications/{id}/read` | Mark notification as read |

### Response Formats

#### Success Response
```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john@example.com"
}

#### Error Response
```json
{
  "error": "Invalid credentials",
  "detail": "Detailed error message (optional)"
}
```
### Installation
# Prerequisites
.NET 9 SDK

SQL Server 2022

Git

Visual Studio 2022 (optional)

# Step 1: Clone Repository
git clone https://github.com/yourusername/supportchat-backend.git
cd supportchat-backend

# Step 2: Configure Database
1.Run the database creation script (database.sql) to create:

Database: SupportChatDb
Schemas: auth, chat, company, customer, notification, system
Tables and stored procedures

2.Update connection string in appsettings.json:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SupportChatDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
# Step 3: Configure JWT
{
  "JwtSettings": {
    "Secret": "YourSuperLongSecretKeyAtLeast32CharactersLong!",
    "Issuer": "SupportChat",
    "Audience": "SupportChatUsers",
    "AccessTokenExpiryMinutes": 30,
    "RefreshTokenExpiryDays": 30
  }
}
# Step 4: Configure Email
{
  "EmailSettings": {
    "SmtpServer": "smtp.yourdomain.com",
    "SmtpPort": 587,
    "Username": "your-email@yourdomain.com",
    "Password": "YourEmailPassword",
    "From": "noreply@yourdomain.com"
  }
}

]
### 🔒 Security
Authentication Flow
User logs in with email/password

Server validates credentials and generates:

Access Token (JWT, 30 min expiry)

Refresh Token (random, 30 days expiry)

Access token sent in Authorization: Bearer {token} header

When access token expires, use refresh token to get new pair

Password Hashing
Algorithm: BCrypt with work factor 12

Salt: Automatically generated and stored in hash

Verification: BCrypt.Verify(password, hash)

### SQL Injection Prevention
✅ All database operations use stored procedures
✅ Parameterized queries with Dapper
✅ No dynamic SQL construction
### XSS Protection
✅ JWT tokens stored securely (HTTP‑only cookies recommended)
✅ Input validation on all endpoints
✅ Output encoding for API responses

### 🤝 Contributing
Fork the repository

Create feature branch (git checkout -b feature/amazing-feature)
Commit changes (git commit -m 'Add amazing feature')
Push to branch (git push origin feature/amazing-feature)

### Open Pull Request

### Development Guidelines
Follow C# Coding Conventions
Write unit tests for services
Document public APIs with XML comments
Update stored procedures in database scripts
Add audit logs for critical operations

### 📄 License
This project is licensed under the MIT License – see the LICENSE file for details.

### 🙏 Acknowledgments
.NET Team for the amazing framework
SignalR for real‑time communication
Dapper for efficient data access
BCrypt for secure password hashing
