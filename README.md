# Collaborative Whiteboard App

A cross-platform real-time collaborative whiteboard with integrated chat, built with .NET MAUI and ASP.NET Core SignalR.

## Current Release — v0.1.0

### What's working

**Drawing**
- Freehand drawing with multi-stroke canvas rendering
- Colour picker (black, red, blue, green, orange, eraser) with active colour preview
- Stroke size selector (S / M / L)
- Full real-time sync — strokes are serialised and broadcast to all connected clients via SignalR; remote strokes are deserialised and rendered on the canvas

**Chat**
- Concurrent chat panel at the bottom of the screen — runs on the same SignalR connection as drawing
- Send messages to all users, a named group, or a specific user
- Message bubbles distinguish own messages (right-aligned, green) from remote messages (left-aligned, with sender ID)
- Auto-scrolls to the newest message

**Connection**
- Hub URL entry with connect/disconnect controls and live status indicator
- Server CORS reads `AllowedOrigins` from `appsettings.json`; permits any origin in Development

### Solution structure

```
src/
  CollaborativeWhiteboard.Server        # ASP.NET Core 8 — SignalR hub
  CollaborativeWhiteboard.ClientApp     # .NET MAUI — iOS, Android, macOS, Windows
  CollaborativeWhiteboard.Core          # net8.0 class library — shared service layer
tests/
  CollaborativeWhiteboard.Server.Test   # 13 xUnit tests for DrawingHub
  CollaborativeWhiteboard.ClientApp.Test # 17 xUnit tests for DrawingService / MessagingService
```

### Architecture

#### Server (`CollaborativeWhiteboard.Server`)

`DrawingHub` is the sole SignalR hub, mapped at `/drawingHub`. It exposes:

| Method | Description |
|--------|-------------|
| `BroadcastDrawAction(userId, actionType, actionData)` | Relays a drawing stroke to all other clients |
| `BroadcastChatMessage(userId, text, sentAt, targetType, targetId)` | Relays a chat message — `targetType` is `"all"` (default), `"group"`, or `"client"` |
| `RegisterUser(userId)` | Stores the caller's `userId → connectionId` mapping for direct messaging |
| `JoinGroup(groupName)` / `LeaveGroup(groupName)` | Manage SignalR group membership |

#### Core library (`CollaborativeWhiteboard.Core`)

Shared between the MAUI app and the test projects. Contains:

- `IHubConnectionWrapper` / `HubConnectionWrapper` — testable abstraction over `HubConnection`
- `DrawingService` — manages the SignalR connection lifecycle; raises `OnDrawActionReceived`
- `MessagingService` — attaches to the shared connection after connect; raises `OnChatMessageReceived`

#### Client (`CollaborativeWhiteboard.ClientApp`)

MVVM with CommunityToolkit.Mvvm.

- `MainPageViewModel` — orchestrates both services; exposes drawing properties (`SelectedColor`, `StrokeSize`) and chat properties (`MessageText`, `Messages`)
- `MainPage.xaml` — three-row layout: connection bar → drawing toolbar → canvas, with a chat panel pinned at the bottom
- `DrawableGraphics` — `IDrawable` implementation that renders all `DrawStroke` objects on the `GraphicsView`

#### Real-time data flow

```
User draws
  → MainPage stamps stroke with UserId, Color, StrokeSize
  → ViewModel serialises to DrawStrokeDto JSON
  → DrawingService.SendDrawAction → BroadcastDrawAction hub method
  → Server relays to Clients.Others as ReceiveDrawAction
  → Remote clients' DrawingService fires OnDrawActionReceived
  → MainPage deserialises, adds to canvas, invalidates

User sends a message
  → ViewModel adds own bubble immediately, calls MessagingService.SendChatMessageAsync
  → BroadcastChatMessage hub method relays to target (all / group / client)
  → Remote clients' MessagingService fires OnChatMessageReceived
  → ViewModel dispatches to main thread, adds remote bubble to Messages
```

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- .NET MAUI workload: `dotnet workload install maui`

### Run the server

```bash
dotnet run --project src/CollaborativeWhiteboard.Server
```

The hub is available at `http://localhost:5000/drawingHub` by default.

### Run the client

```bash
dotnet build src/CollaborativeWhiteboard.ClientApp
```

Set the hub URL in the connection bar when the app starts. The default points to `http://localhost:5000/drawingHub`.

### Run all tests

```bash
dotnet test tests/CollaborativeWhiteboard.Server.Test
dotnet test tests/CollaborativeWhiteboard.ClientApp.Test
```

### Docker (server only)

```bash
dotnet publish src/CollaborativeWhiteboard.Server -c Release
docker build -t collaborative-whiteboard-server src/CollaborativeWhiteboard.Server
```

## Configuration

| File | Key | Purpose |
|------|-----|---------|
| `appsettings.json` | `AllowedOrigins` | CORS origins permitted in production |
| `appsettings.Development.json` | `AllowedOrigins: []` | Empty list → any origin allowed in Development |

## Roadmap

Features planned for future releases:

- **User roles** — Host, Editor, Viewer permissions
- **Undo / redo** — action event log with replay
- **Shape tools** — rectangles, ellipses, lines, text
- **File export** — image, PDF, SVG
- **Session persistence** — save and restore whiteboard state
- **Offline delta-sync** — queue local changes and replay on reconnect
- **Audio / video** — WebRTC or Azure Communication Services integration
- **Handwriting and sketch recognition**
