# Project State — Sistema de Reservas

Estado actual del proyecto y decisiones tomadas. Se actualiza al finalizar cada fase.

---

## Estado de fases

| Fase | Estado | Fecha |
| ---- | ------ | ----- |
| 0. Definición | ✅ Completada | 2026-09-27 |
| 1. Estructura | ✅ Completada | 2026-09-27 |
| 2. Domain | ✅ Completada | 2026-09-27 |
| 3. Application | ✅ Completada | 2026-09-28 |
| 4. Infrastructure | ✅ Completada | 2026-09-28 |
| 5. API | ✅ Completada | 2026-09-28 |
| 6. Servicios | ✅ Completada | 2026-09-28 |
| 7. JWT | ✅ Completada | 2026-09-28 |
| 8. Roles | ✅ Completada | 2026-09-28 |
| 9. Profesionales | ✅ Completada | 2026-09-28 |
| 10. Horarios | ✅ Completada | 2026-09-28 |
| 11. Reservas | ✅ Completada | 2026-09-28 |
| 12. Validaciones | ✅ Completada | 2026-09-28 |
| 13. Tests | ✅ Completada | 2026-09-28 |
| 14. Docker | ✅ Completada | 2026-09-28 |
| 15. Git/GitHub | ✅ Completada | 2026-09-28 |
| 16. Frontend Angular | ✅ Completada | 2026-09-28 |
| 17. CI/CD | ⬜ Pendiente | |
| 18. Revisión final | ⬜ Pendiente | |

---

## Fase 16 — Angular Frontend Foundation (completada)

### Entorno y scaffold

* **Stack:** Angular **22.1.0** (standalone components, Router, HttpClient, Reactive Forms), TypeScript ~6.0.2, Angular CLI global 22.1.5, Node.js v24.19.0, npm 11.17.0.
* **Ubicación:** `src/SpaceManager.Web/` (app generada con `ng new … --directory src/SpaceManager.Web --style=css --ssr=false --skip-tests`; trae su propio `.gitignore` que ignora `node_modules/` y `dist/`).
* **Restricciones cumplidas:** sin NgRx/Akita/Material/PrimeNG/Tailwind ni librerías de estado o UI; sin SSR; sin secretos en el frontend (solo `environment.apiUrl`); sin GitHub Actions/Pages/despliegue; sin cambios en la lógica del backend salvo el CORS de desarrollo.

### Estructura

```text
src/SpaceManager.Web/src/app/
├── core/
│   ├── models/api.models.ts            # interfaces = DTOs reales de la API
│   ├── services/                       # AuthService, ServiceApi, ProfessionalApi, ReservationApi, HttpErrorService
│   ├── interceptors/auth.interceptor.ts
│   └── guards/auth.guard.ts
├── features/                           # login, register, dashboard, services, professionals, reservations
├── shared/state-message/               # componente reutilizable loading/error/empty
├── app.routes.ts / app.config.ts / app.ts (shell + navbar)
└── environments/                       # environment.ts + environment.development.ts (fileReplacements en development)
```

* **Modelos TypeScript:** copian exactamente los DTOs del backend — `User`, `Login/Register`, `LoginResponse`, `Service`, `Professional`, `Availability`, `ProfessionalService` (assign), `Reservation`; enums como uniones de strings (`UserRole`, `ReservationStatus`, `DayOfWeek`), fechas/horas como `string`.
* **Servicios core:** los componentes nunca llaman a HTTP directamente; toda URL sale de `environment.apiUrl = 'http://localhost:5163'` (única configuración de entorno; `environment.development.ts` reemplaza a `environment.ts` vía `fileReplacements` en el configuration de desarrollo).

### Rutas, guard e interceptor

* Públicas: `/login`, `/register`, `/services`, `/professionals`. Protegidas: `/dashboard`, `/reservations` con `authGuard` que redirige a `/login` si no hay JWT (autorización real: sigue siendo del backend; el guard es solo UX).
* `authInterceptor` añade `Authorization: Bearer <token>` cuando existe sesión; se registra una sola vez en `app.config.ts` (`provideHttpClient(withInterceptors([authInterceptor]))`).
* Login guarda JWT + usuario mínimo en localStorage (`sm.token`/`sm.user`; **nunca** la contraseña); logout limpia ambas claves y navega a `/login`.
* Errores de API reutilizables vía `HttpErrorService` (bodies string del `Result<T>`, `{error}`, o fallback por status 401/403/404/sin conexión); estados de carga/vacío/error con `StateMessage`.

### CORS de desarrollo (único cambio en el backend)

* `src/Reservation.Api/Program.cs`: política **explícita de solo desarrollo** — `WithOrigins("http://localhost:4200")` + `AllowAnyHeader`/`AllowAnyMethod`, aplicada con `app.UseCors` antes de `UseAuthentication`. Sin `AllowAnyOrigin` (tampoco con credenciales — el JWT viaja en cabecera, no en cookies) y sin política amplia de producción; documentado en el código como configuración de desarrollo. Nada más cambió en la API.

### Verificación

* **Regresión backend:** `dotnet build` → 0 errores / 0 warnings; `dotnet test` → **73/73** (18 Domain + 55 Application).
* **Builds frontend:** `ng build` desarrollo y producción → correctos; producción ≈ 284 kB iniciales (presupuesto 500 kB).
* **E2E local** (`ng serve` :4200 + API :5163 + PostgreSQL :5435):
  * CORS: preflight `OPTIONS` y `GET` con `Origin: http://localhost:4200` → `Access-Control-Allow-Origin: http://localhost:4200`; con origen distinto → sin cabecera (el navegador bloquea).
  * Registro por formulario → 201 → login → JWT almacenado → `/dashboard` muestra nombre, email y rol `Client`.
  * `/services` y `/professionals` cargan datos reales de PostgreSQL ("Corte de pelo", "Barbería Central").
  * `/reservations` (protegida) → 200 con el interceptor (lista vacía para usuario nuevo; sin 401 → la cabecera llega y el backend la acepta).
  * Guard: sin sesión, acceso directo a `/dashboard` → redirige a `/login`; logout elimina el JWT y el navbar vuelve a "Sign in / Register".
  * Errores: credenciales inválidas → "Invalid email or password." (401 con cuerpo vacío → mensaje contextual del login); consola del navegador sin errores evitables.

### Limitaciones / pendientes

* **Cambios sin commitear:** la fase 16 no pidió commit; el árbol de trabajo queda con estos cambios pendientes de que se solicite.
* **Doc de fases anteriores:** este registro no tiene secciones de las fases 0, 3–6 y 15 (fase 15 — Git/GitHub — solo está reflejada en la tabla). Se reporta sin retomarlas.
* Roles/permisos en frontend, disponibilidad y creación/cancelación de reservas desde la UI quedan para fases futuras; el backend sigue siendo la autoridad de autorización.

---

## Fase 14 — Docker + PostgreSQL Runtime (completada)

### Configuración Docker/PostgreSQL

* **Inspección previa:** `docker ps` mostró los puertos 5432 (`postgres-server`), 5433 (`proyecto-postgres`) y 5434 (`minitask-postgres`) ocupados por contenedores **ajenos** — ninguno se tocó. 5435 se verificó libre antes de elegirlo.
* **Puerto de host elegido: 5435.** `docker-compose.yml` → `"5435:5432"`: dentro de la red Docker PostgreSQL sigue en 5432; la API (corriendo en el host) se conecta a `localhost:5435`. `appsettings.json` solo cambió el puerto a 5435.
* **Credenciales:** `.env` nuevo en la raíz (modo 600) con `POSTGRES_PASSWORD` generado con `secrets.token_urlsafe(24)` — docker compose lo lee automáticamente. `.gitignore` nuevo con `.env` (más `bin/`, `obj/`). El password real **no aparece en ningún archivo del repositorio** (verificado por escaneo de todos los archivos contra el valor de `.env`), ni en README ni en docs. `appsettings.json` conserva el placeholder `CHANGE_ME`; la API recibe el valor real por la variable de entorno `ConnectionStrings__Postgres`.

### Bug de producción encontrado en el arranque (corregido)

* El primer arranque falló con `InvalidOperationException` de `UseExceptionHandler()`. Causa verificada en el código fuente de .NET 10 (`ExceptionHandlerMiddlewareImpl`): el constructor exige que exista un *fallback* — `ExceptionHandlingPath`, `ExceptionHandler` o **`IProblemDetailsService`** — y los `IExceptionHandler` registrados **no** satisfacen esa validación. La Fase 12 registró `AddExceptionHandler<GlobalExceptionHandler>()` sin `AddProblemDetails()`; nunca se detectó porque ninguna fase anterior había arrancado la app.
* **Corrección mínima (1 línea + comentario):** `builder.Services.AddProblemDetails();` en `Program.cs`. `GlobalExceptionHandler` sigue ejecutándose **primero** y siempre maneja la excepción (`TryHandleAsync` → `true`), así que el fallback ProblemDetails nunca se usa en runtime: comportamiento idéntico al diseñado en Fase 12 (500 fijo / 409).

### Migración

`dotnet ef database update --project src/Reservation.Infrastructure --startup-project src/Reservation.Api` → `Applying migration '20260928172254_InitialCreate'` → **Done.** Verificado en `__EFMigrationsHistory`; 7 tablas (6 entidades + historial). La migración no se modificó.

### Resultado de la API en runtime

