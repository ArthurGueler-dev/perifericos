Agente Local - Periféricos

Pré-requisitos
- Windows 10/11 x64
- .NET 8 Desktop Runtime

Configuração
1. Edite `appsettings.json` e ajuste `Agent.ApiBaseUrl`, `Agent.PcIdentifier`, `Agent.AuthToken`.

Executar (modo console)
```powershell
cd agent/Perifericos.Agent
dotnet run --project .
```

Instalar como Serviço
```powershell
cd agent/Perifericos.Agent
dotnet publish -c Release -r win-x64 --self-contained false -o out
sc create PerifericosAgent binPath= "%cd%\out\Perifericos.Agent.exe" start= auto
sc start PerifericosAgent
```

O que faz
- Monitora USB via WMI
- Consulta autorizados no servidor
- Envia eventos à API
- Pop-up para não autorizados


