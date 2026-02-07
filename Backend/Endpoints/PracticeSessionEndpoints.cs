using Backend.Contracts;
using Backend.Services;
using Backend.Utilities;

namespace Backend.Endpoints;

public static class PracticeSessionEndpoints
{
    public static RouteGroupBuilder MapPracticeSessionEndpoints(this RouteGroupBuilder apiGroup)
    {
        var sessionsGroup = apiGroup.MapGroup("/practice-sessions").WithTags("Practice Sessions");

        sessionsGroup.MapPost("/", (
            CreatePracticeSessionRequest request,
            WordBankRepository wordBankRepository,
            PracticeEngine practiceEngine,
            PracticeSessionStore sessionStore,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("PracticeSessionEndpoints");
            var errors = request.ValidateRequest();
            if (errors is not null)
            {
                return Results.ValidationProblem(errors);
            }

            var selectedIds = request.WordIds?
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? wordBankRepository.GetAll().Select(word => word.Id).ToList();

            if (selectedIds.Count == 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["wordIds"] = ["No valid word IDs were provided."]
                });
            }

            if (!wordBankRepository.ContainsAll(selectedIds, out var missingIds))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["wordIds"] = [$"Unknown word IDs: {string.Join(", ", missingIds)}"]
                });
            }

            var selectedWords = wordBankRepository.GetByIds(selectedIds);
            var state = practiceEngine.CreateInitialState(selectedWords);
            var session = sessionStore.Create(selectedIds, state);

            logger.LogInformation(
                "Created practice session {SessionId} with {WordCount} words.",
                session.Id,
                selectedIds.Count);

            var response = new PracticeSessionResponse
            {
                SessionId = session.Id,
                State = state
            };

            return Results.Created($"/api/practice-sessions/{session.Id}", response);
        });

        sessionsGroup.MapGet("/{sessionId:guid}", (Guid sessionId, PracticeSessionStore sessionStore) =>
        {
            if (!sessionStore.TryGetState(sessionId, out var state))
            {
                return Results.NotFound(new { message = $"Session '{sessionId}' was not found." });
            }

            return Results.Ok(new PracticeSessionResponse
            {
                SessionId = sessionId,
                State = state
            });
        });

        sessionsGroup.MapPost("/{sessionId:guid}/attempts", (
            Guid sessionId,
            RecordAttemptRequest request,
            PracticeSessionStore sessionStore,
            PracticeEngine practiceEngine,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("PracticeSessionEndpoints");
            var errors = request.ValidateRequest();
            if (errors is not null)
            {
                return Results.ValidationProblem(errors);
            }

            var outcome = request.Outcome.Trim().ToLowerInvariant();
            var updated = sessionStore.TryUpdateState(
                sessionId,
                state => outcome == "correct"
                    ? practiceEngine.RecordAttemptCorrect(state)
                    : practiceEngine.RecordAttemptIncorrect(state),
                out var nextState);

            if (!updated)
            {
                return Results.NotFound(new { message = $"Session '{sessionId}' was not found." });
            }

            logger.LogInformation(
                "Recorded attempt for session {SessionId}. Outcome: {Outcome}. Turn: {Turn}. Phase: {Phase}.",
                sessionId,
                outcome,
                nextState.Turn,
                nextState.Phase);

            return Results.Ok(new PracticeSessionResponse
            {
                SessionId = sessionId,
                State = nextState
            });
        });

        sessionsGroup.MapPost("/{sessionId:guid}/model-completions", (
            Guid sessionId,
            PracticeSessionStore sessionStore,
            PracticeEngine practiceEngine,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("PracticeSessionEndpoints");

            var updated = sessionStore.TryUpdateState(
                sessionId,
                practiceEngine.CompleteModelAndAdvance,
                out var nextState);

            if (!updated)
            {
                return Results.NotFound(new { message = $"Session '{sessionId}' was not found." });
            }

            logger.LogInformation("Completed model prompt for session {SessionId}.", sessionId);

            return Results.Ok(new PracticeSessionResponse
            {
                SessionId = sessionId,
                State = nextState
            });
        });

        sessionsGroup.MapPost("/{sessionId:guid}/reset", (
            Guid sessionId,
            PracticeSessionStore sessionStore,
            WordBankRepository wordBankRepository,
            PracticeEngine practiceEngine,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("PracticeSessionEndpoints");

            if (!sessionStore.TryGetWordIds(sessionId, out var wordIds))
            {
                return Results.NotFound(new { message = $"Session '{sessionId}' was not found." });
            }

            var words = wordBankRepository.GetByIds(wordIds);
            var resetState = practiceEngine.ResetPractice(words);
            var set = sessionStore.TrySetState(sessionId, resetState, out var nextState);
            if (!set)
            {
                return Results.NotFound(new { message = $"Session '{sessionId}' was not found." });
            }

            logger.LogInformation(
                "Reset session {SessionId}. Word count: {WordCount}.",
                sessionId,
                wordIds.Count);

            return Results.Ok(new PracticeSessionResponse
            {
                SessionId = sessionId,
                State = nextState
            });
        });

        return sessionsGroup;
    }
}
