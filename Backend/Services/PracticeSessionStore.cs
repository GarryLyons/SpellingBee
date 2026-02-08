using System.Collections.Concurrent;
using Backend.Models;

namespace Backend.Services;

public sealed class PracticeSessionStore
{
    private readonly ConcurrentDictionary<Guid, PracticeSession> _sessions = new();

    public PracticeSession Create(IReadOnlyList<string> wordIds, PracticeState state)
    {
        var session = new PracticeSession
        {
            Id = Guid.NewGuid(),
            WordIds = wordIds.ToList(),
            State = state
        };

        _sessions[session.Id] = session;
        return session;
    }

    public IEnumerable<PracticeSession> GetAll()
    {
        return _sessions.Values.OrderByDescending(s => s.CreatedAt);
    }

    public bool TryGetState(Guid sessionId, out PracticeState state)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            state = session.State;
            return true;
        }

        state = null!;
        return false;
    }

    public bool TryGetWordIds(Guid sessionId, out List<string> wordIds)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            wordIds = session.WordIds.ToList();
            return true;
        }

        wordIds = null!;
        return false;
    }

    public bool TryUpdateState(Guid sessionId, Func<PracticeState, PracticeState> update, out PracticeState nextState)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            nextState = null!;
            return false;
        }

        lock (session.SyncRoot)
        {
            session.State = update(session.State);
            nextState = session.State;
            return true;
        }
    }

    public bool TrySetState(Guid sessionId, PracticeState state, out PracticeState nextState)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            nextState = null!;
            return false;
        }

        lock (session.SyncRoot)
        {
            session.State = state;
            nextState = session.State;
            return true;
        }
    }
}
