# MaintenanceApp CMMS

## Analisa Project

- Backend framework: ASP.NET Core Web API .NET 8 (`MaintenanceApp.API`).
- Database existing: MySQL, konfigurasi dari `MaintenanceApp.API/Settings.ini`.
- Data access: sudah ditambahkan Entity Framework Core + Pomelo MySQL. Repository lama Preventive dan MachineTrouble dipindahkan dari Dapper ke EF Core.
- Android existing: project `MaintenanceApp` adalah .NET MAUI multi-target termasuk `net8.0-android`, jadi fitur mobile tahap ini mengikuti struktur MAUI existing, bukan membuat project Kotlin baru.
- Web frontend: tersedia di folder `Frontend`.

## File/Folders Utama Yang Ditambahkan

- `Maintenance.Domain/Cmms/CmmsModels.cs`: entity, enum, request DTO, KPI/dashboard response.
- `Maintenance.Infrastructure/Context/MaintenanceDbContext.cs`: EF Core DbContext dan mapping tabel.
- `Maintenance.Infrastructure/Migrations/20260515_001_cmms_schema_and_seed.sql`: migration SQL + seed data.
- `Maintenance.Infrastructure/Migrations/20260518_001_contractor_monitoring.sql`: tabel contractor monitoring, dokumen, dan audit.
- `Maintenance.Persistence/Services/CmmsService`: business service CMMS.
- `MaintenanceApp.API/Controllers/CmmsControllers.cs`: REST API CMMS.
- `MaintenanceApp/Services/ApiClient.cs`: API client mobile terpusat.
- `MaintenanceApp/Repositories/CmmsRepository.cs`: repository mobile.
- `MaintenanceApp/Mapping/Cmms/CmmsDtos.cs`: DTO mobile.
- `MaintenanceApp/ViewsModel/CmmsViewModels.cs`: ViewModel mobile.
- `MaintenanceApp/Views/*`: login, dashboard, asset, work order, sparepart, technician screens.
- `Maintenance.Tests`: xUnit backend tests.

## Backend Setup

1. Pastikan MySQL berjalan dan database ada:

```sql
CREATE DATABASE IF NOT EXISTS db_maintenance;
```

2. Untuk lokal lama, koneksi bisa dicek di:

```text
MaintenanceApp.API/Settings.ini
```

Default:

```ini
[Connections]
Server=127.0.0.1
Port=3306
UserID=maintenance_user
Password=change-me
Db=db_maintenance
```

Untuk production, gunakan environment variable `MYSQL_CONNECTION_STRING` atau `ConnectionStrings__DefaultConnection`, bukan `Settings.ini`.

3. Restore dan build backend:

```powershell
dotnet restore MaintenanceApp.API\Maintenance.API.csproj
dotnet build MaintenanceApp.API\Maintenance.API.csproj
```

4. Jalankan migration + seed:

```powershell
Get-Content Maintenance.Infrastructure\Migrations\20260515_001_cmms_schema_and_seed.sql | mysql -h 127.0.0.1 -P 3306 -u maintenance_user -p db_maintenance
Get-Content Maintenance.Infrastructure\Migrations\20260518_001_contractor_monitoring.sql | mysql -h 127.0.0.1 -P 3306 -u maintenance_user -p db_maintenance
```

5. Run backend:

```powershell
dotnet run --project MaintenanceApp.API\Maintenance.API.csproj
```

API default local:

```text
http://localhost:5241
```

Swagger:

```text
http://localhost:5241/swagger
```

## Endpoint API

Asset:
- `GET /api/assets`
- `GET /api/assets/{id}`
- `POST /api/assets`
- `PUT /api/assets/{id}`
- `DELETE /api/assets/{id}`

Technician:
- `GET /api/technicians`
- `GET /api/technicians/{id}`
- `POST /api/technicians`
- `PUT /api/technicians/{id}`
- `DELETE /api/technicians/{id}`

Work Order:
- `GET /api/work-orders`
- `GET /api/work-orders/{id}`
- `POST /api/work-orders`
- `PUT /api/work-orders/{id}`
- `DELETE /api/work-orders/{id}`
- `PATCH /api/work-orders/{id}/assign`
- `PATCH /api/work-orders/{id}/start`
- `PATCH /api/work-orders/{id}/complete`
- `PATCH /api/work-orders/{id}/close`
- `POST /api/work-orders/{id}/spareparts`