* Arranca sin errores de conexión, de configuración EF ni de JWT (`http://localhost:5163`, perfil `http`, `ASPNETCORE_ENVIRONMENT=Development`).
* El log muestra comandos `DbCommand` reales contra `"Services"`/`"Users"`/etc. → conectada a PostgreSQL.

### Smoke tests HTTP (38/38 + 3/3)

Script en Python (stdlib, fuera del repo — contiene credenciales de prueba locales). Resultados:

* **Catálogo público:** `GET /api/services` y `GET /api/professionals` → 200 sin token.
* **Autenticación:** registro → 201 (sin material de contraseña en la respuesta); email duplicado → 409; login con contraseña errónea o email desconocido → 401; login correcto → 200 con JWT; la contraseña en claro no aparece ni en el cuerpo ni dentro del token.
* **Autorización:** sin token → 401; Cliente sobre registro ajeno → 403; Cliente sobre el suyo → 200; Cliente sobre endpoint Admin → 403; Admin → 200.
* **Admin local (sin bypass):** registro por API → SQL directo en el contenedor → re-login:

  ```bash
  docker exec reservation-postgres psql -U reservation_user -d reservation_db \
    -c "UPDATE \"Users\" SET \"Role\"='Admin' WHERE \"Email\"='<tu-email>';"
  ```

  (procedimiento temporal documentado; **no** se creó endpoint de administradores.)
* **Flujo de reserva end-to-end (14 pasos):** Admin crea Servicio → Profesional → asigna (y duplicado → 409) → disponibilidad `mañana 09:00–17:00 UTC` (listado público la devuelve) → Cliente se registra/loguea → crea reserva para mañana 12:00 UTC → `Pending`, UserId/ProfessionalId/ServiceId correctos, `EndAt` = +30 min de duración → solape → 409 → GET propio → 200 → listado solo propias → DELETE → `Cancelled` → sigue existiendo → cancelar otra vez → 409 → Admin puede leerla.
* **Manejo de errores en runtime:**409 duplicado, 400 entrada inválida, 401 sin token — cuerpos **sin** stack, SQL, `npgsql`, connection string ni `Exception` (escaneo de patrones). No se provocó un 500 deliberado (no se corrompió la BD, §10); ningún error inesperado ocurrió durante toda la ejecución.

### Persistencia verificada en PostgreSQL (psql, no solo HTTP)

| Tabla | Contenido tras el smoke test |
| ----- | ---------------------------- |
| `__EFMigrationsHistory` | `20260928172254_InitialCreate` |
| `Users` | 2 filas: `admin.local@test.local` (Admin) + `cliente.local@test.local` (Client) |
| `Services` | 1 fila: "Corte de pelo", 30 min, 20.00 |
| `Professionals` | 1 fila: "Barbería Central" |
| `ProfessionalServices` | 1 par professional↔service |
| `Availabilities` | 1 fila: Tuesday 09:00–17:00, activa |
| `Reservations` | 1 fila: **Cancelled**, `2026-09-29 12:00:00+00 → 12:30:00+00` (soft cancel, no borrada) |

### Verificación final (resultados reales)

```text
docker compose config    → OK (POSTGRES_PASSWORD resuelto desde .env; salida saneada)
docker compose ps        → reservation-postgres Up, 0.0.0.0:5435->5432/tcp
pg_isready (en contenedor) → accepting connections
dotnet build             → Build succeeded. 0 Warning(s), 0 Error(s)
dotnet test              → 73 passed (18 Domain + 55 Application), 0 failed
smoke HTTP               → 38/38 checks + 3/3 escaneos de fugas en cuerpos de error
escaneo de secretos      → el password real no está en ningún archivo del repo
git status               → "not a git repository" (sigue sin inicializar; decisión del usuario)
```

### Limitaciones / notas

* **Git:** el directorio aún no es repositorio, por lo que `git status`/`git diff` no aplican; `.gitignore` (con `.env`) queda listo para cuando se inicialice. No se hizo `git init` ni se cometió nada.
* **JWT:** el `Jwt:SecretKey` trackeado sigue siendo el placeholder de desarrollo documentado en Fase 7; producción usa `Jwt__SecretKey`. No se añadió ningún secreto nuevo.
* Contenedores ajenos intactos; no se añadieron healthchecks ni servicios (la API corre en el host y no depende de Compose).
* El 500 genérico del `GlobalExceptionHandler` no se provocó a propósito (§10 prohíbe corromper la BD); su contrato sigue verificado estáticamente desde la Fase 12 y ahora el handler está confirmado operativo en arranque.

---

## Fase 13 — Tests (completada)

### Proyectos creados

```text
tests/
├── Reservation.Domain.Tests/
│   ├── ServiceTests.cs
│   ├── ProfessionalTests.cs
│   ├── AvailabilityTests.cs
│   └── ReservationTests.cs
└── Reservation.Application.Tests/
    ├── Fakes/
    │   ├── FakeCurrentUser.cs / FakePasswordHasher.cs / FakeTokenService.cs
    │   └── InMemory{User,Professional,Service,ProfessionalService,Availability,Reservation}Repository.cs
    ├── ServiceUseCasesTests.cs
    ├── AvailabilityUseCasesTests.cs
    ├── ProfessionalServiceUseCasesTests.cs
    ├── AuthUseCasesTests.cs
    ├── UserUseCasesTests.cs
    └── ReservationUseCasesTests.cs
```

`Reservation.slnx` → carpeta `/tests/` con los dos proyectos (xUnit + `Microsoft.NET.Test.Sdk` de la plantilla `dotnet new xunit`, net10.0).

### Estrategia de tests

* Enfoque en **reglas de negocio de Domain y Application**: invariantes de entidades y casos de uso. Sin frameworks de mocking — fakes escritos a mano en `tests/.../Fakes/`.
* **Cero cambios en código de producción.** Las abstracciones existentes (`Application/Abstractions`) permiten sustituir cada dependencia por un fake; no se hizo público ningún miembro ni se añadió `InternalsVisibleTo`.
* Sin Infrastructure, sin PostgreSQL, sin Testcontainers, sin `docker compose`: el bloqueo de entorno de la fase 4 sigue intacto y la fase no lo requiere.
* Los fakes reproducen el **contrato documentado** de los repositorios (no el SQL):
  * `GetConflictingAsync` → `Status != Cancelled && StartAt < finSolicitado && EndAt > inicioSolicitado`.
  * Listado de disponibilidad → solo activas; `HasOverlapAsync` con la fórmula de solape y exclusión del propio registro.
  * `FakePasswordHasher`/`FakeTokenService` deterministas, registran llamadas: sin PBKDF2 ni JWT en Application (eso es de Infrastructure).
* `FakeCurrentUser` permite configurar `IsAuthenticated`/`UserId`/`IsAdmin` sin referencia a `HttpContext`.
* Convención de nombres `Método_Condición_Esperado`, AAA, un comportamiento por test. Fecha fija "mañana 12:00 UTC" en los tests de reserva → deterministas bajo la convención UTC única.

### Comportamientos cubiertos (73 tests)

* **Domain (18):** Service (nombre vacío, duración ≤ 0, precio negativo, creación válida); Professional (nombre vacío, creación válida); Availability (rango inválido, creación válida, `Update` conserva `ProfessionalId`, update inválido, `Deactivate` → `IsActive=false`); Reservation (estado inicial `Pending`, `EndAt` derivado de la duración, ids vacíos rechazados, `EndAt > StartAt`, `Cancel` → `Cancelled`, doble `Cancel` rechazado).
* **Application (55):**
  * **ServiceUseCases (4):** nombre/duración/precio inválidos → Validation; creación válida persistida.
  * **AvailabilityUseCases (7):** profesional inexistente → 404; rango inválido → 400; solape activo → 409; creación válida; update de periodo de otro profesional → 404 (convención real de la fase 10, no 403); desactivación; listado excluye inactivas.
  * **ProfessionalServiceUseCases (7):** 404 profesional/servicio; inactivos → 400; duplicado → 409; asignación válida; eliminación de relación existente.
  * **AuthUseCases (7):** email duplicado → 409; contraseña pasada por `IPasswordHasher` (nunca en claro); login inválido → 401 con **mensaje idéntico** para email desconocido y contraseña errónea (sin enumeración de cuentas); token creado vía `ITokenService`; login exitoso devuelve los datos de usuario esperados.
  * **UserUseCases (5):** no autenticado → 401; Cliente sobre registro ajeno → 403; Cliente sobre el suyo → éxito; Admin sobre ajeno → éxito; inexistente → 404.
  * **ReservationUseCases (25):** *Create* (14): 401 no autenticado, 403 Cliente creando para otro UserId, 404 usuario/profesional/servicio inexistentes, 400 inactivos, 409 servicio no ofrecido, 400 inicio en el pasado, 400 fuera de disponibilidad, 409 solape, solape con reserva cancelada **no** bloquea, éxito con `Pending` + UserId/ProfessionalId/ServiceId correctos + `EndAt` de la duración + persistencia. *List* (2): Cliente solo las suyas, Admin todas. *GetById* (4): propio/ajeno 403/admin/404. *Cancel* (5): propio, ajeno → 403, Admin sobre ajeno, ya cancelada → 409, persiste `Cancelled` sin borrar el registro.

### Decisiones

