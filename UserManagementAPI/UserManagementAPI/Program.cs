using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UserManagementAPI.Models;
using System.Text;
using MiddlewareError;
using MiddlewareLogging;

var builder = WebApplication.CreateBuilder(args);

// Key for token signature (save secure in production)
var jwtKey = builder.Configuration["Jwt:Key"];
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);


// In-Memory-Liste von Usern (Testdaten)
var users = new List<User>
{
    new User
    {
        Id = 1,
        FirstName = "Anna",
        LastName = "Müller",
        Email = "anna.mueller@firma.de",
        Address = "Hauptstraße 1, Berlin",
        Age = 34,
        Job = "HR Managerin"
    },
    new User
    {
        Id = 2,
        FirstName = "Max",
        LastName = "Mustermann",
        Email = "max.mustermann@firma.de",
        Address = "Musterweg 5, Hamburg",
        Age = 41,
        Job = "IT Administrator"
    }
};

// JWT-Authentifizierung registrieren
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // nur für lokale Tests
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});

builder.Services.AddAuthorization();

// Liste als Singleton-Service registrieren
builder.Services.AddSingleton<List<User>>(users);

// API & Swagger konfigurieren
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User Management API",
        Version = "v1",
        Description = "API zur Verwaltung von Benutzerdaten für HR und IT"
    });

    //add JWT Support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insert here your JWT token: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();
// Error-Handling Middleware ganz oben
app.UseMiddleware<ErrorHandlingMiddleware>();

// Swagger nur in Entwicklung
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Optional: HTTPS-Umleitung deaktivieren für lokale Tests
// app.UseHttpsRedirection();

app.UseMiddleware<ReqResLogger>();

app.UseAuthentication();    // must be placed before authorization!!!
// Routing aktivieren
app.UseRouting();

// Autorisierung (nach Routing)
app.UseAuthorization();

app.MapControllers();

app.Run();

