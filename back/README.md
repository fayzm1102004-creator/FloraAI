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
- Redis Server (if you are running caching locally)

## ⚙️ Environment Variables & Configuration
To run this project, you need to configure the database connection. If you are running locally, update `appsettings.Development.json`. For deployment (e.g., Railway), set these as Environment Variables.

Important Keys:
* `ConnectionStrings:DefaultConnection` - The PostgreSQL connection URL.
* `JWT` settings (Secret Key, Issuer, Audience).
* `Redis` connection string.

Example `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FloraAIDb;Username=postgres;Password=yourpassword"
  }
}
```

## 🏗️ Installation & Running Locally

1. **Restore dependencies:**
   Open your terminal in the `back` directory and run:
   ```bash
   dotnet restore
   ```

2. **Apply Database Migrations:**
   Ensure your PostgreSQL server is running and the connection string is correct, then run:
   ```bash
   cd FloraAI.API
   dotnet ef database update
   ```

3. **Run the API:**
   ```bash
   dotnet run
   ```

4. **Swagger Documentation:**
   Once the API is running, you can test all endpoints by accessing the Swagger UI at:
   `https://localhost:<port>/swagger`

## ☁️ Deployment (Railway)
This project is structured for easy deployment on **Railway**.
1. In the Railway Dashboard, set the **Root Directory** to `/back`.
2. Ensure you have added your PostgreSQL `DATABASE_URL` (or equivalent variable used in your app) in the Railway Variables tab.
3. The included `Dockerfile` will be automatically detected and built.
