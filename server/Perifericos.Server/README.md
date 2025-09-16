Servidor Central - ASP.NET Core

Setup
- Requer: PostgreSQL 14+
- Ajuste `appsettings.json` connection string e JWT Key.

Comandos
```powershell
cd server/Perifericos.Server
dotnet ef database update
dotnet run
```

Swagger: `http://localhost:5099/swagger`

Rotas principais
- POST `/api/auth/login` (mock) → token JWT
- GET `/api/agents/{pcIdentifier}/authorized-devices`
- POST `/api/events` → grava evento e dispara SignalR `hubs/alerts`
- CRUD `/api/admin/*` autenticado


