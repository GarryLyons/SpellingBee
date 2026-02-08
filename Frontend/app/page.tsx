"use client";

import { withAuthenticator } from "@aws-amplify/ui-react";
import "@aws-amplify/ui-react/styles.css";
import { useCallback, useEffect, useRef, useState } from "react";
import {
  completeModel,
  createPracticeSession,
  getPracticeSessions,
  getPracticeSession,
  getWords,
  recordAttempt,
  resetPracticeSession
} from "@/lib/api-client";
import { getCurrentSegmentationStep, getCurrentWord, getPriorityLabel } from "@/lib/practice-engine";
import { AttemptOutcome, PracticeState, PracticeSessionSummary } from "@/lib/types";
import { SessionList } from "./components/SessionList";

function getPhaseTitle(phase: PracticeState["phase"]): string {
  return phase === "attempt" ? "Reading attempt" : "Adult modeling";
}

function getPhaseNote(phase: PracticeState["phase"]): string {
  if (phase === "attempt") {
    return "Ask the student to pronounce this word. Mark correct or incorrect.";
  }
  return "Model the correct pronunciation aloud, then continue practice on this word.";
}

function formatDueIn(turns: number): string {
  if (turns <= 0) {
    return "Now";
  }
  return `${turns} turn${turns === 1 ? "" : "s"}`;
}

