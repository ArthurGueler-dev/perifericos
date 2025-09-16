# Deploy Periféricos - VPS Hostinger

## Pré-requisitos
- VPS Ubuntu 22.04 (Hostinger)
- Acesso SSH root/sudo
- Domínios configurados no Cloudflare

## Passo 1: Configurar DNS no Cloudflare
1. Acesse o painel do Cloudflare
2. Vá em DNS > Records
3. Adicione os registros A:
   - `api.in9automacao.com.br` → IP da sua VPS
   - `ti.in9automacao.com.br` → IP da sua VPS
4. Certifique-se que o proxy (nuvem laranja) está ATIVADO

## Passo 2: Upload dos arquivos para VPS
```bash
# No seu PC Windows, comprima a pasta perifericos
# Suba via SFTP ou SCP para /root/perifericos

# Ou clone via git na VPS:
git clone <seu-repo> /root/perifericos
cd /root/perifericos
```

## Passo 3: Executar deploy
```bash
cd /root/perifericos/deploy
chmod +x deploy.sh
sudo ./deploy.sh
```

## Passo 4: Configurar SSL (Cloudflare)
```bash
# Instalar certbot
sudo apt-get install -y certbot python3-certbot-nginx

# Gerar certificados
sudo certbot --nginx -d api.in9automacao.com.br -d ti.in9automacao.com.br
```

## Passo 5: Configurar Cloudflare SSL
1. No painel Cloudflare, vá em SSL/TLS
2. Defina como "Full (strict)"
3. Ative "Always Use HTTPS"

## Passo 6: Testar
- API: https://api.in9automacao.com.br/swagger
- Dashboard: https://ti.in9automacao.com.br
- Login: qualquer email/senha

## Comandos úteis
```bash
# Ver logs da API
sudo journalctl -u perifericos-api -f

# Reiniciar API
sudo systemctl restart perifericos-api

# Status dos serviços
sudo systemctl status perifericos-api
sudo systemctl status nginx

# Testar configuração Nginx
sudo nginx -t
```

## Instalar Agente nos PCs
1. Edite `agent/Perifericos.Agent/appsettings.json`:
   - ApiBaseUrl: "https://api.in9automacao.com.br"
   - PcIdentifier: "PC-SETOR-01"
2. Publique e instale como serviço:
```powershell
cd agent/Perifericos.Agent
dotnet publish -c Release -r win-x64 --self-contained false -o out
sc create PerifericosAgent binPath= "%cd%\out\Perifericos.Agent.exe" start= auto
sc start PerifericosAgent
```

## Firewall VPS
```bash
sudo ufw allow 22    # SSH
sudo ufw allow 80    # HTTP
sudo ufw allow 443   # HTTPS
sudo ufw enable
```
