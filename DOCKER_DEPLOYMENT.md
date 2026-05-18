# Docker deployment and seed import

Tai lieu nay huong dan deploy API + SQL Server bang Docker Compose va import seed script vao database trong container.

## 1. Chuan bi seed script

Dat script seed vao file:

```text
docker/sql/seed.sql
```

File nay dang la placeholder. Ban hay thay noi dung trong file bang script seed that cua ban.

Luu y: script nen insert vao database `PRN232_LMS`.

Neu script cua ban chua co dong `USE`, them dau file:

```sql
USE [PRN232_LMS];
GO
```

## 2. Chay database container

Mo Docker Desktop truoc, sau do chay:

```powershell
docker compose up -d db
```

Kiem tra container:

```powershell
docker ps
```

Container database la:

```text
prn232-lms-db
```

## 3. Chay API container de tao schema

API da duoc cau hinh de tu dong chay EF Core migrations khi start.

Chay:

```powershell
docker compose up -d api
```

Xem log:

```powershell
docker logs -f prn232-lms-api
```

Khi API chay thanh cong, database `PRN232_LMS` va cac table se duoc tao trong SQL Server container.

Swagger:

```text
http://localhost:8080/swagger
```

## 4. Import seed script vao DB container

Sau khi API da tao schema, chay:

```powershell
docker compose --profile seed run --rm db-seed
```

Lenh nay se:

1. Dung container `mcr.microsoft.com/mssql-tools`.
2. Ket noi toi SQL Server service `db`.
3. Chay file `/scripts/seed.sql`.
4. Import data vao database `PRN232_LMS`.

## 5. Kiem tra data da import

Chay lenh sau de dem so dong:

```powershell
docker run --rm --network prn232lms_default mcr.microsoft.com/mssql-tools `
  /opt/mssql-tools/bin/sqlcmd -S db -U sa -P Your_password123 -d PRN232_LMS `
  -Q "SELECT COUNT(*) AS Students FROM Students; SELECT COUNT(*) AS Courses FROM Courses; SELECT COUNT(*) AS Enrollments FROM Enrollments;"
```

Ket qua mong doi theo LAB1:

```text
Students >= 50
Courses >= 20
Enrollments >= 500
```

## 6. Chay full stack

Neu muon chay ca API va DB:

```powershell
docker compose up -d
```

Neu muon rebuild API image sau khi sua code:

```powershell
docker compose up -d --build api
```

## 7. Reset database container

Neu muon xoa database Docker va import lai tu dau:

```powershell
docker compose down -v
docker compose up -d db
docker compose up -d api
docker compose --profile seed run --rm db-seed
```

Can can than voi `down -v` vi lenh nay xoa volume SQL Server cua Docker.

## 8. Connection string trong Docker

Trong `docker-compose.yml`, API ket noi DB bang service name `db`:

```text
Server=db,1433;Database=PRN232_LMS;User ID=sa;Password=Your_password123;TrustServerCertificate=True;Encrypt=True
```

Khi chay app ngoai Docker tren may local, app van dung connection string trong `appsettings.json`.

