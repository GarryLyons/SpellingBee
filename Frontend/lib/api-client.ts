import { AttemptOutcome, PracticeSessionResponse, WordEntry, PracticeSessionSummary } from "./types";
import { fetchAuthSession } from "aws-amplify/auth";

// Use the public environment variable to hit the backend directly
const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5064";

async function requestJson<T>(path: string, options?: RequestInit): Promise<T> {
  let token: string | undefined;
  try {
    const session = await fetchAuthSession();
    // Use accessToken; check if tokens exist first
    if (session.tokens?.accessToken) {
        token = session.tokens.accessToken.toString();
    } else {
        console.warn("No access token found in session");
    }
  } catch (err) {
    console.warn("Failed to fetch auth session:", err);
  }

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options?.headers as Record<string, string> ?? {})
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    cache: "no-store",
    ...options,
    headers
  });

  if (!response.ok) {
    const payload = await response.json().catch(() => null);
    const message =
      payload?.message ?? payload?.title ?? `Request failed with status ${response.status}: ${response.statusText}`;
    throw new Error(message);
  }

  return (await response.json()) as T;
}

export function getWords(): Promise<WordEntry[]> {
  return requestJson<WordEntry[]>("/api/words");
}

export function getPracticeSessions(): Promise<PracticeSessionSummary[]> {
  return requestJson<PracticeSessionSummary[]>("/api/practice-sessions");
}

export function createPracticeSession(wordIds?: string[]): Promise<PracticeSessionResponse> {
  const body = wordIds && wordIds.length > 0 ? { wordIds } : {};
  return requestJson<PracticeSessionResponse>("/api/practice-sessions", {
    method: "POST",
    body: JSON.stringify(body)
  });
}

export function getPracticeSession(sessionId: string): Promise<PracticeSessionResponse> {
  // Currently backend returns session creation response structure for get/resume?
  // If not, we might need a dedicated GET /api/practice-sessions/{id}.
  // For now, let's assume we can resume using 'reset' or just re-hydrate if we had a proper resume endpoint. 
  // Wait, looking at endpoints, there is no GET /:id details endpoint yet. 
  // User asked to 'select one, it takes you to that lesson'. 
  // We need a way to fetch state by ID.
  // The 'createPracticeSession' POST returns state.
  // The backend store has TryGetState.
  // We should add GET /api/practice-sessions/{id} in backend or just use RE-POST with same IDs? 
  // Re-POST creates NEW session. We need Resume.
  return requestJson<PracticeSessionResponse>(`/api/practice-sessions/${sessionId}`);
}

export function recordAttempt(sessionId: string, outcome: AttemptOutcome): Promise<PracticeSessionResponse> {
  return requestJson<PracticeSessionResponse>(`/api/practice-sessions/${sessionId}/attempts`, {
    method: "POST",
    body: JSON.stringify({ outcome })
  });
}

export function completeModel(sessionId: string): Promise<PracticeSessionResponse> {
  return requestJson<PracticeSessionResponse>(`/api/practice-sessions/${sessionId}/model-completions`, {
    method: "POST",
    body: "{}"
  });
}

export function resetPracticeSession(sessionId: string): Promise<PracticeSessionResponse> {
  return requestJson<PracticeSessionResponse>(`/api/practice-sessions/${sessionId}/reset`, {
    method: "POST",
    body: "{}"
  });
}
