# 🔓 Alohomora

**Simple and magical SSO** — a pluggable authentication & identity library for .NET 10.

> *"Alohomora"* — The Unlocking Charm. Just like the spell, this library opens the door to full-featured identity management without locking you into a monolithic framework.

[![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

---

## ✨ Features

- **JWT-based authentication** — robust access and refresh token lifecycle management via `TokenService`.
- **Multi-factor Ready** — supports both Password and **OTP** (SMS/Email) via `AuthenticationType`.
- **Attribute-based Authorization** — `[AlohomoraAuth]` with dynamic `AlohomoraAuthorizationPolicyProvider`, endpoint scanning, and custom requirement handlers.
- **Clean-Architecture Ready** — strictly decoupled: Domain $\rightarrow$ DataAccess (Contracts) $\rightarrow$ Services $\rightarrow$ Implementation Providers.
- **Pluggable Persistence** — Core is database-agnostic. Use the provided `Alohomora.Sql` (EF Core/SQL Server) or implement `IUnitOfWork` for any DB.
- **Pluggable SMS** — `ISmsService` abstraction with a built-in **PayamResan** gateway provider.
- **Developer-Friendly Utilities** — includes Persian DateTime support (`MD.PersianDateTime`), localized error messages (`.resx`), and robust DTO/Response wrappers.
- **Caching Integrated** — utilizes `EasyCaching` to optimize token and OTP flows.

---

## 🧱 Solution Structure

| Project | Purpose |
|---|---|
| `Alohomora` | **Core**: Domain models, service interfaces, auth attributes, and common utilities. |
| `Alohomora.Sql` | **Persistence**: EF Core implementation of repositories and Unit of Work. |
| `Alohomora.PayamResan` | **SMS Provider**: Adapter for the PayamResan gateway. |
| `Alohomora.TestApi` | **Demo**: A sample Web API showcasing full integration. |

---

## 🚀 Quick Start

### Prerequisites
- .NET 10 SDK
- SQL Server (if using `Alohomora.Sql`)

### Installation
Clone the repository:
```bash
git clone https://github.com/MmHamzeh/Alohomora.git
```

### Dependency Injection
In your `Program.cs`, wire up the services with a single line for each layer:

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Core Identity & Auth
builder.Services.AddAlohomora(builder.Configuration);     

// 2. Data Persistence (SQL Server)
builder.Services.AddAlohomoraSql(builder.Configuration);  

// 3. SMS Provider (Optional)
builder.Services.AddAlohomoraPayamResan(builder.Configuration); 

builder.Services.AddControllers();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

### Using Authorization
Protect your endpoints with the custom attribute:

```csharp
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    [HttpGet]
    [AlohomoraAuth(Policy = "AdminOnly")] // Uses the dynamic policy provider
    public IActionResult GetSensitiveData() 
    {
        return Ok("🔓 Door Unlocked!");
    }
}
```

---

## 🧩 Extensibility

Alohomora is designed to be "unlocked" and customized. You can swap any provider by implementing its interface:

| Component | Interface | Provided Implementation |
|---|---|---|
| **Database** | `IUnitOfWork` / `IRepository` | `Alohomora.Sql` (EF Core) |
| **SMS Gateway** | `ISmsService` | `Alohomora.PayamResan` |
| **Caching** | `ICache` (EasyCaching) | Configurable (Redis, Memory, etc.) |

---

## 🛠️ Technical Stack
- **Framework:** .NET 10
- **ORM:** Entity Framework Core
- **Auth:** JWT, ASP.NET Core Identity Concepts
- **Communication:** PayamResan (SMS)
- **Localization:** Multi-language `.resx` support

## 📄 License
Distributed under the MIT License. See `LICENSE` for more information.

---
*Developed by [MmHamzeh](https://github.com/MmHamzeh)*

***