1. Sin tocar producción para testear (§7 cumplido sin excepciones): no se añadieron métodos de prueba, visibilidad ni dependencias de mocking.
2. La propiedad "update de otro profesional" se verifica como `NotFound`: es la respuesta real elegida en la fase 10 (evita revelar existencia del registro), documentado en el test.
3. Los fakes de listado/solape se implementaron por **contrato** documentado de la interfaz; el filtrado real en SQL sigue pendiente de tests de integración.

### Verificación (resultados reales)

```text
dotnet restore  → OK (6 proyectos)
dotnet build    → Build succeeded. 0 Warning(s), 0 Error(s)
dotnet test     → Reservation.Domain.Tests:         Passed 18, Failed 0
                  Reservation.Application.Tests:    Passed 55, Failed 0
                  TOTAL: 73 passed, 0 failed, 0 skipped
```

Ningún test destapó un bug de producción.

### Limitaciones

* **Sin tests de Infrastructure/Api** (integración con PostgreSQL, estados HTTP, JWT real): el bloqueo de entorno de la fase 4 (falta `POSTGRES_PASSWORD`, puerto 5432 ocupado, migraciones sin ejecutar) sigue vigente; esta fase los postergó por diseño.
* El SQL real de solape y filtrado no se ejecuta: los tests validan cómo el caso de uso reacciona al contrato del repositorio, no la consulta EF.

---

## Fase 12 — Validation and Global Error Handling (completada)

### Archivos creados/modificados

```text
src/Reservation.Application/Common/ConflictException.cs   # nuevo: abstracción de conflicto de unicidad

src/Reservation.Infrastructure/Data/
├── DatabaseConflict.cs                                   # nuevo: SaveChangesAsync + traducción SQLSTATE 23505 → ConflictException
└── Repositories/
    ├── UserRepository.cs                                 # AddAsync → DatabaseConflict (email único)
    └── ProfessionalServiceRepository.cs                  # AddAsync → DatabaseConflict (PK compuesta)

src/Reservation.Api/
├── Common/GlobalExceptionHandler.cs                     # nuevo: IExceptionHandler
├── Program.cs                                            # + AddExceptionHandler, UseExceptionHandler()
├── Features/Services/ServiceEndpoints.cs                 # 2 mapeos inline → ResultTranslation
└── Features/Professionals/ProfessionalEndpoints.cs       # 2 mapeos inline → ResultTranslation
```

### Estrategia de errores globales (§2)

* Mecanismo estándar .NET 10: implementación de `IExceptionHandler` (`GlobalExceptionHandler`), registrada con `AddExceptionHandler<T>()` y `app.UseExceptionHandler()` como **primer middleware**.
* **Excepción inesperada → 500** con `{"error": "An unexpected error occurred."}`; el detalle completo (tipo, stack, SQL) va **solo al log** (`ILogger.LogError` con método+ruta). Sin Serilog — logging estándar de ASP.NET Core.
* **`ConflictException` → 409** con `{"error": "<mensaje seguro de la aplicación>"}` (`LogWarning`).
* El handler nunca escribe `exception.Message` en la ruta genérica: solo la constante fija.

### Manejo de conflictos de base de datos (§3)

* Identificación de la violación de unicidad **solo en Infrastructure**: `DatabaseConflict.SaveChangesAsync` captura `DbUpdateException` cuyo inner es `PostgresException` con `SqlState = 23505` (`PostgresErrorCodes.UniqueViolation`) y relanza `ConflictException` (definido en `Application/Common`) con un mensaje escrito por la aplicación.
* Aplicado a las **dos** restricciones únicas reales del esquema (inspeccionadas: `grep IsUnique` → solo `Users.Email` + la PK de `ProfessionalServices`):
  * `UserRepository.AddAsync` → "Email is already registered." (idéntico al pre-check de Auth → misma respuesta 409).
  * `ProfessionalServiceRepository.AddAsync` → "The professional already offers this service." (idéntico al pre-check de la fase 9).
* Esto **cierra los dos huecos de carrera** documentados en fases 7 y 9 (registro duplicado y par duplicado ya no producen 500).
* Otras `DbUpdateException` (p. ej. FK) → 500 genérico, sin exponer nada de PostgreSQL.

### Revisión de validaciones (§5) — sin huecos reales

Cruce de los **16 `throw` de Dominio** con sus pre-validaciones en Application: User (nombre/email/contraseña → registro), Professional/Service (nombre, duración>0, precio≥0 → use cases), Availability (id, `start<end` → Create/Update), ProfessionalService (ids → Assign/Remove), Reservation (ids, `Cancel` doble → reglas de la fase 11). Todos cubiertos → **cero cambios en use cases** (sin FluentValidation, sin duplicación).

### Domain (§6)

Sin ajustes: cada invariante tiene pre-check en Application (tabla anterior); `Cancel()` pre-verificado → 409; el handler global es la red de seguridad ante errores de programación.

### Mapeos (§4/§7)

`ResultTranslation` intacto (400/401/403/404/409). Única corrección: `ServiceEndpoints` y `ProfessionalEndpoints` usaban `Results.BadRequest/NotFound` inline → centralizados en `ResultTranslation` (comportamiento idéntico, sin duplicación). Cuerpos preservados: los errores de `Result<T>` siguen devolviendo el mensaje como JSON string (comportamiento existente, §7); el 500/409 del handler usa `{"error": "..."}`.

### Verificación (resultados reales)

```text
dotnet restore / dotnet build            → 0 Warning(s), 0 Error(s)
refs                                    → Domain ∅; App→Domain; Infra→App+Domain; Api→App+Infra
grep paquetes prohibidos                 → sin FluentValidation/Serilog/MediatR
grep Npgsql/23505 en Domain+App+Api      → ninguno (conocimiento PostgreSQL solo en Infrastructure)
grep handler                             → solo constante fija + ConflictException.Message; LogError/LogWarning
grep ResultTranslation                   → 5 mapeos presentes; sin mapeos inline en Features
grep handler en Program.cs               → AddExceptionHandler + UseExceptionHandler (primer middleware)
migrations                               → sin cambios
Runtime (500/409 reales)                 → NO probado: PostgreSQL sigue inaccesible
```

### Limitaciones conocidas

* **Runtime sin probar:** el handler y la traducción 23505 se validaron solo por compilación/inspección (bloqueo PostgreSQL persistente: `POSTGRES_PASSWORD` sin definir, puerto 5432 ocupado).
* **Carreras lógicas sin restricción de BD:** solapes de disponibilidad (fase 10) y de reservas (fase 11) no tienen constraint en la BD → dos peticiones simultáneas pueden ambas pasar la consulta (no es un 500; es una carrera lógica, bloqueo de concurrencia fuera de alcance por diseño de la fase).
* **Cuerpos heterogéneos preservados por diseño:** `Result<T>` → JSON string; handler → `{"error":...}`; 401/403 → sin cuerpo (§7 exige preservar el comportamiento existente).
* Si la respuesta HTTP ya comenzó (`HasStarted`), el handler no intenta escribir (evita excepción secundaria).

---

## Fase 11 — Reservation Workflow (completada)

### Archivos creados/modificados

```text
src/Reservation.Domain/Entities/Reservation.cs       # + Cancel() (status → Cancelled + UpdatedAt); comentario obsoleto actualizado
src/Reservation.Domain/Entities/Availability.cs      # sin cambios netos (se restauró un espacio accidental)

src/Reservation.Application/
├── Abstractions/Persistence/IReservationRepository.cs  # + GetAllAsync, UpdateAsync (mínimo necesario)
└── Features/Reservations/ReservationUseCases.cs        # nuevo: List/GetById/Create/Cancel

src/Reservation.Infrastructure/Data/Repositories/ReservationRepository.cs  # + 2 métodos

src/Reservation.Api/Features/Reservations/ReservationEndpoints.cs          # nuevo
src/Reservation.Api/Program.cs                                             # + DI + MapReservationEndpoints
```

### Endpoints (todos con `.RequireAuthorization()` — autenticación requerida)

| Método y ruta | Client | Admin | Éxito |
| ------------- | ------ | ----- | ----- |
| `POST /api/reservations` | identidad propia; **403** si envía otro UserId | puede especificar `UserId` opcional | **201** |
| `GET /api/reservations` | solo los suyos | todos | 200 |
| `GET /api/reservations/{id}` | solo el suyo (**403** si es de otro) | cualquiera | 200 |
| `DELETE /api/reservations/{id}` | solo el suyo (**403**) | cualquiera | **204** |

### Las 10 reglas (§1) — dónde se cumplen

1. **Usuario existe y activo** → `IUserRepository.GetByIdAsync` + `IsActive` (404 / 400).
2. **Profesional existe y activo** → 404 / 400.
3. **Servicio existe y activo** → 404 / 400.
4. **Profesional ofrece el servicio** → `IProfessionalServiceRepository.ExistsAsync` → **409** (incompatibilidad con el estado actual; sin bypass).
5. **StartAt válido** → obligatorio, normalizado a UTC (Utc se conserva, Local se convierte, Unspecified se trata como UTC) y **no en el pasado** (`< DateTime.UtcNow` → 400; interpretación de "válido", reloj del servidor).
6. **EndAt derivado** → dominio: `EndAt = StartAt + DurationInMinutes`; `EndAt` **no** está en el DTO de entrada.
7. **Dentro de disponibilidad activa** → `GetByProfessionalAsync` (solo activos) + contención (`día` + `startTod >= p.Start` + `endTod <= p.End` + `startTod < endTod` que rechaza cruce de medianoche) → 400 si no cabe.
8. **Sin reserva en conflicto** → `IReservationRepository.GetConflictingAsync` (consulta en BD: `Status != Cancelled && StartAt < newEnd && EndAt > newStart`) → **409**.
9. **Pending** → dominio (`new Reservation` ⇒ `Pending`).
10. **Cancelada no bloquea** → el filtro `!= Cancelled` de la consulta existente.

