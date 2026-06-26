# Deploy Render

Build Docker dang thanh cong. Neu app crash voi loi `Missing Jwt:Key configuration`,
hay them bien moi truong sau trong Render Dashboard:

```text
Jwt__Key=<mot chuoi secret dai toi thieu 32 ky tu>
DATABASE_URL=<external PostgreSQL URL cua Render>
ASPNETCORE_ENVIRONMENT=Production
CORS_ALLOWED_ORIGINS=https://tao10m.com,https://tao10.pages.dev,https://tao10-frontends.pages.dev
```

Neu tao service bang Blueprint tu `render.yaml`, Render se tu sinh `Jwt__Key`.
Neu tao service thu cong tu Dashboard, ban phai nhap `Jwt__Key` bang tay.

Vi du tao nhanh secret JWT bang PowerShell:

```powershell
[Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }))
```
