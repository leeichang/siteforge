using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SqlSugar;
using SiteForge.Api.Middleware;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Services;
using SiteForge.Core.Utilities;
using SiteForge.Infrastructure.Data;
using SiteForge.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SiteForge API",
        Version = "v1",
        Description = "Backend API for the SiteForge website builder MVP.",
        Contact = new OpenApiContact
        {
            Name = "SiteForge"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste a JWT access token from /api/auth/login or /api/auth/register."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("SiteForgeCors", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
            .GetChildren()
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToArray();

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is required.");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<AppDbContext>();
builder.Services.AddScoped<ISqlSugarClient>(sp => sp.GetRequiredService<AppDbContext>().CreateClient());

builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddSingleton<PasswordHelper>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddScoped<RUserRepository, UserRepository>();
builder.Services.AddScoped<RSiteRepository, SiteRepository>();
builder.Services.AddScoped<RPageRepository, PageRepository>();
builder.Services.AddScoped<RWidgetTemplateRepository, WidgetTemplateRepository>();
builder.Services.AddScoped<RWidgetBaseRepository, WidgetBaseRepository>();
builder.Services.AddScoped<RThemeRepository, ThemeRepository>();
builder.Services.AddScoped<RLayoutRepository, LayoutRepository>();
builder.Services.AddScoped<RLayoutZoneRepository, LayoutZoneRepository>();
builder.Services.AddScoped<RSiteDomainRepository, SiteDomainRepository>();
builder.Services.AddScoped<RAiConversationRepository, AiConversationRepository>();
builder.Services.AddScoped<RAiMessageRepository, AiMessageRepository>();
builder.Services.AddScoped<RAssetRepository, AssetRepository>();
builder.Services.AddScoped<RPublishTaskRepository, PublishTaskRepository>();

builder.Services.AddScoped<AuthService, AuthServiceImpl>();
builder.Services.AddScoped<SiteService, SiteServiceImpl>();
builder.Services.AddScoped<PageService, PageServiceImpl>();
builder.Services.AddScoped<WidgetService, WidgetServiceImpl>();
builder.Services.AddScoped<WidgetTemplateService, WidgetTemplateServiceImpl>();
builder.Services.AddScoped<ThemeService, ThemeServiceImpl>();
builder.Services.AddScoped<LayoutService, LayoutServiceImpl>();
builder.Services.AddScoped<DomainService, DomainServiceImpl>();
builder.Services.AddScoped<AssetService, AssetServiceImpl>();
builder.Services.AddScoped<AiConversationService, AiConversationServiceImpl>();
builder.Services.AddScoped<PublishTaskService, PublishTaskServiceImpl>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

var swaggerEnabled = bool.TryParse(app.Configuration["Swagger:Enabled"], out var enabled) && enabled;
if (app.Environment.IsDevelopment() || swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SiteForge API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "SiteForge API Docs";
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
    });
}

app.UseCors("SiteForgeCors");
app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.MapFallbackToFile("index.html");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.InitializeDatabaseAsync();
}

await app.RunAsync();
