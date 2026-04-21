using FloraAI.API.Data;
using FloraAI.API.Services;
using FloraAI.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// 1. DATABASE CONFIGURATION - ApplicationDbContext with SQL Server
// ============================================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions
            .MigrationsAssembly("FloraAI.API")
            .EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
            )
    )
);

// ============================================================================
// 2. DEPENDENCY INJECTION - Register all Services and Interfaces
// ============================================================================

// HTTP Client for external API calls (Gemini) using TypedClient pattern
builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddScoped<IGeminiService>(sp => sp.GetRequiredService<GeminiService>());

// Authentication & User Management
builder.Services.AddScoped<IUserService, UserService>();

// Plant Diagnosis & AI Integration
builder.Services.AddScoped<IConditionService, ConditionService>();
builder.Services.AddScoped<IDiagnosisService, DiagnosisService>();

// User Plant Library Management
builder.Services.AddScoped<IUserPlantService, UserPlantService>();

// Offline-First Sync
builder.Services.AddScoped<ISyncService, SyncService>();

// AutoMapper for Entity-DTO mapping
builder.Services.AddAutoMapper(typeof(FloraAI.API.Mappings.MappingProfile));

// Logging
builder.Services.AddLogging();

// ============================================================================
// 3. CORS POLICY - Allow Flutter Mobile App to Connect
// ============================================================================
const string corsPolicy = "AllowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policyBuilder =>
    {
        policyBuilder
            .AllowAnyOrigin()
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
});

builder.Services.AddControllers();

var app = builder.Build();

// ============================================================================
// 5. MIDDLEWARE PIPELINE - Proper Configuration Order
// ============================================================================

// Enable Swagger in Development only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FloraAI API v1");
        options.RoutePrefix = "swagger";
        options.DisplayOperationId();
    });
}

// HTTPS Redirection (commented out for local development, uncomment in production)
// app.UseHttpsRedirection();

// CORS Middleware
app.UseCors(corsPolicy);

// Authentication & Authorization
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
        // Drop database if it exists (useful for development)
        // Uncomment if you need to reset the database
        // dbContext.Database.EnsureDeleted();
        
        // Create database and all tables from DbContext configuration
        dbContext.Database.EnsureCreated();
        
        // Verify tables were created
        var canConnect = dbContext.Database.CanConnect();
        if (canConnect)
        {
            Console.WriteLine("✓ تم توصيل قاعدة البيانات وتهيئتها بنجاح");
            Console.WriteLine("✓ تم إنشاء جميع الجداول من تكوين DbContext");
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
                    CareInstructions = "1. سقاية من القاعدة فقط وليس على الأوراق\n2. توفير 6+ ساعات من أشعة الشمس\n3. تقليم الفروع الميتة\n4. تطبيق المبيدات الفطرية أسبوعياً إذا لزم الأمر\n5. تجنب الازدحام",
                    LastUpdated = DateTime.UtcNow
                },
                new()
                {
                    PlantType = "الطماطم",
                    ConditionName = "اللفحة المبكرة",
                    Treatment = "أزل الأوراق المصابة على الفور. ضع مبيد الفطريات النحاسي. حسّن تدوير الهواء.",
                    CareInstructions = "1. سقاية من القاعدة فقط\n2. إزالة الأوراق السفلية\n3. تطبيق المبيدات الفطرية كل 7-10 أيام\n4. تغطية التربة\n5. حصاد الثمار الناضجة بسرعة",
                    LastUpdated = DateTime.UtcNow
                },
                new()
                {
                    PlantType = "التفاح",
                    ConditionName = "الجرب",
                    Treatment = "قص الفروع المصابة. ضع مبيد الكبريت أو النحاس الفطري. أزل الأوراق الساقطة.",
                    CareInstructions = "1. تقليل عناقيد الثمار\n2. تطبيق المبيدات الفطرية في الربيع\n3. إزالة الثمار المصابة\n4. التقليم لتدوير الهواء\n5. تنظيف الحطام المتساقط",
                    LastUpdated = DateTime.UtcNow
                },
                new()
                {
                    PlantType = "الريحان",
                    ConditionName = "بقعة الأوراق",
                    Treatment = "أزل الأوراق المصابة على الفور. تجنب السقاية من الأعلى. ضع زيت النيم إذا كان الوضع حاداً.",
                    CareInstructions = "1. سقاية على مستوى التربة\n2. تأكد من تدفق الهواء الجيد\n3. إزالة الأوراق التالفة\n4. قرص براعم الأزهار\n5. حصاد بانتظام",
                    LastUpdated = DateTime.UtcNow
                },
                new()
                {
                    PlantType = "الخيار",
                    ConditionName = "البياض الدقيقي",
                    Treatment = "رش الكبريت أو كربونات البوتاسيوم. حسّن تدوير الهواء. أزل الأوراق المصابة.",
                    CareInstructions = "1. سقاية في الصباح الباكر\n2. توفير دعم الشبكة\n3. إزالة الأوراق القديمة\n4. تطبيق رش وقائي\n5. حصاد متكرر",
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





