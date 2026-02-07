import { AttemptOutcome, PracticeSessionResponse, WordEntry } from "./types";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5064";

async function requestJson<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    cache: "no-store",
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(options?.headers ?? {})
    }
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

export function createPracticeSession(wordIds?: string[]): Promise<PracticeSessionResponse> {
  const body = wordIds && wordIds.length > 0 ? { wordIds } : {};
  return requestJson<PracticeSessionResponse>("/api/practice-sessions", {
    method: "POST",
    body: JSON.stringify(body)
  });
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
