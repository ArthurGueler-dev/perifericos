# Deploy VPS - Passo a Passo Completo

## 🎯 Objetivo
Subir o sistema completo na VPS Hostinger com domínios Cloudflare já configurados.

## ✅ Pré-requisitos (JÁ FEITO)
- ✅ VPS Hostinger ativa
- ✅ Domínios configurados no Cloudflare:
  - `api.in9automacao.com.br` → `31.97.169.36`
  - `ti.in9automacao.com.br` → `31.97.169.36`
- ✅ Proxy Cloudflare ativo (nuvem laranja)

## 📤 1. Subir Código no GitHub (5 minutos)

### No seu PC Windows:
```powershell
cd C:\Users\SAMSUNG\Desktop\perifericos

# Inicializar git (se não foi feito)
git init
git add .
git commit -m "Sistema completo de monitoramento de periféricos"

# Criar repositório no GitHub e conectar
git remote add origin https://github.com/SEU_USUARIO/perifericos.git
git branch -M main
git push -u origin main
```

## 🖥️ 2. Conectar na VPS (2 minutos)

```bash
# SSH na VPS Hostinger
ssh root@31.97.169.36

# Ou via painel Hostinger → Terminal
```

## 📥 3. Baixar Código na VPS (3 minutos)

```bash
# Na VPS
cd /root
git clone https://github.com/SEU_USUARIO/perifericos.git
cd perifericos
```

## 🚀 4. Deploy Automático (15 minutos)

```bash
# Na VPS, dentro da pasta perifericos
cd deploy
chmod +x deploy.sh
sudo ./deploy.sh
```

**O script vai fazer automaticamente:**
- ✅ Instalar .NET 8 SDK
- ✅ Instalar Nginx
- ✅ Publicar a API em `/var/www/perifericos-api`
- ✅ Buildar o Dashboard em `/var/www/perifericos-dashboard`
- ✅ Configurar Nginx para os domínios
- ✅ Criar serviço systemd para a API
- ✅ Iniciar todos os serviços

## 🔒 5. Configurar SSL (5 minutos)

```bash
# Na VPS
sudo apt-get install -y certbot python3-certbot-nginx
sudo certbot --nginx -d api.in9automacao.com.br -d ti.in9automacao.com.br

# Aceitar os termos e fornecer email quando solicitado
```

## ☁️ 6. Configurar Cloudflare SSL (2 minutos)

1. Acesse painel Cloudflare
2. Vá em **SSL/TLS** → **Overview**
3. Defina como **"Full (strict)"**
4. Vá em **SSL/TLS** → **Edge Certificates**
5. Ative **"Always Use HTTPS"**

## ✅ 7. Testar Sistema (3 minutos)

### Testar API:
```bash
curl https://api.in9automacao.com.br/swagger
```

### Testar Dashboard:
- Abra: https://ti.in9automacao.com.br
- Login: qualquer email/senha
- Deve carregar o dashboard

### Testar Login:
- Email: `admin@teste.com`
- Senha: `123456`
- Deve logar e mostrar dashboard

## 🖥️ 8. Instalar Agent nos PCs (por PC)

### Em cada PC Windows que vai monitorar:

```powershell
# 1. Baixar o código (se não tiver)
git clone https://github.com/SEU_USUARIO/perifericos.git
cd perifericos\agent\Perifericos.Agent

# 2. Editar configuração
# Abrir appsettings.json e alterar:
# - PcIdentifier: "PC-FINANCEIRO-01" (nome único)
# - ApiBaseUrl já está correto: "https://api.in9automacao.com.br"

# 3. Publicar e instalar
dotnet publish -c Release -r win-x64 --self-contained false -o out
sc create PerifericosAgent binPath= "%cd%\out\Perifericos.Agent.exe" start= auto
sc start PerifericosAgent
```

## 🎯 9. Usar o Sistema

### Dashboard TI:
1. Acesse: https://ti.in9automacao.com.br
2. Faça login com qualquer email/senha
3. Cadastre **Colaboradores** (nome + email)
4. Cadastre **Computadores** (identificador + colaborador)
5. Cadastre **Periféricos** (VendorID + ProductID + nome)
6. **Vincule** periféricos aos computadores corretos
7. Monitore **Eventos** em tempo real

### Teste USB:
1. Conecte um USB em qualquer PC com agent
2. Se não autorizado: pop-up no PC + alerta no dashboard
3. Se autorizado: apenas evento normal

## 🔧 Comandos Úteis

### VPS - Monitorar logs:
```bash
# Logs da API
sudo journalctl -u perifericos-api -f

# Logs do Nginx
sudo tail -f /var/log/nginx/error.log

# Status dos serviços
sudo systemctl status perifericos-api
sudo systemctl status nginx
```

### VPS - Reiniciar serviços:
```bash
sudo systemctl restart perifericos-api
sudo systemctl restart nginx
```

### PC - Gerenciar Agent:
```powershell
# Status
sc query PerifericosAgent

# Reiniciar
sc stop PerifericosAgent
sc start PerifericosAgent

# Logs (Event Viewer)
Get-EventLog -LogName Application -Source "Perifericos.Agent" -Newest 10
```

## 🚨 Troubleshooting

### API não responde:
```bash
sudo systemctl status perifericos-api
sudo journalctl -u perifericos-api --no-pager -l
```

### Dashboard não carrega:
```bash
sudo nginx -t
sudo systemctl status nginx
ls -la /var/www/perifericos-dashboard/
```

### SSL não funciona:
```bash
sudo certbot certificates
sudo nginx -t
```

### Agent não conecta:
- Verificar `appsettings.json`
- Verificar firewall Windows
- Verificar logs no Event Viewer

## 📊 Resultado Final

Após completar todos os passos:

- ✅ **API**: https://api.in9automacao.com.br/swagger
- ✅ **Dashboard**: https://ti.in9automacao.com.br  
- ✅ **HTTPS** funcionando
- ✅ **Agent** instalado nos PCs
- ✅ **Monitoramento** em tempo real
- ✅ **Alertas** funcionando

**Tempo total estimado: 35 minutos**

---

*Sistema pronto para uso em produção!* 🎉
