using DigitalniCjenik.Data;
using DigitalniCjenik.Security;
using DigitalniCjenik.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Primjer: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement{
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme{
                Reference = new Microsoft.OpenApi.Models.OpenApiReference{
                    Id = "Bearer",
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
                }
            },
            new string[]{}
        }
    });
});

// DbContext
builder.Services.AddDbContext<DigitalniCjenikContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("localDatabase"))
);

// AuthService
builder.Services.AddScoped<AuthService>();

// JWT settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

// LDAP Settings
builder.Services.Configure<LdapSettings>(
    builder.Configuration.GetSection("Ldap")
);

//  LDAP Service (Mock ili pravi)
var ldapSettings = builder.Configuration.GetSection("Ldap").Get<LdapSettings>();
if (ldapSettings?.UseMock == true)
{
    builder.Services.AddScoped<ILdapService, MockLdapService>();
    Console.WriteLine("Using MOCK LDAP service for development");
}
else
{
    // Ako nije mock, ipak koristi mock za razvoj
    builder.Services.AddScoped<ILdapService, MockLdapService>();
    Console.WriteLine("Using MOCK LDAP service for development");
}


// Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

        if (string.IsNullOrEmpty(jwt?.Key))
        {
            throw new InvalidOperationException("JWT Key must be provided in configuration.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.Key)
            )
        };
    });

// Authorization
builder.Services.AddAuthorization();

// CORS 
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();