Contractor Monitoring:
- `GET /api/contractor-monitoring`
- `GET /api/contractor-monitoring/reminders`
- `GET /api/contractor-monitoring/{id}`
- `POST /api/contractor-monitoring`
- `PUT /api/contractor-monitoring/{id}`
- `DELETE /api/contractor-monitoring/{id}`
- `POST /api/contractor-monitoring/{id}/documents`
- `GET /api/contractor-monitoring/{id}/documents/{documentId}/content`
- `DELETE /api/contractor-monitoring/{id}/documents/{documentId}`
- `POST /api/contractor-monitoring/{id}/supervision-work-order`

Preventive Schedule:
- `GET /api/preventive-schedules`
- `GET /api/preventive-schedules/{id}`
- `POST /api/preventive-schedules`
- `PUT /api/preventive-schedules/{id}`
- `DELETE /api/preventive-schedules/{id}`
- `POST /api/preventive-schedules/generate-due`

Downtime:
- `GET /api/downtime-logs`
- `GET /api/downtime-logs/{id}`
- `POST /api/downtime-logs`
- `PUT /api/downtime-logs/{id}`
- `DELETE /api/downtime-logs/{id}`

Sparepart:
- `GET /api/spareparts`
- `GET /api/spareparts/{id}`
- `POST /api/spareparts`
- `PUT /api/spareparts/{id}`
- `DELETE /api/spareparts/{id}`

Master:
- `/api/failure-codes`
- `/api/root-causes`

KPI/Dashboard:
- `GET /api/kpi/reliability?asset_id=&start_date=&end_date=`
- `GET /api/dashboard/summary`

## Business Rules

- Status normal WO: `OPEN -> ASSIGNED -> IN_PROGRESS -> COMPLETED -> CLOSED`.
- WO tidak bisa `CLOSED` sebelum `COMPLETED`.
- `completed_at` wajib saat status `COMPLETED`.
- `closed_at` wajib saat status `CLOSED`.
- `repair_minutes` dan `downtime_minutes` dihitung otomatis dari start/end.
- WO `BREAKDOWN` wajib punya `failure_code` dan `root_cause` sebelum `CLOSED`.
- Jika WO memiliki downtime start/end, downtime log dibuat atau diperbarui otomatis.
- Pemakaian sparepart mengurangi stock dan mencatat inventory transaction; stock tidak boleh minus.

## Android / MAUI App

Project mobile existing:

```text
MaintenanceApp/MaintenanceApp.csproj
```

Base URL API disimpan di SQLite local melalui halaman `Setting`. Default baru:

```text
http://10.0.2.2:120
```

Untuk emulator Android, `10.0.2.2` mengarah ke host machine. Untuk device fisik, ubah dari halaman `Setting` ke IP PC, misalnya:

```text
http://192.168.1.10:120
```

Dummy login:

```text
username: technician
password: maintenance
```

Run Android:

```powershell
dotnet workload restore MaintenanceApp\MaintenanceApp.csproj
dotnet build MaintenanceApp\MaintenanceApp.csproj -f net8.0-android
```

Jika workload Android belum terpasang, install MAUI/Android workload dari Visual Studio Installer atau jalankan `dotnet workload restore`.

## Testing

Backend tests:

```powershell
dotnet test Maintenance.Tests\Maintenance.Tests.csproj
```

Coverage test minimal yang sudah ada:

- create asset
- create work order
- status transition work order
- repair minutes calculation
- downtime minutes calculation
- generate preventive WO
- calculate MTTR/MTBF
- sparepart stock decrease dan reject stock minus

## Catatan Build

- Backend build berhasil.
- Backend tests berhasil: 6 passed.
- MAUI target Windows berhasil compile untuk validasi C#/XAML.
- Build Android membutuhkan workload `android` dan `wasm-tools-net8`; `dotnet workload restore` sempat timeout di environment ini, jadi Android final build perlu dijalankan setelah workload tersedia.