### Convención de tiempo (§4/§16)

* `StartAt`/`EndAt`: **UTC** (`DateTime` existente conservado — cambiar a `DateTimeOffset` no era necesario ni seguro sin migración). El DTO acepta cualquier `Kind` y se normaliza una sola vez.
* Disponibilidad: `DayOfWeek` + `TimeOnly` **se comparan directamente contra los timestamps UTC** — zona horaria de negocio implícita = UTC. **Limitación documentada:** un sistema multi-zona real requiere una zona explícita por profesional/negocio (fuera de alcance, sin librerías de zona horaria).
* Sin infraestructura de husos; nada de hora local del servidor.

### Decisiones

1. **UserId en creación:** Client → siempre `ICurrentUser.UserId`; si un Client envía un UserId distinto → **403** (no se ignora en silencio, no se crea para otro). Admin → puede especificar `UserId` opcional (si no, el suyo) — para testing/administración básica; sin dashboard de administración.
2. **Matriz de errores:** 401 (no autenticado, red de seguridad), 403 (propiedad/UserId ajeno), 404 (entidad inexistente), 400 (campos, entidades inactivas, pasado, fuera de disponibilidad), 409 (no ofrece el servicio, solape de reservas, ya cancelada).
3. **Cancelación:** `DELETE` = **baja lógica** vía `Reservation.Cancel()` (dominio). Si ya estaba cancelada → **409**. El dominio también lanza excepción si se llama dos veces (la use case lo pre-verifica → nunca se dispara). Nunca `Remove` en el repositorio.
4. **Repositorio:** solo `+ GetAllAsync` (listado Admin) y `+ UpdateAsync` (cancelación; attach+SaveChanges sobre entidad desadjunta). `GetConflictingAsync` y `GetByUserIdAsync` ya existían. Lecturas con `AsNoTracking`.
5. **Listado:** un único `ListAsync` que ramifica según `ICurrentUser.IsAdmin` en Application (misma pauta de propiedad de la fase 8); no se mezclan checks de rol en endpoints.
6. **Sin acciones nuevas en Admin** más allá de listar/ver/cualquier-cancelación (§13 tabla).

### Verificación (resultados reales)

```text
dotnet restore / dotnet build            → 0 Warning(s), 0 Error(s)
grep EF/AspNet en Application            → OK (ninguno)
grep DbContext en Api                    → OK (ninguno)
grep GetConflictingAsync                 → use case + implementación en BD (fórmula + != Cancelled)
grep propiedad en Application            → IsOwner/IsAdmin/Forbidden en List/GetById/Create/Cancel
grep Remove en repositorio de reservas   → ninguno; solo reservation.Cancel()
grep EndAt en CreateReservationRequest   → ausente (calculado en dominio)
grep Pending/EndAt                       → dominio: AddMinutes(DurationInMinutes) + Status = Pending
grep authorization                       → los 4 endpoints con RequireAuthorization()
migrations                               → sin cambios
Runtime (201/200/204/403/409 reales)     → NO probado: PostgreSQL sigue inaccesible
```

### Limitaciones conocidas

* **Concurrencia (§15):** dos reservas simultáneas pueden pasar ambas la consulta de conflicto antes de que ninguna commitee → sin bloqueos ni exclusion constraints (explícitamente fuera de alcance; fase 12+ si se requiere).
* **Zona horaria:** convención UTC única (arriba); disponibilidad semanal repetitiva no modela DST ni zonas por profesional.
* **Reloj del servidor** para "StartAt en el futuro" (`DateTime.UtcNow`).
* **Sin runtime de prueba:** traducción EF y flujos 401/403/409 verificados solo por compilación/inspección (bloqueo PostgreSQL persistente).
* La lista no tiene ordenación ni paginación (no solicitado).

---

## Fase 10 — Professional Availability (completada)

### Archivos creados/modificados

```text
src/Reservation.Domain/Entities/Availability.cs        # + Update(day, start, end) — único cambio de Dominio

src/Reservation.Application/
├── Abstractions/Persistence/IAvailabilityRepository.cs # + GetByIdAsync, UpdateAsync, HasOverlapAsync
└── Features/Availability/AvailabilityUseCases.cs       # + UpdateAsync, DeactivateAsync; overlap en Create

src/Reservation.Infrastructure/Data/Repositories/AvailabilityRepository.cs
    # + 3 métodos; GetByProfessionalAsync ahora filtra IsActive; overlap como consulta AnyAsync en BD

src/Reservation.Api/Features/Availability/AvailabilityEndpoints.cs
    # + PUT/DELETE {availabilityId:guid}, + UpdateAvailabilityRequest, traducción vía ResultTranslation
```

### Endpoints (authorization sin cambios + nuevos)

| Método y ruta | Auth | Éxito | Fallos |
| ------------- | ---- | ----- | ------ |
| `GET .../availability` | **Público** (sin cambios) | 200, solo activos, orden DayOfWeek→StartTime | — |
| `POST .../availability` | Admin (sin cambios) | 201 | 400, 404, **409** (solape) |
| `PUT .../availability/{availabilityId}` | **Admin (nuevo)** | 200 con registro actualizado | 400, 404 (profesional/registro/propiedad), **409** |
| `DELETE .../availability/{availabilityId}` | **Admin (nuevo)** | **204** (baja lógica) | 400, 404 |

### Regla de solape (§5)

Se rechaza si `existing.StartTime < newEndTime && existing.EndTime > newStartTime` entre registros **activos** del mismo profesional/día. Verificado con los ejemplos de la fase: rechaza 08–10, 10–11, 11–13, 09–12; admite 08–09 y 12–13 (adyacencia permitida por desigualdades estrictas).

### Decisiones

1. **Mutador mínimo en Dominio:** `Availability.Update(DayOfWeek, TimeOnly, TimeOnly)` — necesario porque los setters son privados; valida `endTime > startTime` (invariante intacto, §11). `IsActive` usa los métodos ya existentes `Activate()`/`Deactivate()`. `ProfessionalId` jamás se modifica (§2.6).
2. **Repositorio (§4/§6):** solo `GetByIdAsync` (AsNoTracking), `UpdateAsync` (attach + SaveChanges, funciona con entidad desadjunta) y `HasOverlapAsync` — la consulta de solape corre **en la BD** (`AnyAsync` con las tres condiciones + `IsActive` + `excludeAvailabilityId` opcional). Sin `Remove`: **nunca borrado físico**.
3. **Filtro de activos (§8):** `GetByProfessionalAsync` del repositorio ahora incluye `a.IsActive` (el único consumidor es el GET público). La documentación del contrato lo indica explícitamente.
4. **Orden (§9):** `DayOfWeek` se persiste como **texto** (`HasConversion<string>`), así que `ORDER BY` en SQL sería alfabético (Friday < Monday < ...) y no calendario — el orden se aplica en Application con LINQ-to-objects (`OrderBy(DayOfWeek).ThenBy(StartTime)`) sobre la enumeración ya filtrada por la BD. "Where practical" no aplica al criterio día-semana con almacenamiento textual y sin runtime de prueba.
5. **Solape en PUT solo si el registro queda activo:** si `IsActive=false` no se consulta solape (desactivar nunca puede quedar bloqueado por un 409 — un registro inactivo no genera conflictos). En CREATE el registro nace activo → siempre se consulta. En PUT se pasa `excludeAvailabilityId = availabilityId` (§6.2).
6. **PUT con desajuste de propiedad → 404** (mismo criterio que DELETE en §3): no se revela la existencia de registros de otro profesional.
7. **DTOs:** `CreateAvailabilityRequest` existente intacto; nuevo `UpdateAvailabilityRequest(DayOfWeek, StartTime, EndTime, IsActive)`; respuesta reutiliza `AvailabilityResponse`. `AvailabilityEndpoints` ahora usa el traductor compartido `ResultTranslation` (necesario para el nuevo 409).
8. **Dominio:** solo el método `Update`; sin motor de scheduling ni cambios en otras entidades. **Sin migración** (esquema suficiente).

### Verificación (resultados reales)

```text
dotnet restore / dotnet build            → 0 Warning(s), 0 Error(s)
grep EF/DbContext en Application         → OK (ninguno)
grep DbContext en Api                    → OK (ninguno)
grep HasOverlapAsync en Infrastructure   → fórmula exacta + IsActive + excludeAvailabilityId
grep filtro activos                      → Where(... && a.IsActive) en GetByProfessionalAsync
grep Remove en repositorio               → ninguno; solo Deactivate() en use cases
grep authorization                       → GET público; POST/PUT/DELETE con Policies.AdminOnly
grep invariante Dominio                  → startTime<endTime en ctor y en Update
migrations                               → sin cambios (InitialCreate intacto)
Runtime (201/200/204/409 reales)         → NO probado: PostgreSQL sigue inaccesible
```

### Limitaciones conocidas

