# Spelling Bee

Project is now split into:

- `Frontend` - Next.js app UI
- `Backend` - .NET 8 Minimal API with practice engine + hardcoded word bank

## Backend

```bash
cd Backend
dotnet run
```

Default local URL from `launchSettings.json`: `http://localhost:5064`.

### API endpoints

- `GET /api/words`
- `POST /api/practice-sessions`
- `GET /api/practice-sessions/{sessionId}`
- `POST /api/practice-sessions/{sessionId}/attempts`
- `POST /api/practice-sessions/{sessionId}/model-completions`
- `POST /api/practice-sessions/{sessionId}/reset`

## Frontend

```bash
cd Frontend
npm install
npm run dev
```

Optional environment variable:

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:5064
```
