# Event Ticketing System

## Description
A modern ASP.NET Core MVC application for discovering events, registering seamlessly, and receiving ticket emails. It features:
- Light/Dark theme with smooth transitions and glass morphism
- Event browsing and details
- Registration and demo payment flow
- Mail Inbox (two-pane) to view ticket emails
- Download Ticket (PDF) via wkhtmltopdf + Rotativa
- Admin dashboard with registration exports (PDF/Excel)
- SQLite-backed data with seeded roles and admin user

## Prerequisites
- Windows
- .NET SDK (7.0 or later recommended)
- Optional for PDF generation:
  - wkhtmltopdf (Windows 64-bit)
  - Rotativa.AspNetCore configured to use `wwwroot\Rotativa`

## Installation
1) Restore packages:
```bash
dotnet restore
```

2) (Optional) Apply migrations if you prefer a fresh database:
```bash
dotnet ef database update
```
Note: A SQLite DB is included (`EventTicketing.db`). You can run directly without this step.

3) (Optional, for real email sending)
- Update SMTP settings under `appsettings.json` → `EmailSettings`.
- The default `EmailService` logs email content to console (no external calls).

4) Enable PDF ticket download (optional but recommended):
- Create the folder if it does not exist:

 ```bash
mkdir your_directory\dotnet_project_202303103510153-main\dotnet_project_202303103510153-main\wwwroot\Rotativa
```
-eg:
```bash
mkdir "D:\event proj\EventTicketingSystem\wwwroot\Rotativa"
```
- Download the Windows wkhtmltopdf (portable zip or installer).
- Copy the executable into the app:
```bash
Copy-Item "C:\Program Files\wkhtmltopdf\bin\wkhtmltopdf.exe" "D:\event proj\EventTicketingSystem\wwwroot\Rotativa\wkhtmltopdf.exe"
```
If you extracted a zip to Downloads, your source may be:
```bash
Copy-Item "C:\Users\YourUser\Downloads\wkhtmltox\bin\wkhtmltopdf.exe" "D:\event proj\EventTicketingSystem\wwwroot\Rotativa\wkhtmltopdf.exe"
```
- Unblock the file if Windows marked it as downloaded:
```bash
Unblock-File "D:\event proj\EventTicketingSystem\wwwroot\Rotativa\wkhtmltopdf.exe"
```
- Verify the binary runs:
```bash
& "D:\event proj\EventTicketingSystem\wwwroot\Rotativa\wkhtmltopdf.exe" --version
```

Rotativa setup (already configured in `Program.cs`):
- `RotativaConfiguration.Setup(app.Environment.WebRootPath);` ensures Rotativa looks under `wwwroot\Rotativa`.

## How To Run
1) Build:
```bash
dotnet build
```

2) Run:
```bash
dotnet run
```

3) Open the app using the URL displayed in the console (e.g., `http://localhost:5000` or similar).

## Default Accounts
- Admin:
  - Email: `admin@example.com`
  - Password: `Admin123!`
- Roles: `Admin`, `User` (seeded via `SeedData`)

## Key Features & Routes
- Home: `Home/Index` — hero, quick actions, features, featured events
- Events: `Event/Index`, `Event/Details/{id}` — browse and view details
- Registration: `Registration/Register/{id}` → Payment → `Registration/PaymentSuccess/{id}`
- My Tickets: `Event/MyTickets` — list user registrations
- Mail Inbox: `Registration/EmailTickets` — two-pane email viewer
- Download Ticket: Button in Mail preview (requires wkhtmltopdf) — falls back to HTML if binary is missing
- Admin: `Admin/Index`, exports via `ExportPdf` and `ExportExcel`

## Notes
- If `wkhtmltopdf.exe` is missing, “Download Ticket” gracefully falls back to the HTML view to avoid crashes.
- Theme toggle respects system preferences and persists your choice.
- For real payments, swap the demo `PaymentService` with a gateway (e.g., Razorpay).
