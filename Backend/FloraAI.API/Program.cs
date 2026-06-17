using FloraAI.API.Data;
using FloraAI.API.Services;
using FloraAI.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// SECRETS VALIDATION (Fail Fast)
// ============================================================================
var dbConn = builder.Configuration.GetConnectionString("DefaultConnection");
var geminiKey = builder.Configuration["Gemini:ApiKey"];
var jwtKey = builder.Configuration["Jwt:Key"] ?? builder.Configuration["JWT_KEY"];


if (string.IsNullOrWhiteSpace(dbConn) || dbConn == "REPLACE_VIA_ENV_VARIABLES")
    throw new InvalidOperationException("CRITICAL: Database connection string is missing or not configured.");
if (string.IsNullOrWhiteSpace(geminiKey) || geminiKey == "REPLACE_VIA_ENV_VARIABLES")
    throw new InvalidOperationException("CRITICAL: Gemini API Key is missing or not configured.");
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey == "REPLACE_VIA_ENV_VARIABLES")
    throw new InvalidOperationException("CRITICAL: JWT Key is missing or not configured.");

// ============================================================================
// 1. DATABASE CONFIGURATION - ApplicationDbContext with SQL Server
// ============================================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
    npgsqlOptions => npgsqlOptions.MigrationsAssembly("FloraAI.API")));

// ============================================================================
// 2. DEPENDENCY INJECTION - Service Registration
// ============================================================================
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IConditionService, ConditionService>();
builder.Services.AddScoped<IDiagnosisService, DiagnosisService>();
builder.Services.AddScoped<IGeminiService, GeminiService>();
builder.Services.AddScoped<IUserPlantService, UserPlantService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();

// ============================================================================
// 2.5 AUTHENTICATION & AUTHORIZATION CONFIGURATION
// ============================================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? builder.Configuration["JWT_KEY"] ?? "Fallback_Security_Key_For_Development_Only_Change_Immediately")),
        ClockSkew = TimeSpan.Zero

    };
    
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var blacklistService = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
            var jti = context.Principal?.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
            
            if (jti != null && await blacklistService.IsTokenBlacklistedAsync(jti))
            {
                context.Fail("تم تسجيل الخروج من هذه الجلسة.");
            }
        }
    };
});

// ============================================================================
// 2.6 REDIS DISTRIBUTED CACHE CONFIGURATION
// ============================================================================
builder.Services.AddStackExchangeRedisCache(options =>
{
    // Added abortConnect=false so the API won't crash on startup if Redis is down
    var redisConfig = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    options.Configuration = $"{redisConfig},abortConnect=false";
    options.InstanceName = "FloraAI_";
});

// HTTP Client for external API calls (Gemini) using TypedClient pattern with Polly Resilience
builder.Services.AddHttpClient<GeminiService>()
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30))
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError() // 5xx or 408
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                Console.WriteLine($"[Polly] Gemini API Retry {retryAttempt} after {timespan.TotalSeconds}s delay.");
            }))
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
        
builder.Services.AddScoped<IGeminiService>(sp => sp.GetRequiredService<GeminiService>());

// User Plant Library Management
builder.Services.AddScoped<IUserPlantService, UserPlantService>();

// Offline-First Sync
builder.Services.AddScoped<ISyncService, SyncService>();

// AutoMapper for Entity-DTO mapping
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<FloraAI.API.Mappings.MappingProfile>());

// Logging
builder.Services.AddLogging();

// ============================================================================
// 3. CORS POLICY - Allow Flutter Mobile App to Connect
// ============================================================================
const string corsPolicy = "AllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    // Read allowed origins from config, default to localhost for dev and a hypothetical Flutter web domain
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                         ?? new[] { "http://localhost:3000", "https://localhost:3001", "https://app.floraai.com" };
    
    options.AddPolicy(corsPolicy, policyBuilder =>
    {
        policyBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ============================================================================
// 4. SWAGGER/OPENAPI CONFIGURATION
// ============================================================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FloraAI Backend API",
        Version = "v1",
        Description = "Offline-First AI Plant Disease Detection API with Gemini Integration"
    });

    // Add JWT Bearer Security to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();

// ============================================================================
// 4. RATE LIMITING CONFIGURATION
// ============================================================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 500, // Temporarily increased for testing (was 100)
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("AuthLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 50, // Temporarily increased for testing (was 5)
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("DiagnosisLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100, // Temporarily increased for testing (was 20)
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

// ============================================================================
// 5. MIDDLEWARE PIPELINE - Proper Configuration Order
// ============================================================================

// Global Exception Handler (Stops Hiding Errors)
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var contextFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            var env = app.Services.GetRequiredService<IWebHostEnvironment>();
            var errorResponse = new 
            {
                message = env.IsDevelopment() ? contextFeature.Error.Message : "حدث خطأ غير متوقع في الخادم",
                details = env.IsDevelopment() ? contextFeature.Error.InnerException?.Message : null,
                stackTrace = env.IsDevelopment() ? contextFeature.Error.StackTrace : null
            };
            
            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    });
});

// Enable Swagger globally for testing on SmarterASP
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "FloraAI API v1");
    options.RoutePrefix = string.Empty; // Make Swagger open at the main URL directly
    options.DisplayOperationId();
});

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

// HTTPS Redirection
app.UseHttpsRedirection();

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// CORS Middleware
app.UseCors(corsPolicy);

// Authentication & Authorization
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Health Check Endpoint
app.MapGet("/health", () => Results.Ok(new { status = "API is running", timestamp = DateTime.UtcNow }))
    .WithName("GetHealth");