* PostgreSQL inaccesible (POSTGRES_PASSWORD sin definir; puerto 5432 ocupado) → la traducción de `HasOverlapAsync` a SQL (TimeOnly comparables, DayOfWeek con conversor de texto) se validó solo por compilación, no en runtime — riesgo residual de traducción de EF.
* Carrera `HasOverlapAsync`→`AddAsync` (dos POST simultáneos con solape) → sin índice de exclusión en la BD, ambos podrían pasar → el bloqueo definitivo de solapes es responsabilidad de la fase 11 (validación al reservar) + posible índice parcial.
* El orden por día de la semana se hace en memoria (decisión 4); correcto para volúmenes pequeños.

---

## Fase 9 — Professional Services Management (completada)

### Archivos creados/modificados

```text
src/Reservation.Application/
├── Abstractions/Persistence/IProfessionalServiceRepository.cs  # + DeleteAsync (método mínimo, §5)
└── Features/Professionals/ProfessionalServiceUseCases.cs       # nuevo: GetServices/Assign/Remove

src/Reservation.Infrastructure/Data/Repositories/ProfessionalServiceRepository.cs  # + DeleteAsync

src/Reservation.Api/
├── Features/Professionals/ProfessionalServiceEndpoints.cs      # nuevo
└── Program.cs                                                  # + DI + MapProfessionalServiceEndpoints
```

### Endpoints (los tres con `Policies.AdminOnly`)

| Método y ruta | Éxito | Fallos |
| ------------- | ----- | ------ |
| `GET /api/professionals/{id}/services` | 200 con `ServiceResponse[]` | 400 (id vacío), 404 (profesional) |
| `POST /api/professionals/{id}/services` | **201** con `ServiceResponse` | 400, 404, **409** (ya ofrece el servicio) |
| `DELETE /api/professionals/{id}/services/{serviceId}` | **204** sin cuerpo | 400, 404 (profesional o relación inexistente) |

### Validaciones (en orden, §3/§4)

* **Assign:** professionalId/serviceId no vacíos → profesional existe → servicio existe → profesional activo → servicio activo → relación no duplicada (`ExistsAsync` → `Conflict`/409). Persiste con `IProfessionalServiceRepository.AddAsync`.
* **Remove:** ids no vacíos → profesional existe → relación existe (si no → 404) → `DeleteAsync`.
* **Get:** id no vacío → profesional existe (si no → 404) → `GetByProfessionalAsync` y resolución de cada par a su `Service` con `IServiceRepository.GetByIdAsync`.

### Decisiones

1. **Reutilización de modelo de respuesta:** se devuelve `ServiceResponse` (el DTO público existente en `Features/Services/ServiceEndpoints.cs`, exactamente la forma pedida: Id/Name/Description/DurationInMinutes/Price/IsActive). No se duplicó DTO; `AssignServiceRequest` (solo `serviceId`) es nuevo porque su forma es distinta de `CreateServiceRequest`.
2. **Contrato de repositorio:** solo se añadió `DeleteAsync(Guid, Guid)` a `IProfessionalServiceRepository` (§5). Implementación: `FindAsync` por clave compuesta → `Remove` + `SaveChangesAsync`; **tolerante a carrera** (si la fila ya no existe, no hace nada) porque la comprobación previa `ExistsAsync` y el borrado no son atómicos. El resto del contrato y de los repositorios quedó intacto.
3. **Clase de casos de uso nueva:** `ProfessionalServiceUseCases` (convención una-clase-por-entidad de la fase 6), inyecta los tres repositorios existentes (`IProfessionalRepository`, `IServiceRepository`, `IProfessionalServiceRepository`); sin `DbContext` en Application.
4. **Unicidad:** pre-check en Application (`ExistsAsync` → 409) **más** la clave compuesta existente `HasKey(ps => new { ProfessionalId, ServiceId })` como red de base de datos. Sin reglas de dominio nuevas; entidad `ProfessionalService` sin cambios.
5. **GET con N+1 deliberado:** resolver cada par con `GetByIdAsync` usa solo contratos existentes (una consulta por servicio ofrecido, PK indexada, `AsNoTracking`, catálogo pequeño). Se evita añadir `GetByIdsAsync` o rediseñar `GetByProfessionalAsync` por alcance de fase; optimizar si crece la escala.
6. **Sin cambios** en Availability, reservas, JWT, roles, Docker ni migraciones (el esquema no cambió).

### Verificación (resultados reales)

```text
dotnet restore / dotnet build        → 0 Warning(s), 0 Error(s)
refs                                → Domain ∅; App→Domain; Infra→App+Domain; Api→App+Infra
grep DbContext/EF en Api+Application → solo un comentario (Program.cs); sin uso real
grep endpoints nuevos                → MapGet/MapPost/MapDelete los tres con RequireAuthorization(Policies.AdminOnly)
grep validaciones use case           → todas las rutas de §3/§4 presentes (400/404/409)
grep DeleteAsync                     → en contrato (Application) e implementación (Infrastructure)
grep Availability/Auth/Security      → sin modificaciones en esta fase (mtimes de la fase 8)
migrations                           → sin cambios (PK compuesta ya existía)
Runtime (201/204/409 reales)         → NO probado: PostgreSQL sigue inaccesible
```

### Limitaciones conocidas

* PostgreSQL inaccesible (POSTGRES_PASSWORD sin definir; puerto 5432 ocupado) → verificación solo por compilación e inspección estática.
* `GetServicesAsync` resuelve servicios con una consulta por par (N pequeño); ver decisión 5.
* Carrera `ExistsAsync`→`AddAsync` en el POST simultáneo del mismo par → índice único de la BD → 500 (el manejo global de errores es la fase 12).

---

## Fase 8 — Roles and Authorization (completada)

### Archivos creados/modificados

```text
src/Reservation.Application/
├── Abstractions/Services/ICurrentUser.cs        # + bool IsAdmin
├── Common/Result.cs                             # + ErrorKind.Forbidden + factory
└── Features/Users/UserUseCases.cs               # + ICurrentUser, regla de propiedad

src/Reservation.Infrastructure/Security/CurrentUser.cs   # + IsAdmin (IsInRole "Admin")

src/Reservation.Api/
├── Common/Policies.cs                           # const AdminOnly
├── Common/ResultTranslation.cs                  # traducción compartida 400/401/403/404/409
├── Program.cs                                   # + política AdminOnly, MapInboundClaims=false
├── Features/Auth/AuthEndpoints.cs               # usa ResultTranslation (helper privado eliminado)
├── Features/Services/ServiceEndpoints.cs        # POST → AdminOnly
├── Features/Professionals/ProfessionalEndpoints.cs # POST → AdminOnly
├── Features/Availability/AvailabilityEndpoints.cs  # POST → AdminOnly
└── Features/Users/UserEndpoints.cs              # GET → RequireAuthorization()
```

### Reglas de autorización implementadas

| Endpoints | Requisito |
| --------- | --------- |
| `POST /api/auth/register`, `POST /api/auth/login` | Público (sin `RequireAuthorization`) |
| `GET /api/services`, `GET /api/services/{id}` | Público (catálogo para navegación) |
| `GET /api/professionals`, `GET /api/professionals/{id}` | Público |
| `POST /api/services`, `POST /api/professionals`, `POST /api/professionals/{id}/availability` | `AdminOnly` |
| `GET /api/users/{id}` | Autenticado + regla de propiedad en Application |

### Decisiones

1. **Mapeo de rol:** el JWT ya emitía `ClaimTypes.Role`; se añadió `options.MapInboundClaims = false` en `AddJwtBearer` para que los claims (ya con tipos ASP.NET Core) pasen sin transformación → `IsInRole("Admin")` funciona de forma determinista. **Token e hashing sin cambios.**
2. **Una sola política:** `AdminOnly = RequireRole("Admin")` definida en `AddAuthorization`; referenciada por la constante `Policies.AdminOnly`. Sin framework propio, sin middleware de roles, sin `FallbackPolicy` (los endpoints no listados quedan públicos por defecto).
3. **Regla de propiedad de `/api/users/{id}`:** HTTP exige autenticación (`.RequireAuthorization()`); `UserUseCases.GetByIdAsync` inyecta `ICurrentUser` y devuelve `Forbidden` (403) si `!IsAdmin && UserId != id` **antes** de leer la BD (no revela existencia). Admin → cualquier usuario. `ICurrentUser` solo se extendió con `bool IsAdmin`.
4. **Traducción HTTP compartida:** nuevo `Api/Common/ResultTranslation.cs` (400/401/403/404/409) usado por Auth y Users; `AuthEndpoints.ToHttpError` privado eliminado para no duplicar.
5. **Registro seguro:** `RegisterRequest` no tiene campo de rol; `AuthUseCases` asigna `UserRole.Client` por código. Sin endpoint de auto-promoción a Admin.
6. **Admin para pruebas manuales (temporal, documentado):** no hay endpoint ni seed de Admin. Crear uno en desarrollo ejecutando en PostgreSQL (el rol se guarda como texto):
   ```sql
   -- tras registrar el usuario por POST /api/auth/register
   UPDATE "Users" SET "Role" = 'Admin' WHERE "Email" = 'admin@example.com';
   ```
   Método transitorio, no es una bajada de seguridad permanente.
7. **Endpoint no especificado:** `GET /api/professionals/{id}/availability` no aparece en las listas de la fase 8 → permanece **público** (lectura de navegación, coherente con el catálogo). Revisar si la fase 11 lo requiere autenticado.

### Verificación (resultados reales)