function Home() {
  const [view, setView] = useState<"list" | "session">("list");
  const [sessions, setSessions] = useState<PracticeSessionSummary[]>([]);
  
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [state, setState] = useState<PracticeState | null>(null);
  
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const initializedRef = useRef(false);

  // Load list of sessions on mount
  useEffect(() => {
    if (initializedRef.current) return;
    initializedRef.current = true;
    
    // Initial fetch
    refreshList();
  }, []);

  const refreshList = useCallback(async () => {
    setIsLoading(true);
    try {
      const list = await getPracticeSessions();
      setSessions(list);
      setView("list");
    } catch (err) {
      console.warn("Failed to load sessions", err);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const createSession = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const words = await getWords();
      const response = await createPracticeSession(words.map((word) => word.id));
      setSessionId(response.sessionId);
      setState(response.state);
      setView("session");
    } catch (err) {
      const message = err instanceof Error ? err.message : "Unable to start a practice session.";
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const selectSession = useCallback(async (id: string) => {
      setIsLoading(true);
      setError(null);
      try {
          const response = await getPracticeSession(id);
          setSessionId(response.sessionId);
          setState(response.state);
          setView("session");
      } catch (err) {
          setError("Failed to load session details.");
      } finally {
          setIsLoading(false);
      }
  }, []);

  // Removed old auto-start effect
  // useEffect(() => { ... }, [initializeSession]);

  const runAction = useCallback(
    async (action: (activeSessionId: string) => Promise<{ state: PracticeState }>) => {
      if (!sessionId) {
        return;
      }

      setIsSubmitting(true);
      setError(null);

      try {
        const response = await action(sessionId);
        setState(response.state);
      } catch (err) {
        const message = err instanceof Error ? err.message : "Request failed.";
        setError(message);
      } finally {
        setIsSubmitting(false);
      }
    },
    [sessionId]
  );

  const handleAttempt = useCallback(
    async (outcome: AttemptOutcome) => {
      await runAction((activeSessionId) => recordAttempt(activeSessionId, outcome));
    },
    [runAction]
  );

  const handleModelComplete = useCallback(async () => {
    await runAction((activeSessionId) => completeModel(activeSessionId));
  }, [runAction]);

  const handleReset = useCallback(async () => {
    // Instead of resetting state, go back to list
    setView("list");
    setSessionId(null);
    setState(null);
    void refreshList();
  }, [refreshList]);

  if (view === "list") {
      return (
        <main className="page-shell">
            <header className="hero">
                <h1>Reading Practice Dashboard</h1>
                <p>Select a session to resume or start a new practice loop.</p>
            </header>
            {isLoading ? (
                 <section className="practice-panel"><p>Loading sessions...</p></section>
            ) : (
                <SessionList 
                    sessions={sessions} 
                    onSelectSession={selectSession} 
                    onCreateNew={createSession} 
                />
            )}
        </main>
      );
  }

  if (isLoading || !state) {
    return (
      <main className="page-shell">
        <header className="hero">
          <h1>Reading Practice Loop</h1>
          <p>Loading practice session from backend...</p>
        </header>
        {error && (
          <section className="practice-panel">
            <p className="phase-note">{error}</p>
            <div className="action-row">
              <button type="button" className="btn btn-info" onClick={() => void handleReset()}>
                Back to Dashboard
              </button>
            </div>
          </section>
        )}
      </main>
    );
  }

  const currentWord = getCurrentWord(state);
  const currentStep = getCurrentSegmentationStep(state);
  const levelLabel = `Level ${currentStep.levelNumber}: ${currentStep.title}`;

  const entries = Object.values(state.progress);
  const summary = {
    attempts: entries.reduce((total, item) => total + item.attempts, 0),
    firstTryCorrect: entries.reduce((total, item) => total + item.firstTryCorrect, 0),
    supportedCorrect: entries.reduce((total, item) => total + item.correctedAfterSupport, 0),
    highPriorityIncorrect: entries.reduce((total, item) => total + item.highPriorityIncorrect, 0),
    firstAttemptIncorrect: entries.reduce((total, item) => total + item.firstAttemptIncorrect, 0),
    accuracy: 0
  };
  summary.accuracy = summary.attempts > 0 ? Math.round((summary.firstTryCorrect / summary.attempts) * 100) : 0;

  const ranked = state.wordBank
    .map((word) => {
      const item = state.progress[word.id];
      return {
        word,
        item,
        priority: getPriorityLabel(item)
      };
    })
    .sort((left, right) => {
      const difficultyDiff = right.item.difficulty - left.item.difficulty;
      if (difficultyDiff !== 0) {
        return difficultyDiff;
      }

      const dueDiff = left.item.dueIn - right.item.dueIn;
      if (dueDiff !== 0) {
        return dueDiff;
      }

      return left.word.word.localeCompare(right.word.word);
    });

  return (
    <main className="page-shell">
      <header className="hero">
        <h1>Reading Practice Loop</h1>
        <p>
          Present one word at a time. Start with whole-word reading, then reveal progressively deeper segmentation
          levels only when the student misses.
        </p>
      </header>

      {error && (
        <section className="practice-panel">
          <p className="phase-note">{error}</p>
        </section>
      )}

      <div className="panel-grid">
        <section className="practice-panel">
          <div className="status-strip">
            <span className="status-pill">Cycle {state.cycle}</span>
            <span className="status-pill">Presented words: {state.turn}</span>
            <span className="status-pill">Support level: {levelLabel}</span>
            <span className="status-pill">Current priority: {getPriorityLabel(state.progress[currentWord.id])}</span>
          </div>

          <article className="word-card">
            <p className="phase-title">{getPhaseTitle(state.phase)}</p>
            {state.phase === "attempt" && <p className="phase-note">{levelLabel}</p>}
            <h2 className="word-display">{state.phase === "attempt" ? currentStep.value : currentWord.word}</h2>
            <p className="phase-note">{getPhaseNote(state.phase)}</p>
          </article>

          {state.phase === "attempt" && (
            <div className="action-row">
              <button
                type="button"
                className="btn btn-good"
                disabled={isSubmitting}
                onClick={() => void handleAttempt("correct")}
              >
                Correct
              </button>
              <button
                type="button"
                className="btn btn-warn"
                disabled={isSubmitting}
                onClick={() => void handleAttempt("incorrect")}
              >
                Incorrect
              </button>
            </div>
          )}

          {state.phase === "model_prompt" && (
            <div className="model-box">
              <p>This is the most segmented level. Model the correct pronunciation aloud, then resume this word.</p>
              <div className="action-row">
                <button
                  type="button"
                  className="btn btn-info"
                  disabled={isSubmitting}
                  onClick={() => void handleModelComplete()}
                >
                  Continue practice
                </button>
              </div>
            </div>
          )}
        </section>

        <aside className="progress-panel">
          <h3 className="panel-title">Session summary</h3>
          <div className="summary-grid">
            <div className="metric">
              <p className="metric-label">Presented words</p>
              <p className="metric-value">{summary.attempts}</p>
            </div>
            <div className="metric">
              <p className="metric-label">First try correct</p>
              <p className="metric-value">{summary.firstTryCorrect}</p>
            </div>
            <div className="metric">
              <p className="metric-label">Correct after support</p>
              <p className="metric-value">{summary.supportedCorrect}</p>
            </div>
            <div className="metric">
              <p className="metric-label">High priority misses</p>
              <p className="metric-value">{summary.highPriorityIncorrect}</p>
            </div>
            <div className="metric">
              <p className="metric-label">First-attempt misses</p>
              <p className="metric-value">{summary.firstAttemptIncorrect}</p>
            </div>
            <div className="metric">
              <p className="metric-label">First-try accuracy</p>
              <p className="metric-value">{summary.accuracy}%</p>
            </div>
          </div>

          <div className="priority-table-wrap">
            <table className="priority-table">
              <thead>
                <tr>
                  <th>Word</th>
                  <th>Priority</th>
                  <th>Next due</th>
                  <th>C / I / H</th>
                </tr>
              </thead>
              <tbody>
                {ranked.map(({ word, item, priority }) => (
                  <tr key={word.id}>
                    <td>{word.word}</td>
                    <td>
                      <span className={`tag tag-${priority.toLowerCase()}`}>{priority}</span>
                    </td>
                    <td>{formatDueIn(item.dueIn)}</td>
                    <td>
                      {item.firstTryCorrect} / {item.firstAttemptIncorrect} / {item.highPriorityIncorrect}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="reset-row">
            <button type="button" className="btn btn-reset" disabled={isSubmitting} onClick={() => void handleReset()}>
              Back to Dashboard
            </button>
          </div>
        </aside>
      </div>
    </main>
  );
}

export default withAuthenticator(Home);

