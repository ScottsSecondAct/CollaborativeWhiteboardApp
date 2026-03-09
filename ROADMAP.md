# Roadmap

This document tracks planned features and improvements for the CollaborativeWhiteboardApp beyond the v0.1.x MVP.

Items are grouped by theme and ordered roughly by priority within each section.

---

## Authentication & Identity

**Goal:** Replace anonymous user IDs with real accounts so sessions are attributable, auditable, and secure.

### Plan

1. **ASP.NET Core Identity + SQLite** — Add `Microsoft.AspNetCore.Identity.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.Sqlite` to the server. Create `AppUser : IdentityUser` and `AppDbContext : IdentityDbContext<AppUser>`.
2. **JWT Bearer tokens** — Issue short-lived JWTs (15 min access + 7-day refresh) from `AuthController` (`POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/refresh`).
3. **Protect the hub** — Add `[Authorize]` to `DrawingHub`. Pass the token via the SignalR `AccessTokenProvider` query-string hook.
4. **MAUI `AuthService`** — Wraps register/login HTTP calls, stores tokens in `SecureStorage`, exposes `GetAccessTokenAsync()` for auto-refresh.
5. **Login UI** — New `LoginPage` / `LoginPageViewModel` with email + password fields and a Register toggle. Shell navigates to `MainPage` on success.
6. **Token lifecycle** — Intercept 401 responses in `DrawingService`, call refresh, retry once; on failure navigate back to login.

### Files affected (estimated)
- `src/CollaborativeWhiteboard.Server/` — `AppDbContext`, `AppUser`, `AuthController`, `DrawingHub`, `Program.cs`, `appsettings.json`
- `src/CollaborativeWhiteboard.Core/` — `AuthService`, `IAuthService`
- `src/CollaborativeWhiteboard.ClientApp/` — `LoginPage`, `LoginPageViewModel`, `AppShell.xaml`, `MauiProgram.cs`
- `tests/` — `AuthControllerTests`, `AuthServiceTests`

---

## Encryption

**Goal:** Ensure data cannot be read by third parties whether it is moving over the network or stored on disk.

### Data in transit

| Layer | Mechanism |
|---|---|
| Server ↔ Client (SignalR / HTTP) | **TLS 1.2+** — enforce HTTPS in `Program.cs` (`UseHttpsRedirection`, `UseHsts`). In production, terminate TLS at a reverse proxy (nginx / Azure Front Door) with a valid certificate (Let's Encrypt via Certbot or managed cert). |
| SignalR handshake token | JWT already encrypted/signed by server secret. Pass only over TLS. |
| MAUI HTTP client | Pin certificate in production (`HttpClientHandler.ServerCertificateCustomValidationCallback`) or rely on OS trust store — prefer OS trust store unless threat model requires pinning. |

### Data at rest

| Store | Mechanism |
|---|---|
| SQLite user database (server) | **SQLCipher** (`Microsoft.EntityFrameworkCore.Sqlite` + `SQLitePCLRaw.bundle_sqlcipher`) — encrypts the entire database file with a passphrase stored in an environment variable / secret manager. |
| Refresh tokens (server) | Store only a **bcrypt hash** of the token, not the raw value, so a DB leak does not expose live tokens. |
| Access token (MAUI client) | **`SecureStorage`** — on iOS uses Keychain, on Android uses EncryptedSharedPreferences / Android Keystore, on Windows uses DPAPI. Never store in plain `Preferences`. |
| Drawing / chat history (future) | If persisted server-side, encrypt sensitive fields at the application layer (AES-256-GCM) before writing to the database, with keys managed by a KMS or environment secret. |

### Implementation steps

1. Configure HTTPS redirection and HSTS in `Program.cs`.
2. Update `Dockerfile` to expose port 443 and mount a TLS certificate.
3. Add `appsettings.Production.json` with `ConnectionStrings__DefaultConnection` referencing a SQLCipher key from an env var.
4. In `AuthService`, store the refresh token hash with `BCrypt.Net-Next`.
5. Update `AuthService` on the MAUI side to write all tokens exclusively through `SecureStorage`.
6. Document the TLS setup in `README.md` under a "Production Deployment" section.

---

## Reliability & Connection Management

- **Auto-reconnect** — Use SignalR's built-in `WithAutomaticReconnect()` and expose a `ConnectionStatus` observable in the ViewModel so the UI shows "Reconnecting…" and disables draw input.
- **Heartbeat / keep-alive** — Configure `KeepAliveInterval` and `HandshakeTimeout` on both server and client to detect stale connections quickly.
- **Offline queue** — Buffer draw strokes and chat messages locally while disconnected; flush on reconnect.

---

## Collaborative State

- **Canvas sync for late joiners** — Server maintains an in-memory (or Redis-backed) list of `DrawStroke` events per session. New clients receive the full history on connect via `SendAsync("SyncCanvas", strokes)`.
- **User presence** — Broadcast `UserJoined` / `UserLeft` events on `OnConnectedAsync` / `OnDisconnectedAsync`. Show an avatar list or name list in the toolbar.
- **Cursor sharing** — Broadcast pointer position at a throttled rate (e.g., 30 fps) so collaborators see each other's cursors in real time.

---

## Drawing Capabilities

- **Undo / redo** — Client-side command stack (`Stack<IDrawCommand>`); server broadcasts `UndoStroke(strokeId)` so other clients can remove the stroke.
- **Shape tools** — Rectangle, ellipse, line, arrow. Serialize as typed `DrawAction` variants rather than point lists.
- **Text annotations** — Place draggable text labels on the canvas.
- **Eraser tool** — Either a white-filled brush or an explicit erase action that removes strokes by region.
- **Image import** — Drop or pick an image; broadcast as a base-64 data URI or an upload URL.

---

## Export & Persistence

- **PNG / SVG export** — Render the current canvas to a file from the client. MAUI `GraphicsView` can save to a `Stream`; SVG requires manual serialization of strokes.
- **Session save / load** — Serialize the full stroke list to JSON and allow users to save and reload named sessions.
- **Server-side session storage** — Persist sessions to SQLite so they survive server restarts.

---

## Authorization & Roles

- **Session ownership** — The user who creates a whiteboard session is the owner and can invite others or set it public/private.
- **Read-only viewers** — Role-based hub authorization: viewers receive draw actions but their `BroadcastDrawAction` calls are rejected.
- **Admin panel** — Endpoint to list active sessions, connected users, and force-disconnect abusive clients.

---

## Audio / Video

- **Voice chat** — Integrate WebRTC or a managed service (e.g., Azure Communication Services, Daily.co) for low-latency voice during whiteboard sessions.
- **Video tiles** — Optional camera feeds alongside the canvas for a full "virtual room" experience.

---

## Observability

- **Structured logging** — Add `Serilog` with sinks for console (development) and a log aggregator (production).
- **Metrics** — Expose a `/metrics` endpoint (Prometheus format) tracking active connections, messages/sec, and error rates.
- **Health checks** — `app.MapHealthChecks("/health")` for load-balancer probes.

---

## DevOps

- **Docker Compose** — Add `docker-compose.yml` that runs the server with nginx TLS termination for local end-to-end testing.
- **CI for client tests** — GitHub Actions matrix to run `CollaborativeWhiteboard.ClientApp.Test` and `CollaborativeWhiteboard.Server.Test` on every PR.
- **Semantic versioning automation** — Auto-bump patch version and draft a changelog on merge to `main`.