// ============================================================================
// 6. DATABASE INITIALIZATION - Ensure database exists and is created
// ============================================================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // On shared hosting, apply migrations safely. If no migrations exist, EnsureCreated acts as fallback.
        try
        {
            await dbContext.Database.MigrateAsync();
            
            // Temporary: Truncate tables to clear old format data on next startup
            await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ScanHistories\", \"ConditionsDictionary\" RESTART IDENTITY CASCADE;");
            
            Console.WriteLine("✓ تم تطبيق التحديثات وتطهير قاعدة البيانات (Postgres) بنجاح");
        }
        catch (InvalidOperationException)
        {
            // Thrown if no migrations are found in the assembly
            dbContext.Database.EnsureCreated();
            Console.WriteLine("✓ تم إنشاء الجداول باستخدام EnsureCreated");
        }
        
        // Verify tables were created
        var canConnect = dbContext.Database.CanConnect();
        if (canConnect)
        {
            Console.WriteLine("✓ تم توصيل قاعدة البيانات وتهيئتها بنجاح");
        }

        // ========================================================================
        // البيانات المدرجة - إضافة شروط نبات اختبار لاختبار API
        // ========================================================================
        var existingConditions = dbContext.ConditionsDictionary.Count();
        if (existingConditions == 0)
        {
            Console.WriteLine("📦 جاري إدراج شروط نبات الاختبار في قاعدة البيانات...");
            
            var seedConditions = new List<FloraAI.API.Models.Entities.ConditionsDictionary>
            {
                new()
                {
                    PlantType = "الورد",
                    ConditionName = "البياض الدقيقي",
                    Treatment = "رش مسحوق الكبريت أو زيت النيم. تأكد من تدوير الهواء الجيد. أزل الأوراق المصابة.",
                    WateringAdvice = "سقاية من القاعدة فقط وليس على الأوراق.",
                    LightAdvice = "توفير 6+ ساعات من أشعة الشمس.",
                    FertilizingAdvice = "تقليم الفروع الميتة والتسميد الدوري.",
                    SoilAdvice = "تجنب الازدحام لضمان تهوية التربة.",
                    HumidityAdvice = "تطبيق المبيدات الفطرية أسبوعياً إذا لزم الأمر.",
                    LastUpdated = DateTime.UtcNow
                },
                new()
                {
                    PlantType = "الطماطم",
                    ConditionName = "اللفحة المبكرة",
                    Treatment = "أزل الأوراق المصابة على الفور. ضع مبيد الفطريات النحاسي. حسّن تدوير الهواء.",
                    WateringAdvice = "سقاية من القاعدة فقط.",
                    LightAdvice = "توفير إضاءة شمسية قوية.",
                    FertilizingAdvice = "تطبيق المبيدات الفطرية كل 7-10 أيام.",
                    SoilAdvice = "تغطية التربة لحماية الجذور.",
                    HumidityAdvice = "حصاد الثمار الناضجة بسرعة.",
                    LastUpdated = DateTime.UtcNow
                },
                new()
                {
                    PlantType = "التفاح",
                    ConditionName = "الجرب",
                    Treatment = "قص الفروع المصابة. ضع مبيد الكبريت أو النحاس الفطري. أزل الأوراق الساقطة.",
                    WateringAdvice = "تنظيف الحطام المتساقط حول الشجرة.",
                    LightAdvice = "تأكد من وصول الشمس لقلب الشجرة.",
                    FertilizingAdvice = "تطبيق المبيدات الفطرية في الربيع.",
                    SoilAdvice = "التقليم الجيد لتدوير الهواء.",
                    HumidityAdvice = "إزالة الثمار المصابة فوراً.",
                    LastUpdated = DateTime.UtcNow
                },
                new()
                {
                    PlantType = "الريحان",
                    ConditionName = "بقعة الأوراق",
                    Treatment = "أزل الأوراق المصابة على الفور. تجنب السقاية من الأعلى. ضع زيت النيم إذا كان الوضع حاداً.",
                    WateringAdvice = "سقاية على مستوى التربة.",
                    LightAdvice = "تأكد من تدفق الهواء والضوء الجيد.",
                    FertilizingAdvice = "قرص براعم الأزهار لتقوية النبتة.",
                    SoilAdvice = "تجنب تراكم المياه الراكدة.",
                    HumidityAdvice = "حصاد الأوراق بانتظام.",
                    LastUpdated = DateTime.UtcNow
                },
                new()
                {
                    PlantType = "الخيار",
                    ConditionName = "البياض الدقيقي",
                    Treatment = "رش الكبريت أو كربونات البوتاسيوم. حسّن تدوير الهواء. أزل الأوراق المصابة.",
                    WateringAdvice = "سقاية في الصباح الباكر.",
                    LightAdvice = "توفير دعم الشبكة لرفع الأوراق عن الأرض.",
                    FertilizingAdvice = "تطبيق رش وقائي دوري.",
                    SoilAdvice = "تحسين صرف التربة.",
                    HumidityAdvice = "حصاد متكرر لمنع الإجهاد.",
                    LastUpdated = DateTime.UtcNow
                }
            };

            dbContext.ConditionsDictionary.AddRange(seedConditions);
            await dbContext.SaveChangesAsync();
            
            Console.WriteLine($"✓ تم إدراج {seedConditions.Count} شروط نبات بنجاح");
        }
        else
        {
            Console.WriteLine($"✓ قاعدة البيانات تحتوي بالفعل على {existingConditions} شرط");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Database initialization error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

// Launch Application
app.Run();

public partial class Program { }





