# Copilot Instructions for SpellingBee Project

## Project Context
This is a monorepo containing a full-stack application for a spelling practice engine.
- **Backend**: .NET 8 Web API (Minimal API)
- **Frontend**: Next.js 14 (App Router) with TypeScript and Tailwind CSS

## Architecture & Code Organization

### Monorepo Structure
- Root `package.json` contains orchestration scripts (`start:backend`, `start:frontend`).
- `Backend/`: .NET solution.
- `Frontend/`: Next.js application.

### Backend (.NET 8)
- **Pattern**: Minimal API with functional organization.
- **Key Directories**:
  - `Endpoints/`: Contains static classes defining route groups (e.g., `PracticeSessionEndpoints.cs`). Use `MapGroup` and extension methods to register routes.
  - `Services/`: Core business logic (e.g., `PracticeEngine.cs`). Most services are currently registered as Singleton (in-memory state).
  - `Contracts/`: DTOs for API requests/responses.
  - `Models/`: Internal domain models.
- **Data Flow**: `Endpoint` -> `Service` -> `Store` (In-Memory).
- **Validation**: Manual validation logic in endpoints (e.g., `request.ValidateRequest()`) returning `Results.ValidationProblem`.

### Frontend (Next.js 14)
- **Pattern**: App Router with Client Components ("use client") for interactive pages.
- **API Interaction**: Centralized in `lib/api-client.ts`. All data fetching checks `NEXT_PUBLIC_API_BASE_URL`.
- **State**: React local state (`useState`) manages session data returned from the backend.
- **Types**: Shared types are manually synced in `lib/types.ts` matching Backend `Contracts/Models`.

## Key Conventions & Patterns

### Backend Development
- **Start-up**: Modify `Program.cs` to register new services or endpoint groups.
- **Logging**: Inject `ILoggerFactory` into endpoint delegates to create named loggers.
- **CORS**: "Frontend" policy is configured to allow all origins/methods/headers.
- **Nullability**: Nullable reference types are enabled (`<Nullable>enable</Nullable>`). Handle nulls explicitly.

### Frontend Development
- **API Calls**: **Always** use helper functions in `lib/api-client.ts` rather than raw `fetch`.
- **Components**: Prefer small, functional components. Use "use client" only when hooks are needed.
- **Environment**: Respect `process.env.NEXT_PUBLIC_API_BASE_URL`.

## Critical Developer Workflows

### Running the Project
- **Backend**: `dotnet run` inside `Backend/` or `npm run start:backend` from root.
  - default URL: `http://localhost:5064`
- **Frontend**: `npm run dev` inside `Frontend/` or `npm run start:frontend` from root.
  - default URL: `http://localhost:3000`

### adding New Features
1. Define C# DTOs in `Backend/Contracts`.
2. Implement logic in `Backend/Services`.
3. Expose via new/updated endpoint in `Backend/Endpoints`.
4. Update `Frontend/lib/types.ts` to match C# DTOs.
5. Add client method in `Frontend/lib/api-client.ts`.
6. Implement UI in `Frontend/app`.

## Useful References
- **Startup Logic**: [Backend/Program.cs](Backend/Program.cs)
- **Business Logic**: [Backend/Services/PracticeEngine.cs](Backend/Services/PracticeEngine.cs)
- **API Client**: [Frontend/lib/api-client.ts](Frontend/lib/api-client.ts)
