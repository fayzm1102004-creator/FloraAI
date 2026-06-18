# FloraAI Backend API

This is the backend API for the FloraAI application, built using **.NET 10 (C#)**.

## 🚀 Technologies Used
- **Framework:** .NET 10.0 ASP.NET Core Web API
- **Database:** PostgreSQL (via `Npgsql.EntityFrameworkCore.PostgreSQL`)
- **ORM:** Entity Framework Core 10
- **Caching:** Redis (`Microsoft.Extensions.Caching.StackExchangeRedis`)
- **Authentication:** JWT (JSON Web Tokens)
- **Security:** Password Hashing (BCrypt.Net-Next)
- **Object Mapping:** AutoMapper
- **Documentation:** Swagger / OpenAPI

## 🛠️ Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- PostgreSQL Server
- Redis Server (optional, for caching)

## ⚙️ Environment Variables & Configuration
To run this project, you need to configure the database connection. If you are running locally, update `appsettings.Development.json`. For deployment (e.g., Railway), set these as Environment Variables.

Example `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FloraAIDb;Username=postgres;Password=yourpassword"
  }
}
```

## 🏗️ Installation & Running Locally (Step-by-Step)

To run the backend on your local machine, open your terminal and follow these exact steps:

**1. Navigate to the backend directory:**
```bash
cd back
```

**2. Restore the required .NET packages:**
```bash
dotnet restore
```

**3. Navigate into the API project folder:**
```bash
cd FloraAI.API
```

**4. Apply the database migrations:**
Ensure your PostgreSQL server is running, then run:
```bash
dotnet ef database update
```

**5. Run the API:**
```bash
dotnet run
```

Once the server is running, you can test all endpoints by accessing the Swagger UI at:
`https://localhost:<port>/swagger`

## ☁️ Deployment (Railway)
This project is structured for easy deployment on **Railway**.
1. In the Railway Dashboard, set the **Root Directory** to `/back`.
2. Ensure you have added your PostgreSQL `DATABASE_URL` (or equivalent variable used in your app) in the Railway Variables tab.
3. The included `Dockerfile` will be automatically detected and built.
