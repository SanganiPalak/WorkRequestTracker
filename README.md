# Work Request Tracker

A small, straightforward implementation using **ASP.NET Core Web API (.NET 8)**, **Entity Framework Core + SQL Server**, and **React (Vite)**.

## What is included
- List requests with pagination, optional status filter and title/client search
- Get a request by ID
- Create a request with basic validation
- Update request status
- Add notes to a request
- Consistent API error responses
- SQL schema and useful indexes
- Basic React page with loading/error states

## Prerequisites
- Visual Studio 2022 with the **ASP.NET and web development** workload
- .NET 8 SDK
- SQL Server Express or LocalDB (Visual Studio usually installs LocalDB)
- SQL Server Management Studio (SSMS)
- Node.js LTS (includes npm)
- Visual Studio Code (recommended for the React folder)

## 1. Create / open the backend
The backend project is included at `backend/WorkRequestTracker.Api/`.

Open `WorkRequestTracker.sln` in Visual Studio. If you prefer to create it yourself:
1. Create a new **ASP.NET Core Web API** project.
2. Framework: **.NET 8.0**.
3. Authentication: **None**.
4. Check **Use controllers** and **Enable OpenAPI support**.
5. Name it `WorkRequestTracker.Api`.
6. Add the NuGet packages listed below.

The source files in this folder are already arranged as a .NET project.

### Configure SQL Server
Open `backend/WorkRequestTracker.Api/appsettings.json`. The included connection string uses LocalDB:

`Server=(localdb)\\MSSQLLocalDB;Database=YourDatabaseName;Trusted_Connection=True;TrustServerCertificate=True;`

If your SQL Server instance is different, update `ConnectionStrings:DefaultConnection`.

In Visual Studio, set `WorkRequestTracker.Api` as the startup project and press **F5**. On startup, the API creates the database and tables with `EnsureCreated()` if they do not exist. This is intentionally simple for a timed assignment; migrations would be preferable for a production project.

Swagger should open at `/swagger`.

## 2. Run the React frontend
Open a terminal in `frontend/` and run:

```bash
npm install
npm run dev
```

Open the local URL printed by Vite (normally `http://localhost:5173`).

The frontend expects the API at `https://localhost:7161`. If Visual Studio starts the API on a different port, update `API_BASE_URL` in `frontend/src/App.jsx`.

If the browser reports a certificate error, open the API's Swagger URL in the browser and accept/trust the local development certificate, then refresh the frontend.

## 3. Test the API
Use Swagger or Postman:
- `GET /api/work-requests`
- `GET /api/work-requests/1`
- `POST /api/work-requests`
- `PATCH /api/work-requests/1/status`
- `POST /api/work-requests/1/notes`

Example create request:

```json
{
  "title": "Review client dashboard",
  "clientName": "Northwind",
  "description": "Check dashboard calculations and filters.",
  "priority": "Medium",
  "status": "New",
  "dueDate": "2026-11-15",
  "notes": null
}
```

Example status update:

```json
{
  "status": "InProgress"
}
```

Example add note:

```json
{
  "text": "Waiting for client feedback."
}
```

## 4. SQL schema
See `sql/schema.sql`. The app uses EF Core `EnsureCreated()` for a quick local setup, while the SQL file documents the intended tables and indexes. Run the SQL script in SSMS if you want to inspect the schema separately, but do not run it against a database that already has conflicting tables.

## 5. Before submitting
1. Run the API and frontend locally.
2. Create a request, filter/search, change status, and add a note.
3. Check the API URL/port in `App.jsx`.
4. Update the candidate details and submission date in the response document.
5. Zip the project folder (or push it to a repository) and share the ZIP/repository link.
6. Be ready to explain each endpoint, DTO, validation check, index, and React state variable in your own words.

## Known limitations
- No authentication/authorization.
- Basic styling only.
- No automated tests included.
- Status transitions are not restricted; any defined status can be selected.
- Uses `EnsureCreated()` rather than migrations for quick local setup.
- Note creation is stored in a separate table and is returned when retrieving a single request; the list response stays lightweight.