```text
dotnet restore / dotnet build        → 0 Warning(s), 0 Error(s)
refs                                → Domain ∅; App→Domain; Infra→App+Domain; Api→App+Infra
grep HttpContext/JWT en Application  → OK (ninguno)
grep endpoint authorization map      → 3 POST exactamente AdminOnly; 4 GET catálogo sin requisito;
                                      register/login públicos; GET users con RequireAuthorization
grep Program.cs                      → MapInboundClaims=false, AddPolicy RequireRole("Admin"),
                                      UseAuthentication → UseAuthorization (orden correcto)
grep rol en registro                 → sin campo Role en request; UserRole.Client hardcodeado
grep FallbackPolicy/AllowAnonymous   → ninguno (sin bypasses)
migrations                           → sin cambios (sin esquema nuevo)
Runtime (401/403 reales)             → NO probado: PostgreSQL sigue inaccesible
```

### Limitaciones conocidas

* PostgreSQL inaccesible (POSTGRES_PASSWORD sin definir; puerto 5432 ocupado) → autorización verificada solo por compilación e inspección estática.
* `GET /api/professionals/{id}/availability` queda público (no especificado; ver decisión 7).
* Sin endpoint de creación de Admin (método SQL temporal documentado).

---

## Fase 7 — Authentication and JWT (completada)

### Archivos creados/modificados

```text
src/Reservation.Application/
├── Abstractions/Services/IPasswordHasher.cs     # Hash/Verify
├── Abstractions/Services/ITokenService.cs       # + record TokenResult
├── Common/Result.cs                             # + ErrorKind.Unauthorized/Conflict + factories
└── Features/Auth/
    ├── AuthUseCases.cs                          # RegisterAsync + LoginAsync
    └── LoginResult.cs                           # AccessToken + ExpiresAtUtc + UserModel

src/Reservation.Infrastructure/Security/
├── JwtSettings.cs                               # config tipada (sección "Jwt")
├── PasswordHasher.cs                            # PBKDF2-HMAC-SHA256
├── TokenService.cs                              # JWT HMAC-SHA256
└── CurrentUser.cs                               # ICurrentUser desde HttpContext.User
src/Reservation.Infrastructure/DependencyInjection.cs  # + config Jwt + hasher/token/current-user
src/Reservation.Infrastructure/Reservation.Infrastructure.csproj  # + FrameworkReference AspNetCore.App, + System.IdentityModel.Tokens.Jwt 8.23.0, − 2 packages Microsoft.Extensions redundant
src/Reservation.Api/Features/Auth/AuthEndpoints.cs     # POST /api/auth/register, POST /api/auth/login
src/Reservation.Api/Program.cs                 # + JWT bearer, + AuthUseCases, + UseAuthentication/UseAuthorization
src/Reservation.Api/appsettings.json            # + sección Jwt (placeholder dev)
src/Reservation.Api/Reservation.Api.csproj      # + Microsoft.AspNetCore.Authentication.JwtBearer 10.0.12
```

### Decisiones

1. **Password hashing:** PBKDF2-HMAC-SHA256 propio (`Rfc2898DeriveBytes.Pbkdf2`), 600 000 iteraciones (recomendación OWASP), salt aleatorio de 128 bits, comparación en tiempo fijo (`CryptographicOperations.FixedTimeEquals`). Formato `PBKDF2-SHA256$iteraciones$salt$hash`. Sin librerías extra; nunca texto plano; hashing solo en Infrastructure.
2. **JWT:** `ITokenService` en Application; `TokenService` en Infrastructure emite claims `ClaimTypes.NameIdentifier` (ID), `ClaimTypes.Email`, `ClaimTypes.Role` (+ `jti`), HMAC-SHA256, issuer/audience/expiración desde `JwtSettings`.
3. **Configuración:** sección `Jwt` (`SecretKey`, `Issuer`, `Audience`, `ExpirationMinutes`). En `appsettings.json` solo un **placeholder DEV obviamente falso** (≥32 chars); producción debe inyectar `Jwt__SecretKey` (validación al arrancar: sección presente y clave ≥32 caracteres → fallo temprano). Sin secretos reales en el repositorio.
4. **Registro** (`POST /api/auth/register`): valida name/email/password → email duplicado → `Conflict` (409) → hash → `User` con rol **Client** → `IUserRepository`. Responde `UserModel` (201) sin `PasswordHash`. Sin endpoint admin de registro.
5. **Login** (`POST /api/auth/login`): mensaje único `"Invalid email or password."` para email inexistente o contraseña incorrecta (sin enumeración de cuentas); `"Your account is inactive."` solo con contraseña correcta; inactive → 401. Éxito → 200 con `LoginResult` (token, expiración, `UserModel` con rol).
6. **`ErrorKind`:** +`Unauthorized` (→401), +`Conflict` (→409); traducción HTTP en `AuthEndpoints.ToHttpError`.
7. **`ICurrentUser`:** implementado en Infrastructure (`CurrentUser`) leyendo `HttpContext.User`; sin sesión → `Guid.Empty`/`IsAuthenticated=false`. Registrado con `AddHttpContextAccessor` en `AddInfrastructure`. Sin políticas de autorización aún (fase 8).
8. **Middleware order:** `UseAuthentication()` → `UseAuthorization()` antes de mapear endpoints. Endpoints existentes **no** protegidos (fase 8).
9. **Paquetes (necesarios, no evitables):** `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.12 y `System.IdentityModel.Tokens.Jwt` 8.23.0 **no** están en el shared framework de .NET 10. Con el `FrameworkReference` añadido, los packages `Microsoft.Extensions.{Configuration,DependencyInjection}.Abstractions` quedaron redundantes (NU1510) y se eliminaron.
10. **Dominio sin cambios** (User ya tenía `PasswordHash`). **Sin migración:** el esquema no cambió.

### Verificación (resultados reales)

```text
dotnet restore / dotnet build        → 0 Warning(s), 0 Error(s)
refs (Domain/Application/Infra/Api)  → correctas; Application sin ASP.NET/EF/JWT types
grep secretos en *.cs                → ninguno (solo placeholder en appsettings.json)
grep PasswordHash en Api             → solo comentario "never exposed"
grep AddAuthentication/AddJwtBearer/UseAuthentication → presentes en Program.cs
migrations                           → sin cambios (InitialCreate intacto)
Runtime (login/register)             → NO probado: PostgreSQL sigue inaccesible
```

### Limitaciones conocidas (siguen de fases 4/7)

* PostgreSQL inaccesible (`POSTGRES_PASSWORD` sin definir; puerto 5432 ocupado por `postgres-server`) → login/registro verificados solo por compilación e inspección estática, no en runtime.
* Sin protección de endpoints (fase 8), sin refresh tokens/lockout/reset (explícitamente fuera de alcance).
* Carrera en registro simultáneo del mismo email → violación de índice único → 500 (el manejo de errores global es fase 12).

---

### Archivos creados/modificados

```text
src/Reservation.Application/
├── Common/
│   └── Result.cs                        # Result<T> + ErrorKind (primero en Common/)
└── Features/
    ├── Services/ServiceUseCases.cs
    ├── Professionals/ProfessionalUseCases.cs
    ├── Availability/AvailabilityUseCases.cs
    └── Users/
        ├── UserUseCases.cs
        └── UserModel.cs                 # modelo seguro (sin PasswordHash)

