using Infrastructure.Data;
using Infrastructure.Services;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Data;
using Npgsql;
using Infrastructure; // para UnitOfWork

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext (solo si lo necesitas en paralelo a Dapper, para migraciones/EF)
builder.Services.AddDbContext<DotnetDapperJwtDbContext>(options =>
{
    string connectionString  = builder.Configuration.GetConnectionString("DefaultConnection")!;
    options.UseNpgsql(connectionString);
});

// Registrar conexión IDbConnection (Dapper)
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new NpgsqlConnection(config.GetConnectionString("DefaultConnection"));
});

// Registrar UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Registrar AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// JWT Auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)
        )
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();