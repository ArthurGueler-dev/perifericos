# Sistema de Monitoramento de Periféricos

Sistema completo para monitoramento e controle de periféricos USB em ambiente corporativo.

## 🏗️ Arquitetura

- **Agent (C# .NET 8)**: Serviço Windows que monitora conexões USB
- **Server (ASP.NET Core)**: API REST + SignalR para gerenciamento central
- **Dashboard (React + TailwindCSS)**: Interface web para equipe de TI

## 🚀 Acesso Rápido (Produção)

- **Dashboard**: https://ti.in9automacao.com.br
- **API**: https://api.in9automacao.com.br/swagger
- **Login**: qualquer email/senha (JWT mock para desenvolvimento)

## 📋 Funcionalidades

### Agent Windows
- Monitora conexões/desconexões USB em tempo real
- Pop-up para dispositivos não autorizados
- Envia eventos para servidor central
- Executa como serviço Windows

### Dashboard Web
- Login para equipe de TI
- CRUD de Colaboradores, PCs e Periféricos
- Vinculação de periféricos autorizados por PC
- Alertas em tempo real via SignalR
- Relatórios de eventos com export PDF/Excel
- Dashboard com métricas

### API Central
- Autenticação JWT
- Endpoints REST para todas operações
- SignalR Hub para notificações em tempo real
- Banco SQLite (fácil para desenvolvimento)

## 🛠️ Desenvolvimento Local

### Pré-requisitos
- .NET 8 SDK
- Node.js 18+
- Git

### 1. Clonar e Executar

```powershell
git clone https://github.com/SEU_USUARIO/perifericos.git
cd perifericos

# Terminal 1 - API
cd server/Perifericos.Server
dotnet run --urls http://localhost:5099

# Terminal 2 - Dashboard  
cd dashboard
npm install
npm run dev

# Terminal 3 - Agent (opcional)
cd agent/Perifericos.Agent
dotnet run
```

### 2. Acessar
- Dashboard: http://localhost:5173
- API: http://localhost:5099/swagger

## 🌐 Deploy VPS (Hostinger + Cloudflare)

### Pré-requisitos
- VPS Ubuntu 22.04
- Domínios configurados no Cloudflare:
  - `api.in9automacao.com.br` → IP da VPS
  - `ti.in9automacao.com.br` → IP da VPS

### Deploy Automático

```bash
# SSH na VPS
ssh root@SEU_IP_VPS

# Clone do repositório
git clone https://github.com/SEU_USUARIO/perifericos.git
cd perifericos/deploy

# Executar deploy
chmod +x deploy.sh
sudo ./deploy.sh

# Configurar SSL
sudo certbot --nginx -d api.in9automacao.com.br -d ti.in9automacao.com.br
```

### Configuração Cloudflare
1. SSL/TLS → **Full (strict)**
2. Ativar **Always Use HTTPS**
3. Proxy ativado (nuvem laranja)

## 📦 Instalar Agent nos PCs

```powershell
# Em cada PC Windows
cd agent/Perifericos.Agent

# Editar appsettings.json:
# - ApiBaseUrl: "https://api.in9automacao.com.br"  
# - PcIdentifier: "PC-SETOR-01" (único por PC)

# Publicar e instalar como serviço
dotnet publish -c Release -r win-x64 --self-contained false -o out
sc create PerifericosAgent binPath= "%cd%\out\Perifericos.Agent.exe" start= auto
sc start PerifericosAgent
```

## 🔧 Comandos Úteis

### VPS
```bash
# Logs da API
sudo journalctl -u perifericos-api -f

# Reiniciar serviços
sudo systemctl restart perifericos-api
sudo systemctl restart nginx

# Status
sudo systemctl status perifericos-api
```

### Windows Agent
```powershell
# Ver status do serviço
sc query PerifericosAgent

# Parar/iniciar
sc stop PerifericosAgent
sc start PerifericosAgent

# Ver logs
Get-EventLog -LogName Application -Source "Perifericos.Agent"
```

## 🎯 Como Usar

1. **Configuração Inicial**:
   - Acesse https://ti.in9automacao.com.br
   - Cadastre colaboradores e PCs
   - Registre periféricos autorizados
   - Vincule periféricos aos PCs corretos

2. **Monitoramento**:
   - Instale o Agent nos PCs
   - Conecte/desconecte USBs para testar
   - Veja alertas em tempo real no dashboard
   - Gere relatórios de auditoria

3. **Alertas**:
   - USB não autorizado → Pop-up no PC + alerta no dashboard
   - USB autorizado → Apenas evento registrado
   - Mudança de PC → Incidente registrado

## 📁 Estrutura do Projeto

```
perifericos/
├── agent/                 # Serviço Windows C#
├── server/                # API ASP.NET Core  
├── dashboard/             # Frontend React
├── deploy/                # Scripts de deploy
└── README.md
```

## 🔒 Segurança

- JWT para autenticação
- CORS configurado
- HTTPS obrigatório em produção
- Logs auditáveis
- Validação de entrada

## 📈 Roadmap

- [ ] Integração Active Directory
- [ ] Bloqueio automático de USBs
- [ ] App mobile para alertas
- [ ] Dashboard avançado com BI
- [ ] Suporte a outros tipos de dispositivos

## 🤝 Contribuição

1. Fork o projeto
2. Crie sua feature branch (`git checkout -b feature/nova-funcionalidade`)
3. Commit suas mudanças (`git commit -am 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para detalhes.

---

**Desenvolvido para monitoramento corporativo de periféricos USB**