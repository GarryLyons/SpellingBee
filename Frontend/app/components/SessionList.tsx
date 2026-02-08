"use client";

import { useMemo, useState } from "react";
import { PracticeSessionSummary } from "@/lib/types";

interface SessionListProps {
  sessions: PracticeSessionSummary[];
  onSelectSession: (sessionId: string) => void;
  onCreateNew: () => void;
}

export function SessionList({ sessions, onSelectSession, onCreateNew }: SessionListProps) {
  const [filterText, setFilterText] = useState("");

  const filtered = useMemo(() => {
    const term = filterText.toLowerCase();
    return sessions.filter((s) => {
      const dateStr = s.createdAt ? new Date(s.createdAt).toLocaleString().toLowerCase() : "";
      const matchesSearch = s.id.toLowerCase().includes(term) || dateStr.includes(term);
      return matchesSearch;
    });
  }, [sessions, filterText]);

  return (
    <div className="panel-grid">
      <section className="practice-panel" style={{ gridColumn: "1 / -1" }}>
        <h2 className="panel-title">Your Practice Sessions</h2>
        
        <div className="action-row" style={{ justifyContent: "space-between", marginBottom: "1rem" }}>
          <input
            type="text"
            placeholder="Filter sessions..."
            className="input-field"
            value={filterText}
            onChange={(e) => setFilterText(e.target.value)}
            style={{ 
              padding: "0.5rem", 
              borderRadius: "0.5rem", 
              border: "2px solid #e1e4e8", 
              fontFamily: "inherit",
              fontSize: "1rem",
              width: "100%",
              maxWidth: "300px"
            }}
          />
          <button type="button" className="btn btn-good" onClick={onCreateNew}>
            New Session
          </button>
        </div>

        <div className="priority-table-wrap">
          <table className="priority-table">
            <thead>
              <tr>
                <th>Date Started</th>
                <th>Cycle</th>
                <th>Progress (Done/Total)</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filtered.length === 0 ? (
                <tr>
                  <td colSpan={4} style={{ textAlign: "center", padding: "2rem" }}>
                    No sessions found. Start a new one!
                  </td>
                </tr>
              ) : (
                filtered.map((session) => (
                  <tr key={session.id}>
                    <td>{new Date(session.createdAt).toLocaleString()}</td>
                    <td>{session.cycle}</td>
                    <td>
                      {session.completedWords} / {session.wordCount}
                    </td>
                    <td>
                      <button 
                        className="btn btn-info" 
                        style={{ padding: "0.25rem 0.75rem", fontSize: "0.9rem" }}
                        onClick={() => onSelectSession(session.id)}
                      >
                        Resume
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}