src/Reservation.Api/
├── Program.cs                           # + registro de los 4 use cases (AddScoped)
└── Features/…/*Endpoints.cs             # refactorizados: llaman use cases, no repositorios
```

### Uso cases creados

| Clase | Operaciones |
| ----- | ----------- |
| `ServiceUseCases` | `GetAllAsync`, `GetByIdAsync`, `CreateAsync` (valida name, duración > 0, precio ≥ 0) |
| `ProfessionalUseCases` | `GetAllAsync`, `GetByIdAsync`, `CreateAsync` (valida name) |
| `AvailabilityUseCases` | `GetByProfessionalAsync`, `CreateAsync` (valida Guid profesional, profesional existe → NotFound, `StartTime < EndTime`) |
| `UserUseCases` | `GetByIdAsync` → `Result<UserModel>` |

### Decisiones

1. **`Result<T>` mínimo** (`Common/Result.cs`): `IsSuccess`, `Value`, `Error`, `Kind` (`Validation`/`NotFound`). Factories `Success`/`Failure`/`NotFound`. Sin framework de resultados; el API traduce `Kind` → 400/404.
2. **Validación en Application, no en API:** los endpoints ya no contienen reglas; solo binding HTTP, llamada al use case, traducción de `Result` a HTTP y mapeo a DTOs. La validación pre-existe al constructor de dominio para convertir entradas inválidas en fallos esperados (400) en vez de excepciones (500) — sin duplicar reglas que el dominio ya garantiza.
3. **Reglas cross-entity** (profesional existente antes de crear disponibilidad) viven en Application según la fase las especifica. Sin detección de solapes (el contrato de repositorio no la soporta).
4. **Modelos:** los use cases devuelven entidades de dominio directamente (sin duplicar modelos); solo `UserModel` es un modelo nuevo y justificado (PasswordHash nunca sale de Application). `UserResponse` de la fase 5 se eliminó: `GET /api/users/{id}` devuelve `UserModel` (evita DTO duplicado idéntico). DTOs de request/response de Services/Professionals/Availability permanecen en API.
5. **DI:** los 4 use cases se registran `AddScoped` en `Program.cs` (sin añadir paquetes a Application — sigue con 0 packages). Registro de Infrastructure sin cambios; sin dependencias circulares.
6. **Namespace:** alias `AvailabilityEntity` en `AvailabilityUseCases.cs` (mismo choque de nombre de siempre).

### Verificación (resultados reales)

```text
dotnet build Reservation.slnx                → Build succeeded. 0 Warning(s), 0 Error(s)
dotnet list reference (Domain)               → ninguna
dotnet list reference (Application)          → solo Reservation.Domain
dotnet list reference (Infrastructure)       → Application + Domain
dotnet list reference (Api)                  → Application + Infrastructure
grep ASP.NET/EF/HttpContext/IResult en App   → sin coincidencias
grep "Repository" en src/Reservation.Api     → sin coincidencias (endpoints usan use cases)
dotnet list package (Application)            → sin packages
```

### Limitaciones conocidas (siguen de fase 4)

* PostgreSQL inaccesible (falta `POSTGRES_PASSWORD`; puerto 5432 ocupado por `postgres-server`) → sin verificación en runtime de endpoints/use cases; solo compilación y arquitectura.

---

### Archivos creados/modificados

```text
src/Reservation.Api/
├── Program.cs                                  # + enum-as-string JSON + mapeo de 4 grupos de endpoints
└── Features/
    ├── Services/ServiceEndpoints.cs
    ├── Professionals/ProfessionalEndpoints.cs
    ├── Availability/AvailabilityEndpoints.cs
    └── Users/UserEndpoints.cs
```

### Endpoints

| Método | Ruta | Respuesta |
| ------ | ---- | --------- |
| GET | `/api/services` | 200 lista |
| GET | `/api/services/{id}` | 200 / 404 |
| POST | `/api/services` | 201 + Location / 400 |
| GET | `/api/professionals` | 200 lista |
| GET | `/api/professionals/{id}` | 200 / 404 |
| POST | `/api/professionals` | 201 + Location / 400 |
| GET | `/api/professionals/{professionalId}/availability` | 200 lista |
| POST | `/api/professionals/{professionalId}/availability` | 201 + Location / 404 (profesional) / 400 |
| GET | `/api/users/{id}` | 200 / 404 |

### DTOs

`ServiceResponse`/`CreateServiceRequest`, `ProfessionalResponse`/`CreateProfessionalRequest`, `AvailabilityResponse`/`CreateAvailabilityRequest`, `UserResponse`. Todos como `record` en el mismo archivo de su feature. **`UserResponse` no expone `PasswordHash`.**

### Decisiones

1. **Feature-based** en `Features/<Feature>/*Endpoints.cs`: cada archivo expone `MapXxxEndpoints(this IEndpointRouteBuilder)` y `Program.cs` solo los invoca. Sin controllers.
2. **Handlers** como métodos `private static Task<IResult>`: retorno explícito `IResult` (evita problemas de inferencia de tipos de `Results.*` y da flexibilidad de respuesta).
3. **Validación manual** (sin framework): Name requerido, `DurationInMinutes > 0`, `Price >= 0`, `StartTime < EndTime` → 400 con mensaje; espejo exacto de los invariantes de dominio.
4. **Repositorios vía DI** (`IServiceRepository`…): cero inyección de `ReservationDbContext` desde la API (verificado por grep); sin segundo registro de DbContext.
5. **`POST availability` verifica que el profesional exista** (404) para evitar violación de FK; sin detección de solapes (no soportada aún por el contrato).
6. **Enums como strings en JSON** (`JsonStringEnumConverter` en `ConfigureHttpJsonOptions`): `Role` → `"Client"`, `DayOfWeek` → `"Monday"`.
7. **OpenAPI:** no existía configuración que conservar (plantilla `web` sin `AddOpenAPI`); no se añadió.
8. **Alias de namespace:** `AvailabilityEntity` en el archivo de Availability (el simple name `Availability` resolvería al namespace `Reservation.Api.Features.Availability`).

### Verificación (resultados reales)

```text
dotnet restore Reservation.slnx            → OK
dotnet build Reservation.slnx              → Build succeeded. 0 Warning(s), 0 Error(s)
grep ReservationDbContext en Api            → sin coincidencias
grep PasswordHash en Api                    → solo comentario "never exposed"
dotnet list reference (Api)                → solo Application + Infrastructure
Pruebas/E2E con BD                          → no ejecutables (bloqueo PostgreSQL de fase 4)
```

### Limitaciones conocidas (fase 5)

* Los endpoints **no se probaron en runtime** porque PostgreSQL sigue inaccesible (bloqueo documentado en fase 4: falta `POSTGRES_PASSWORD` y el puerto 5432 está ocupado). La verificación fue de compilación y arquitectura.
* Sin autenticación/autorización, sin reservations, sin manejo de errores global — por diseño (fases 7, 8, 11, 12).

---

### Archivos creados/modificados

```text
src/Reservation.Infrastructure/
├── Data/
│   ├── ReservationDbContext.cs            # 6 DbSets + ApplyConfigurationsFromAssembly
│   ├── Configurations/
│   │   ├── UserConfiguration.cs
│   │   ├── ServiceConfiguration.cs
│   │   ├── ProfessionalConfiguration.cs
│   │   ├── ProfessionalServiceConfiguration.cs
│   │   ├── AvailabilityConfiguration.cs
│   │   └── ReservationConfiguration.cs
│   └── Repositories/
│       ├── UserRepository.cs
│       ├── ServiceRepository.cs
│       ├── ProfessionalRepository.cs
│       ├── ProfessionalServiceRepository.cs
│       ├── AvailabilityRepository.cs
│       └── ReservationRepository.cs
├── DependencyInjection.cs                 # + registros Scoped de los 6 repositorios
├── Migrations/20260928172254_InitialCreate.cs (+ Designer + snapshot)
src/Reservation.Api/Reservation.Api.csproj # + Microsoft.EntityFrameworkCore.Design 10.0.4 (dev-only)
dotnet-tools.json                          # tool local dotnet-ef 10.0.4
src/Reservation.Domain/Entities/*.cs       # + constructores parameterless privados (reconstitución)
```

### Decisiones

1. **Proveedor:** `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3 (EF Core 10.0.4). El DbContext y el registro DI existentes se reutilizaron; no hay segundo DbContext ni segundo mecanismo de DI.
2. **Tablas:** `Users`, `Services`, `Professionals`, `ProfessionalServices`, `Availabilities`, `Reservations` (PK `Id` Guid; `ProfessionalServices` con PK compuesto `ProfessionalId + ServiceId`, que además garantiza la unicidad exigida por el dominio §6).
3. **Enums en string (convención estable y legible):** `Role` → varchar(50), `Status` → varchar(50), `DayOfWeek` → varchar(20). Sin tablas para enums. La representación string es inmune a reordenaciones numéricas de los enums.
4. **Delete behavior: `Restrict` en las 6 relaciones, nunca `Cascade`.** Borrar un usuario, profesional o servicio no puede borrar reservas históricas; el baja es lógica vía `IsActive` (coincide con el modelo de negocio). También Restrict en `Availability` y `ProfessionalService`.
5. **Fechas:** `CreatedAt`/`UpdatedAt`/`StartAt`/`EndAt` → `timestamp with time zone` (UTC). Los casos de uso deberán proporcionar `DateTime` con `Kind=Utc` para `StartAt`/`EndAt` (decisión de zona horaria del dominio §14: almacenamiento en UTC). Auditoría usa `DateTime.UtcNow`.
6. **Money:** `Service.Price` → `numeric(10,2)` (`decimal`, nunca float/double). `TimeOnly` → `time without time zone`.
7. **Índices:** `IX_Users_Email` (único), `IX_Availabilities_ProfessionalId`, `IX_Reservations_UserId/_ProfessionalId/_ServiceId` (los de FK también los crea la convención de EF).
8. **Repositorios:** implementan exactamente los contratos de Application (sin métodos nuevos). Consultas de lectura con `AsNoTracking`; los métodos mutantes hacen `SaveChangesAsync` propio (no hay `IUnitOfWork` por decisión de la fase 3 — el commit es por operación).
9. **`GetConflictingAsync`:** `ProfessionalId = @ AND Status != Cancelled AND StartAt < endAt AND EndAt > startAt` — solape exacto de la Regla 7, reservas canceladas no bloquean (Regla 10).
10. **Domain tocado mínimo:** se añadió un constructor parameterless `private` por entidad (reconstitución de EF; `ProfessionalService` pasó de `{ get; }` a `{ get; private set; }` por la misma razón). Sin atributos EF ni dependencias en Domain — sigue persistence-ignorant.
11. **Design package** solo en Api (startup, `PrivateAssets=all`); migraciones viven en Infrastructure.

### Migración

* Nombre: **`InitialCreate`** (`20260928172254_InitialCreate.cs`), generada con `dotnet ef` local.
* Representa las 6 entidades, sin tablas/columnas especulativas. Contenido verificado: 6 `CreateTable`, PKs (incl. compuesto), FKs `onDelete: Restrict`, índices, `unique: true` en `IX_Users_Email`.

### Verificación (resultados reales)

```text
dotnet restore Reservation.slnx             → OK
dotnet build Reservation.slnx               → Build succeeded. 0 Warning(s), 0 Error(s)
dotnet list reference (Application)         → solo Reservation.Domain
dotnet list reference (Domain)              → sin referencias
grep EF/DataAnnotations en Domain           → limpio (persistence-ignorant)
dotnet ef migrations add InitialCreate      → Done (Build succeeded)
dotnet ef database update                   → NO EJECUTADO (ver Pendientes)
```

---

### Estructura creada

```text
src/Reservation.Application/
├── Common/                        # reservado (vacío hasta tener un uso concreto)
├── Abstractions/
│   ├── Persistence/               # contratos de persistencia
│   │   ├── IUserRepository.cs
│   │   ├── IServiceRepository.cs
│   │   ├── IProfessionalRepository.cs
│   │   ├── IProfessionalServiceRepository.cs
│   │   ├── IAvailabilityRepository.cs
│   │   └── IReservationRepository.cs
│   └── Services/
│       └── ICurrentUser.cs
└── Features/                      # reservado por feature (Users, Services,
                                    # Professionals, Availability, Reservations)
```

### Decisiones

1. **Repositorios por entidad, no genéricos:** un interfaz por entidad con los métodos justificados por los casos de uso documentados (Consultar/Crear, email único, Regla 4 `ExistsAsync`, Regla 7 `GetConflictingAsync`). Sin `IGenericRepository<T>`, `IUnitOfWork` ni `IRepository<T>`.
2. **Métodos de actualización aún no definidos:** se añadirán junto con los casos de uso que los necesiten (fase 6+), cuando exista comportamiento de dominio que los respalde.
3. **`ICurrentUser`** en `Abstractions/Services/`: solo `UserId` + `IsAuthenticated`; sin HTTP/JWT/ASP.NET. La implementación llegará en la API.
4. **Sin `Result`/errores ni paginación:** el manejo de errores esperado pertenece a la fase 12 y no hay requisito de paginación; no se crean abstractions especulativas. Sin MediatR, FluentValidation, AutoMapper, DTOs ni CQRS.
5. **`Features/` y `Common/` creados pero vacíos** a propósito: no se generan archivos placeholder. Se poblarán en las fases 6+.
6. **Colisión de nombres resuelta:** el tipo `Reservation` se importa con alias `ReservationEntity` dentro de `Reservation.Application.*` porque el nombre simple `Reservation` resuelve al namespace raíz.
7. **Dominio sin cambios** en esta fase.

### Verificación (resultados reales)

```text
dotnet build Reservation.slnx        → Build succeeded. 0 Warning(s), 0 Error(s)
dotnet list reference (Application)  → solo ../Reservation.Domain/Reservation.Domain.csproj
dotnet list package (Application)    → sin packages
grep EF/ASP.NET en Application       → sin coincidencias
```

---

## Fase 2 — Domain Model (completada)

### Archivos creados

```text
src/Reservation.Domain/
├── Enums/
│   ├── UserRole.cs            # Admin, Client
│   └── ReservationStatus.cs   # Pending, Confirmed, Cancelled, Completed
└── Entities/
    ├── User.cs
    ├── Service.cs
    ├── Professional.cs
    ├── ProfessionalService.cs
    ├── Availability.cs
    └── Reservation.cs
```

### Decisiones

1. **Identificadores:** `Guid` generado dentro del dominio (`Guid.NewGuid()`) en cada constructor de entidad. `ProfessionalService` no tiene `Id` (es la asociación N:M; sus dos claves son `ProfessionalId`/`ServiceId`).
2. **Sin `BaseEntity`:** cada entidad declara sus propiedades con *private setters*. Sin interfaces, value objects, eventos ni fábricas adicionales.
3. **Pertenencia a `Reservation.Domain.Entities` / `.Enums`:** las únicas carpetas del proyecto.
4. **Tipos:** dinero con `decimal` (nunca `float`/`double`); horarios con `TimeOnly` + `System.DayOfWeek`; auditoría con `DateTime.UtcNow` en `CreatedAt`/`UpdatedAt`. La zona horaria de `StartAt`/`EndAt` queda pendiente de decisión (dominio §14).
5. **`Reservation`:** el constructor recibe la entidad `Service` y calcula `EndAt = StartAt + DurationInMinutes` (Regla 5 del dominio). Estado inicial `Pending`. Sin detección de conflictos ni validación de disponibilidad (requerirán datos externos).
6. **`ProfessionalService`:** solo propiedades de clave con *getters*; la restricción de unicidad quedará en Infrastructure.
7. **Sin persistencia en Domain:** cero atributos EF (`[Table]`, `[Key]`…), cero constructores parameterless para EF, cero dependencias. El hashing de contraseñas no está aquí.
8. **Excepciones:** `ArgumentException`/`ArgumentOutOfRangeException` estándar; sin librerías de validación.
9. **Reglas locales implementadas:** nombre no vacío (User, Service, Professional); duración > 0 y precio ≥ 0 (Service); `EndTime > StartTime` (Availability); Guids válidos; `Enum.IsDefined` para `Role`.

### Verificación (resultados reales)

```text
dotnet build Reservation.slnx        → Build succeeded. 0 Warning(s), 0 Error(s)
dotnet list reference (Domain)       → no project references
dotnet list package (Domain)         → no packages
grep "using Microsoft|entityframework" src/Reservation.Domain → sin coincidencias
```

---

## Fase 1 — Project Foundation (completada)

### Estructura creada

```text
SistReservas/
├── Reservation.slnx              # Solución (.NET 10, formato slnx)
├── docker-compose.yml            # PostgreSQL 17 para desarrollo local
├── src/
│   ├── Reservation.Api/          # Entrada HTTP (Minimal API)
│   ├── Reservation.Application/  # Casos de uso (vacío por ahora)
│   ├── Reservation.Domain/       # Reglas de negocio (vacío por ahora)
│   └── Reservation.Infrastructure/ # EF Core + PostgreSQL
└── docs/
```

### Referencias entre proyectos (verificadas)

```text
Domain          → (ninguna)
Application     → Domain
Infrastructure  → Application, Domain
Api             → Application, Infrastructure
```

### Archivos clave

| Archivo | Propósito |
| ------- | --------- |
| `src/Reservation.Infrastructure/Data/ReservationDbContext.cs` | DbContext (sin entidades aún; se añaden incrementalmente) |
| `src/Reservation.Infrastructure/DependencyInjection.cs` | `AddInfrastructure(IServiceCollection, IConfiguration)` registra el DbContext con Npgsql |
| `src/Reservation.Api/Program.cs` | Compone la app: `builder.Services.AddInfrastructure(...)` |
| `src/Reservation.Api/appsettings.json` | Connection string `Postgres` con placeholder `Password=CHANGE_ME` |
| `docker-compose.yml` | PostgreSQL 17 (`reservation_user` / `reservation_db`), password desde variable de entorno `POSTGRES_PASSWORD` |

### Decisiones

1. **Solución `Reservation.slnx`:** el SDK .NET 10 genera por defecto el formato de solución nuevo (`.slnx`). Se aceptó el formato por defecto; los proyectos conservan el nombre `Reservation.*` definido en el README.
2. **Layout `src/`:** proyectos separados de `docs/`; los tests (fase 13) irán en `tests/` aparte. No se creó proyecto de tests en esta fase.
3. **EF Core 10.0.3 / Npgsql 10.0.3** (`Microsoft.EntityFrameworkCore` 10.0.4 transitivo): compatible con SDK 10.0.110.
4. **DbContext en Infrastructure** (`Reservation.Infrastructure.Data`), sin `DbSet`s todavía. Las configuraciones de entidades se añadirán en `Configurations/` cuando exista el modelo de dominio.
5. **Registro de DI** en `DependencyInjection.AddInfrastructure(...)` dentro de Infrastructure; `Program.cs` solo lo invoca.
6. **Sin credenciales reales en el repositorio:** el connection string usa `Password=CHANGE_ME`; el valor real se inyecta por variable de entorno (`ConnectionStrings__Postgres`). `docker-compose.yml` exige `POSTGRES_PASSWORD` (p. ej. en un `.env` no commiteado).
7. **Sin migraciones EF aún:** no hay entidades, generar una migración sería vacío. Corresponde cuando exista el modelo (fase 2+).
8. **Endpoint raíz eliminado:** se quitó el `MapGet("/")` de "Hello World" de la plantilla; no hay endpoints en esta fase.

### Verificación (resultados reales)

```text
dotnet --version                      → 10.0.110
dotnet restore Reservation.slnx       → OK (4 proyectos)
dotnet build Reservation.slnx         → Build succeeded. 0 Warning(s), 0 Error(s)
dotnet list reference                 → reglas de dependencias verificadas (arriba)
dotnet list package (Infrastructure)  → Npgsql.EFCore 10.0.3 / EF Core 10.0.4
POSTGRES_PASSWORD=... docker compose config -q → OK (sintaxis válida)
dotnet test                           → no aplica: aún no existen proyectos de tests
```

### Pendientes / incidencias conocidas

* ~~**`dotnet ef database update` NO ejecutado — bloqueo de entorno (fase 4).**~~ **RESUELTO en fase 14:** `.env` con `POSTGRES_PASSWORD` creado (ignorado por `.gitignore`), mapeo de host cambiado a `5435:5432` (los puertos 5432–5434 estaban ocupados por contenedores ajenos), migración aplicada y API verificada en runtime. Detalle en *Fase 14*.

* **Git:** el directorio aún no es un repositorio y no existe `.gitignore`. Definir cuándo inicializar git (no forma parte de la fase 1).
* **Docs (menores):** `docs/domain-model.md` lista `ReservationStatus` como entidad nº 7 aunque se define como enum; `Availability` no tiene `CreatedAt`/`UpdatedAt`. No se modificó el modelo de dominio (restringido en fases 1 y 2).
