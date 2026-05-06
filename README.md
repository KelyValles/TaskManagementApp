# Task Management System

Sistema interno de gestión de tareas: registrar usuarios, asignar y dar seguimiento a tareas con estados (`Pending`, `InProgress`, `Done`).

Stack: **.NET 9 Web API** + **SQL Server** (ADO.NET puro) + **Angular 18** con **Angular Material**.

```
TaskManagementSystem/
├── backend/TaskManager.Api/        Web API .NET 9 (Controllers / Services / Repositories)
├── database/script.sql             Esquema SQL Server + consultas JSON nativas
├── frontend/task-manager-ui/       SPA Angular 18 standalone
└── README.md
```

---

## 1. Requisitos previos

| Componente | Versión recomendada |
|-----------|--------------------|
| .NET SDK | **9.0+** |
| SQL Server | **2016+** |
| Node.js | **18.19+** o **20.11+** (Angular 18 no soporta Node 16) |
| npm | 9+ |

> Si tu Node es anterior a 18.19, instala una versión LTS desde https://nodejs.org antes de ejecutar el frontend.

---

## 2. Pasos para ejecutar el proyecto

### 2.1 Base de datos

1. Abrir SQL Server Management Studio (o Azure Data Studio).
2. Conectarse a la instancia local.
3. Ejecutar el script:

   ```
   database/script.sql
   ```

   El script es **idempotente**: crea `TaskDB`, las tablas `Users` y `Tasks`, FK, índices, restricciones (`CHECK ISJSON`, `CHECK Status IN (...)`), datos sembrados y, al final, ejemplos de consultas con funciones JSON nativas.

### 2.2 Backend (.NET Web API)

1. Editar la cadena de conexión en (backend/TaskManager.Api/appsettings.json) si tu instancia SQL Server no es la local por defecto.
2. Restaurar y ejecutar:

   ```bash
   cd backend/TaskManager.Api
   dotnet restore
   dotnet run
   ```

3. La API queda disponible en `https://localhost:5001` (o el puerto que muestre la consola). Swagger UI en `/swagger`.

### 2.3 Frontend (Angular)

```bash
cd frontend/task-manager-ui
npm install
npm start
```

La SPA abre en `http://localhost:4200` y proxea las llamadas `/api/*` al backend mediante [proxy.conf.json](frontend/task-manager-ui/proxy.conf.json).

---

## 3. Endpoints de la API

| Verbo | Ruta | Descripción |
|-------|------|-------------|
| `POST` | `/api/users` | Crea usuario (`name`, `email`). |
| `GET`  | `/api/users` | Lista usuarios. |
| `POST` | `/api/tasks` | Crea tarea (`title`, `userId`, `additionalInfo` opcional como JSON). |
| `GET`  | `/api/tasks` | Lista tareas. Query params opcionales: `userId`, `status`, `priority`. |
| `PUT`  | `/api/tasks/{id}/status` | Cambia el estado. Body: `{ "newStatus": "InProgress" }`. |

### Códigos de respuesta del manejo de errores

- **400** Validación fallida (`ModelState`) o JSON inválido en `additionalInfo`.
- **404** Recurso no encontrado (usuario o tarea).
- **409** Conflicto (correo duplicado).
- **422** Violación de regla de negocio (transición `Pending → Done` directa).
- **500** Error inesperado.

Todas las respuestas de error siguen el formato `application/problem+json` (`ProblemDetails`).

---

## 4. Decisiones técnicas

| Tema | Decisión | Motivo |
|------|----------|--------|
| Acceso a datos | **ADO.NET puro** (`SqlConnection`, `SqlCommand`, `SqlDataReader`) | El enunciado pone foco en SQL Server y funciones JSON nativas. El SQL queda explícito y demuestra el uso real de `JSON_VALUE` desde el backend. |
| Validación | **Data Annotations + `ModelState`** | Cero dependencias adicionales, suficiente para los requisitos del enunciado. |
| Manejo de errores | **Middleware central** que mapea excepciones de dominio → `ProblemDetails` con códigos HTTP estables. | Mantiene los controladores limpios y unifica el contrato de errores para el frontend. |
| Estilo de API | **Controllers MVC clásicos** | Mejor separación visual de capas; alineado con la rúbrica de evaluación. |
| Frontend | **Angular 18 standalone + Material + signals** | Componentes accesibles listos (MatTable, MatSelect) y menos boilerplate sin NgModules. |
| Arquitectura backend | Controller → Service → Repository, con DTOs separados de las entidades | Aísla reglas de negocio de la persistencia y el contrato HTTP. |
| Connection factory | `ISqlConnectionFactory` registrada como **singleton** | Crea conexiones nuevas por petición; el pool de SQL Client gestiona el reciclado. |

---

## 5. Funcionalidades pendientes

- **Autenticación / autorización** — no está pedida en el enunciado, así que la API es pública en local.
- **Tests automatizados** — no se incluyen tests unitarios ni de integración (mejora futura: xUnit + Testcontainers).
- **Edición / borrado de tareas y usuarios** — no exigido; sólo se implementan los endpoints del enunciado.
- **Paginación** en `GET /api/tasks` — adecuada cuando el volumen crezca.
- **CI/CD** — pipeline de build y publicación.


