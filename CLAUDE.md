# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build entire solution
dotnet build

# Run the server
dotnet run --project src/CollaborativeWhiteboard.Server

# Run all tests
dotnet test

# Run tests for a specific project
dotnet test tests/CollaborativeWhiteboard.Server.Test
dotnet test tests/CollaborativeWhiteboard.ClientApp.Test

# Run a single test (by name filter)
dotnet test --filter "TestMethodName"

# Publish server (e.g. for Docker)
dotnet publish src/CollaborativeWhiteboard.Server -c Release
```

## Architecture

This is a .NET 8 solution with two primary projects:

### Backend — `src/CollaborativeWhiteboard.Server` (ASP.NET Core)

- **`Hubs/DrawingHub.cs`**: The sole SignalR hub. Clients invoke `BroadcastDrawAction(userId, actionType, actionData)` and the hub re-broadcasts to all *other* connected clients via `Clients.Others.SendAsync("ReceiveDrawAction", ...)`.
- **`Program.cs`**: Registers SignalR and a CORS policy permitting the MAUI client origin.

### Frontend — `src/CollaborativeWhiteboard.ClientApp` (.NET MAUI)

Targets iOS, Android, macOS Catalyst, and Windows from a single codebase.

- **MVVM** using CommunityToolkit.Mvvm — `ViewModels/MainPageViewModel.cs` holds all business logic; `Views/MainPage.xaml` is the view.
- **`Services/DrawingService.cs`**: Wraps the SignalR `HubConnection`. Exposes `OnDrawActionReceived` event and `InvokeAsync("BroadcastDrawAction", ...)` for sending. DI registered as a singleton in `MauiProgram.cs`.
- **`Canvas/DrawableGraphics.cs`**: Implements `IDrawable` for the MAUI `GraphicsView`. Renders line collections using SkiaSharp-backed Graphics APIs.

### Real-Time Data Flow

1. User draws → `MainPageViewModel` calls `DrawingService`
2. `DrawingService` invokes `BroadcastDrawAction` on the server hub
3. Server rebroadcasts `ReceiveDrawAction` to all other clients
4. Other clients' `DrawingService` fires `OnDrawActionReceived` → ViewModel updates `DrawableGraphics` → canvas redraws

### Key Configuration

- **Server hub URL**: Currently a placeholder (`https://yoururlhere.com/DrawingHub`) in `MainPageViewModel.cs` — must be updated for real deployments.
- **Docker**: The server has a multi-stage `Dockerfile` ready for containerized deployment.
- **DI bootstrap**: `MauiProgram.cs` is the entry point for MAUI DI and service registration.

## MVP Task List

| # | Task | Status | Blocked By |
|---|------|--------|------------|
| 1 | Fix MainPageViewModel to use DrawingService (remove direct HubConnection, inject DrawingService, wire up ConnectCommand/DisconnectCommand, IsConnected property) | completed | — |
| 2 | Wire up outbound drawing sync — on stroke end, serialize DrawStroke to JSON and call DrawingService.SendDrawAction | completed | #1 |
| 3 | Wire up inbound drawing sync — deserialize received strokes, add to DrawableGraphics, invalidate canvas (dispatch to UI thread) | completed | #1 |
| 4 | Add connection UI — server URL Entry, Connect/Disconnect button, connection status label | completed | #1 |
| 5 | Add drawing toolbar — color buttons (black, red, blue, green, eraser) + stroke size selector | completed | #1 |
| 6 | Fix server CORS — read allowed origins from appsettings.json; allow any origin in Development | completed | — |
| 7 | Write server unit tests for DrawingHub relay behavior (mock SignalR context) | completed | — |
| 8 | Write client unit tests for DrawingService (mock HubConnection) | completed | — |
