# Comanda - Server

API del sistema de pedidos en tiempo real. .NET 8 + Entity Framework Core + PostgreSQL (Supabase) + SignalR.

## Stack

- ASP.NET Core 8 Web API
- Entity Framework Core + Npgsql
- PostgreSQL (Supabase)
- SignalR para tiempo real

## Base de datos (Supabase)

La app usa el schema **comanda** (no `public`). Crear el schema si no existe:

```sql
CREATE SCHEMA IF NOT EXISTS comanda;
```

Configurar la cadena de conexión (no commitear credenciales):

1. **Opción A – archivo local:** Copiar `appsettings.Development.example.json` a `appsettings.Development.json` y reemplazar placeholders. Este archivo está en `.gitignore`.

2. **Opción B – User Secrets (recomendado):**
   ```bash
   cd Comanda.Api
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Password=...;"
   ```

3. **Opción C:** Variable de entorno `ConnectionStrings__DefaultConnection`.

Obtener la cadena: Supabase Dashboard → Project Settings → Database → **Connect** → **Session mode**.

Ver `.env.example` en la raíz del server para la lista de variables. En Render, configurar en el dashboard del servicio.

## Setup

```bash
dotnet restore
dotnet build
dotnet run --project Comanda.Api
```

La API corre en `http://localhost:5000`. Swagger: `http://localhost:5000/swagger`.

## Migraciones

```bash
dotnet ef database update --project Comanda.Infrastructure --startup-project Comanda.Api
```

Instalar herramientas EF si hace falta: `dotnet tool install --global dotnet-ef`

## Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/auth/login` | Login (name, role) |
| GET | `/api/orders` | Lista pedidos (query: userId, role, status) |
| POST | `/api/orders` | Crear pedido (query: createdByUserId) |
| PUT | `/api/orders/{id}/status` | Cambiar estado |

## SignalR Hub

- **URL:** `/hubs/orders?role=Waiter|Cook`
- **Eventos:** `OrderCreated`, `OrderStatusChanged`

## Despliegue en Render (Docker)

Render requiere Docker para .NET. El repo incluye `Dockerfile`.

1. Crear **Web Service** → **Docker**
2. **Dockerfile Path:** `./Dockerfile` (default)
3. **Variables de entorno:** Configurar según `.env.example`
   - `ConnectionStrings__DefaultConnection`
   - `Cors__AllowedOrigins` (URL del frontend, ej. `https://comanda-app.onrender.com`)

No hace falta instalar Docker localmente; Render construye la imagen al desplegar.

**Health Check:** Configurar `/healthz` en Render para monitoreo.
