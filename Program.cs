using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using WashUpAPIFix;

var builder = WebApplication.CreateBuilder(args);

// =============================
// 1. Configure Services
// =============================

//builder.Services.AddControllers()
//    .AddJsonOptions(x =>
//        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


// PostgreSQL DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ??
                      throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// Add controller support
builder.Services.AddControllers();

// JWT settings from appsettings.json
var jwtSettings = builder.Configuration.GetSection("Jwt");
string issuer = jwtSettings["Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer not configured");
string audience = jwtSettings["Audience"] ?? throw new ArgumentNullException("Jwt:Audience not configured");
string keyString = jwtSettings["Key"] ?? throw new ArgumentNullException("Jwt:Key not configured");

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // set to true on production
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = key
        };
    });

builder.Services.AddAuthorization();

// Swagger + JWT auth support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WashUp API",
        Version = "v1",
        Description = "API for WashUp Laundry Service"
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Masukkan token JWT seperti ini: Bearer {token}",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// =============================
// 2. Configure Middleware
// =============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts(); // Optional: for production HTTPS hardening
}

app.UseHttpsRedirection();
app.UseAuthentication();    // harus sebelum UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
