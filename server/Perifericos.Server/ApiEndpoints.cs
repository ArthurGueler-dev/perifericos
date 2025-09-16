using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Perifericos.Server.Data;
using Perifericos.Server.Hubs;
using Perifericos.Server.Models;

namespace Perifericos.Server;

public static class ApiEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/auth/login", ([FromBody] LoginRequest req, IConfiguration cfg) =>
        {
            // mock auth: qualquer email/pass gera token
            var claims = new[] { new Claim(ClaimTypes.Name, req.Email), new Claim(ClaimTypes.Role, "TI") };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: cfg["Jwt:Issuer"],
                audience: cfg["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return Results.Ok(new LoginResponse(jwt));
        });

        app.MapGet("/api/agents/{pcIdentifier}/authorized-devices", async (string pcIdentifier, DeviceService devices, CancellationToken ct) =>
        {
            var resp = await devices.GetAuthorizedDevicesAsync(pcIdentifier, ct);
            return Results.Ok(resp);
        });

        app.MapPost("/api/events", async ([FromBody] EventDto dto, DeviceService devices, AppDbContext db, IHubContext<AlertsHub> hub, CancellationToken ct) =>
        {
            var (saved, isAlert, isIncident) = await devices.StoreEventAsync(dto, ct);
            if (isAlert)
            {
                await hub.Clients.All.SendAsync("alert", new { saved.Id, saved.TimestampUtc, saved.PcIdentifier, saved.FriendlyName, saved.SerialNumber, saved.EventType }, ct);
            }
            return Results.Ok(new { saved.Id, isAlert, isIncident });
        });

        // CRUDs básicos (simplificados)
        app.MapGroup("/api/admin").RequireAuthorization().MapAdminCrud();
    }

    private static RouteGroupBuilder MapAdminCrud(this RouteGroupBuilder group)
    {
        group.MapGet("employees", async (AppDbContext db) => await db.Employees.ToListAsync());
        group.MapPost("employees", async (AppDbContext db, Employee e) => { db.Employees.Add(e); await db.SaveChangesAsync(); return e; });
        group.MapPut("employees/{id}", async (AppDbContext db, int id, Employee e) => { e.Id = id; db.Update(e); await db.SaveChangesAsync(); return e; });
        group.MapDelete("employees/{id}", async (AppDbContext db, int id) => { var e = await db.Employees.FindAsync(id); if (e==null) return Results.NotFound(); db.Remove(e); await db.SaveChangesAsync(); return Results.NoContent(); });

        group.MapGet("computers", async (AppDbContext db) => await db.Computers.Include(c=>c.Employee).ToListAsync());
        group.MapPost("computers", async (AppDbContext db, Computer c) => { db.Computers.Add(c); await db.SaveChangesAsync(); return c; });
        group.MapPut("computers/{id}", async (AppDbContext db, int id, Computer c) => { c.Id = id; db.Update(c); await db.SaveChangesAsync(); return c; });
        group.MapDelete("computers/{id}", async (AppDbContext db, int id) => { var c = await db.Computers.FindAsync(id); if (c==null) return Results.NotFound(); db.Remove(c); await db.SaveChangesAsync(); return Results.NoContent(); });

        group.MapGet("peripherals", async (AppDbContext db) => await db.Peripherals.ToListAsync());
        group.MapPost("peripherals", async (AppDbContext db, Peripheral p) => { db.Peripherals.Add(p); await db.SaveChangesAsync(); return p; });
        group.MapPut("peripherals/{id}", async (AppDbContext db, int id, Peripheral p) => { p.Id = id; db.Update(p); await db.SaveChangesAsync(); return p; });
        group.MapDelete("peripherals/{id}", async (AppDbContext db, int id) => { var p = await db.Peripherals.FindAsync(id); if (p==null) return Results.NotFound(); db.Remove(p); await db.SaveChangesAsync(); return Results.NoContent(); });

        group.MapGet("assignments", async (AppDbContext db) => await db.PeripheralAssignments.Include(a=>a.Peripheral).Include(a=>a.Computer).ToListAsync());
        group.MapPost("assignments", async (AppDbContext db, PeripheralAssignment a) => { db.PeripheralAssignments.Add(a); await db.SaveChangesAsync(); return a; });
        group.MapDelete("assignments/{id}", async (AppDbContext db, int id) => { var a = await db.PeripheralAssignments.FindAsync(id); if (a==null) return Results.NotFound(); db.Remove(a); await db.SaveChangesAsync(); return Results.NoContent(); });

        group.MapGet("events", async (AppDbContext db, int take = 200) => await db.DeviceEvents.OrderByDescending(e => e.TimestampUtc).Take(take).ToListAsync());
        group.MapDelete("events", async (AppDbContext db) => { 
            await db.Database.ExecuteSqlRawAsync("DELETE FROM DeviceEvents");
            return Results.Ok(new { message = "Todos os eventos foram removidos" });
        });
        return group;
    }
}



