# Production Deployment

Panduan ini untuk production: GitHub private repo, Docker Compose, aaPanel/Nginx reverse proxy, SSL, dan backup MySQL.

## 1. Siapkan GitHub Private Repo

Jangan commit file secret. Repo ini sudah punya `.gitignore` untuk `.env`, `Settings.ini`, `node_modules`, `bin/obj`, `.next`, dan log.

```bash
git init
git add .
git commit -m "Initial production deployment setup"
git branch -M main
git remote add origin git@github.com:USERNAME/maintenance-management.git
git push -u origin main
```

## 2. Siapkan VPS Ubuntu

Install Docker, Docker Compose plugin, Git, dan MySQL client untuk backup.

```bash
sudo apt update
sudo apt install -y git docker.io docker-compose-plugin mysql-client
sudo systemctl enable --now docker
```

Clone repo ke folder production.

```bash
sudo mkdir -p /opt/maintenance-management
sudo chown "$USER":"$USER" /opt/maintenance-management
git clone git@github.com:USERNAME/maintenance-management.git /opt/maintenance-management
cd /opt/maintenance-management
```

## 3. Isi Environment Production

```bash
cp .env.example .env
nano .env
```

Wajib diganti:

- `FRONTEND_ORIGIN`: domain frontend, contoh `https://maintenance.example.com`
- `NEXT_PUBLIC_API_BASE_URL`: untuk domain tunggal pakai `https://cmms.nusakaryadigital.com`
- `MYSQL_CONNECTION_STRING`: koneksi MySQL production
- `JWT_SIGNING_KEY`: secret panjang random, contoh buatnya: `openssl rand -base64 64`
- `SWAGGER_ENABLED`: biarkan `false` untuk production public, ubah hanya saat perlu debugging terbatas

Kalau MySQL ada langsung di VPS yang sama, compose sudah menyediakan `host.docker.internal`. Pastikan MySQL menerima koneksi dari Docker bridge, atau pakai host/IP database yang memang bisa diakses container.

## 4. Jalankan Aplikasi

```bash
docker compose -f docker-compose.prod.yml --env-file .env up -d --build
docker compose -f docker-compose.prod.yml ps
```

Cek log:

```bash
docker compose -f docker-compose.prod.yml logs -f api
docker compose -f docker-compose.prod.yml logs -f frontend
```

Port internal VPS:

- Docker web gateway: `127.0.0.1:8088`

## 5. aaPanel / Nginx Reverse Proxy

Untuk domain yang sudah dibuat, aaPanel cukup diarahkan ke satu target:

- `https://cmms.nusakaryadigital.com/` reverse proxy ke Docker web gateway `http://127.0.0.1:8088`

Routing `/` ke frontend dan `/api/` ke backend ditangani oleh Nginx di dalam stack Docker melalui `deploy/nginx-stack.conf`.

Aktifkan SSL Let's Encrypt untuk domain itu. Setelah SSL aktif, pastikan `.env` memakai:

```env
FRONTEND_ORIGIN=https://cmms.nusakaryadigital.com
NEXT_PUBLIC_API_BASE_URL=https://cmms.nusakaryadigital.com
```

Lalu rebuild frontend karena `NEXT_PUBLIC_API_BASE_URL` dibaca saat build:

```bash
docker compose -f docker-compose.prod.yml --env-file .env up -d --build frontend
```

## 6. Update Production

```bash
cd /opt/maintenance-management
git pull
docker compose -f docker-compose.prod.yml --env-file .env up -d --build
```

## 7. Backup MySQL

Copy contoh env backup:

```bash
cp deploy/backup.env.example deploy/backup.env
nano deploy/backup.env
chmod +x deploy/backup-mysql.sh
```

Test backup:

```bash
./deploy/backup-mysql.sh
```

Cron harian jam 02:00:

```bash
crontab -e
```

Tambahkan:

```cron
0 2 * * * /opt/maintenance-management/deploy/backup-mysql.sh >> /var/log/maintenance-db-backup.log 2>&1
```

Simpan backup penting di lokasi lain juga, misalnya object storage atau server backup terpisah.
