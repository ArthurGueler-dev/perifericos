#!/bin/bash
set -e

echo "=== Deploy Periféricos - VPS Hostinger ==="

# 1. Instalar .NET 8 (se não instalado)
if ! command -v dotnet &> /dev/null; then
    echo "Instalando .NET 8..."
    wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    sudo dpkg -i packages-microsoft-prod.deb
    sudo apt-get update
    sudo apt-get install -y aspnetcore-runtime-8.0
fi

# 2. Instalar Nginx (se não instalado)
if ! command -v nginx &> /dev/null; then
    echo "Instalando Nginx..."
    sudo apt-get install -y nginx
fi

# 3. Criar diretórios
echo "Criando diretórios..."
sudo mkdir -p /var/www/perifericos-api
sudo mkdir -p /var/www/perifericos-dashboard

# 4. Publicar API
echo "Publicando API..."
cd ../server/Perifericos.Server
dotnet publish -c Release -o /tmp/perifericos-api
sudo cp -r /tmp/perifericos-api/* /var/www/perifericos-api/
sudo chown -R www-data:www-data /var/www/perifericos-api
sudo chmod +x /var/www/perifericos-api/Perifericos.Server

# 5. Build Dashboard
echo "Buildando Dashboard..."
cd ../../dashboard
npm install
VITE_API_BASE=https://api.in9automacao.com.br npm run build
sudo cp -r dist/* /var/www/perifericos-dashboard/
sudo chown -R www-data:www-data /var/www/perifericos-dashboard

# 6. Configurar Nginx
echo "Configurando Nginx..."
sudo cp ../deploy/nginx-api.conf /etc/nginx/sites-available/api.in9automacao.com.br
sudo cp ../deploy/nginx-dashboard.conf /etc/nginx/sites-available/ti.in9automacao.com.br
sudo ln -sf /etc/nginx/sites-available/api.in9automacao.com.br /etc/nginx/sites-enabled/
sudo ln -sf /etc/nginx/sites-available/ti.in9automacao.com.br /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx

# 7. Configurar serviço systemd
echo "Configurando serviço..."
sudo cp ../deploy/perifericos-api.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable perifericos-api
sudo systemctl start perifericos-api

echo "=== Deploy concluído ==="
echo "API: https://api.in9automacao.com.br/swagger"
echo "Dashboard: https://ti.in9automacao.com.br"
echo ""
echo "Próximos passos:"
echo "1. Configure DNS no Cloudflare:"
echo "   - api.in9automacao.com.br A $(curl -s ifconfig.me)"
echo "   - ti.in9automacao.com.br A $(curl -s ifconfig.me)"
echo "2. Configure SSL com certbot:"
echo "   sudo certbot --nginx -d api.in9automacao.com.br -d ti.in9automacao.com.br"
