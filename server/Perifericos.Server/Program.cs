using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Perifericos.Server;
using Perifericos.Server.Data;
using Perifericos.Server.Hubs;
using Perifericos.Server.Models;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/server-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Auth JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Permite token via query para SignalR
            var accessToken = context.Request.Query["access_token"].ToString();
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/alerts"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://ffperifericos.in9automacao.com.br", "http://ffperifericos.in9automacao.com.br")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Necessário para SignalR
    });
});

// App services
builder.Services.AddScoped<DeviceService>();

var app = builder.Build();

// Apply migrations dev-only
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Com SQLite e sem migrations, cria o schema automaticamente
    db.Database.EnsureCreated();
    if (app.Environment.IsDevelopment())
    {
        if (!db.Employees.Any())
        {
            var emp = new Employee { Name = "Alice Silva", Email = "alice@example.com" };
            var emp2 = new Employee { Name = "Bruno Souza", Email = "bruno@example.com" };
            db.Employees.AddRange(emp, emp2);
            db.SaveChanges();

            var pc1 = new Computer { Identifier = "PC-001", Hostname = "PC001", EmployeeId = emp.Id };
            var pc2 = new Computer { Identifier = "PC-002", Hostname = "PC002", EmployeeId = emp2.Id };
            db.Computers.AddRange(pc1, pc2);
            db.SaveChanges();

            var mouse = new Peripheral { VendorId = "046D", ProductId = "C534", SerialNumber = "SN-123", FriendlyName = "Logitech Mouse" };
            var teclado = new Peripheral { VendorId = "1A2B", ProductId = "3C4D", SerialNumber = "SN-987", FriendlyName = "Teclado USB" };
            db.Peripherals.AddRange(mouse, teclado);
            db.SaveChanges();

            db.PeripheralAssignments.Add(new PeripheralAssignment { PeripheralId = mouse.Id, ComputerId = pc1.Id });
            db.PeripheralAssignments.Add(new PeripheralAssignment { PeripheralId = teclado.Id, ComputerId = pc1.Id });
            db.SaveChanges();
        }
    }
}

// Habilita Swagger sempre para facilitar testes locais
app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();
app.UseRouting();
app.UseCors("AllowSpecificOrigins"); // Usar a policy específica
app.UseAuthentication();
app.UseAuthorization();

// Map hubs
app.MapHub<AlertsHub>("/hubs/alerts");

// Minimal APIs
ApiEndpoints.Map(app);

app.Run();


