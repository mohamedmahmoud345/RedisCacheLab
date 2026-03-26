# RedisCacheLab

A small **ASP.NET Core Web API learning demo** that shows how to use **Redis caching** (cache-aside pattern) with:
- **Entity Framework Core + SQL Server** for persistence
- **Redis** for caching API responses
- Simple **cache invalidation** on create/update/delete

## What it demonstrates

- Cache-aside flow:
  - On GET: check Redis first → if miss, load from DB → store in Redis
  - On POST/PUT/DELETE: update DB → remove related cache keys
- Cache keys for:
  - A paginated list of books
  - A single book by ISBN
- `X-Cache` response header (`HIT` / `MISS`) so you can see when Redis is used

## Tech stack

- ASP.NET Core Web API
- EF Core + SQL Server
- Redis
- `IDistributedCache` + `StackExchange.Redis`

## Prerequisites

- .NET SDK (a recent version that supports minimal hosting)
- SQL Server (LocalDB or full SQL Server)
- Redis (local or Docker)

## Configuration

Set connection strings (example below). You can use either:
- `appsettings.Development.json`, or
- .NET User Secrets (recommended for local dev)

Example `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "conStr": "Server=localhost;Database=RedisCacheLabDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  }
}
```

## Database migrations

From the repo root (or the project folder), run:

```bash
dotnet ef database update
```

If EF tools aren’t installed:

```bash
dotnet tool install --global dotnet-ef
```

## Run the API

```bash
dotnet run --project RedisCacheLab
```

If Swagger / OpenAPI is enabled in Development, open the browser at the Swagger/OpenAPI endpoint printed in the console.

## API endpoints

Base route: `/api/book`

### 1) Get paginated books (cached)
`GET /api/book?page=1&pageSize=10`

- On cache hit: returns header `X-Cache: HIT`
- On cache miss: returns header `X-Cache: MISS`

### 2) Get book by ISBN (cached)
`GET /api/book/{isbn}`

### 3) Create a book (invalidates cache)
`POST /api/book`

Example body:
```json
{
  "id": 1,
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "9780132350884",
  "isAvailable": true
}
```

### 4) Update a book (invalidates cache)
`PUT /api/book/{isbn}`

### 5) Delete a book (invalidates cache)
`DELETE /api/book/{isbn}`

## Notes

- This is intended as a **learning lab**, not a production-ready caching strategy.
- Pattern-based cache deletion is convenient for demos but can be expensive at scale.

## License

Add a license if you plan to share/reuse this code.
