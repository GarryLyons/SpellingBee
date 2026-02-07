using Backend.Models;

namespace Backend.Services;

public sealed class PracticeEngine
{
    private const int DifficultyMin = 1;
    private const int DifficultyMax = 10;
    private static readonly int[] FibonacciGaps = [5, 8, 13, 21, 34];

    private static readonly Dictionary<string, (int LevelNumber, string Title)> SegmentationMeta = new(StringComparer.Ordinal)
    {
        ["whole_word"] = (1, "Whole Word"),
        ["morphological_units"] = (2, "Morphological Units"),
        ["syllables"] = (3, "Syllables"),
        ["onset_rime"] = (4, "Onset-Rime"),
        ["grapheme_groups"] = (5, "Grapheme Groups"),
        ["individual_phonemes"] = (6, "Individual Phonemes")
    };

    public PracticeState CreateInitialState(IReadOnlyList<WordEntry> wordBank)
    {
        if (wordBank.Count == 0)
        {
            throw new InvalidOperationException("Word bank cannot be empty.");
        }

        var progress = wordBank.ToDictionary(word => word.Id, word => InitProgress(word.Id), StringComparer.Ordinal);
        var currentWordId = PickNextWordId(wordBank, progress, string.Empty);

        return new PracticeState
        {
            WordBank = wordBank.ToList(),
            Progress = progress,
            CurrentWordId = currentWordId,
            CurrentSegmentationIndex = 0,
            Level1MissedOnCurrentWord = false,
            Phase = "attempt",
            Turn = 0,
            Cycle = 1,
            SeenInCycle = []
        };
    }

    public PracticeState RecordAttemptCorrect(PracticeState state)
    {
        if (!string.Equals(state.Phase, "attempt", StringComparison.Ordinal))
        {
            return state;
        }

        if (state.CurrentSegmentationIndex > 0)
        {
            return new PracticeState
            {
                WordBank = state.WordBank,
                Progress = state.Progress,
                CurrentWordId = state.CurrentWordId,
                CurrentSegmentationIndex = state.CurrentSegmentationIndex - 1,
                Level1MissedOnCurrentWord = state.Level1MissedOnCurrentWord,
                Phase = state.Phase,
                Turn = state.Turn,
                Cycle = state.Cycle,
                SeenInCycle = state.SeenInCycle
            };
        }

        return CompleteEncounterAndAdvance(state);
    }

    public PracticeState RecordAttemptIncorrect(PracticeState state)
    {
        if (!string.Equals(state.Phase, "attempt", StringComparison.Ordinal))
        {
            return state;
        }

        var currentWord = GetCurrentWord(state);
        var plan = GetSegmentationPlan(currentWord);
        var hasNextLevel = state.CurrentSegmentationIndex < plan.Count - 1;
        var isFirstLevel = state.CurrentSegmentationIndex == 0;
        var level1MissedOnCurrentWord = state.Level1MissedOnCurrentWord || isFirstLevel;
        var current = state.Progress[state.CurrentWordId];

        var nextCurrent = new WordProgress
        {
            WordId = current.WordId,
            Attempts = current.Attempts,
            FirstTryCorrect = current.FirstTryCorrect,
            CorrectedAfterSupport = current.CorrectedAfterSupport,
            FirstAttemptIncorrect = isFirstLevel && !state.Level1MissedOnCurrentWord
                ? current.FirstAttemptIncorrect + 1
                : current.FirstAttemptIncorrect,
            HighPriorityIncorrect = current.HighPriorityIncorrect,
            Status = current.Status,
            NextFibonacciIndex = current.NextFibonacciIndex,
            WasEverWrong = current.WasEverWrong,
            Streak = 0,
            Difficulty = current.Difficulty,
            DueIn = current.DueIn,
            LastSeenTurn = current.LastSeenTurn
        };

        if (hasNextLevel)
        {
            nextCurrent = nextCurrent with { Difficulty = ClampDifficulty(nextCurrent.Difficulty + 1) };
            var progressed = CloneProgress(state.Progress);
            progressed[state.CurrentWordId] = nextCurrent;

            return new PracticeState
            {
                WordBank = state.WordBank,
                Progress = progressed,
                CurrentWordId = state.CurrentWordId,
                CurrentSegmentationIndex = state.CurrentSegmentationIndex + 1,
                Level1MissedOnCurrentWord = level1MissedOnCurrentWord,
                Phase = state.Phase,
                Turn = state.Turn,
                Cycle = state.Cycle,
                SeenInCycle = state.SeenInCycle
            };
        }

        nextCurrent = nextCurrent with
        {
            HighPriorityIncorrect = nextCurrent.HighPriorityIncorrect + 1,
            Difficulty = ClampDifficulty(nextCurrent.Difficulty + 2)
        };

        var finalProgress = CloneProgress(state.Progress);
        finalProgress[state.CurrentWordId] = nextCurrent;

        return new PracticeState
        {
            WordBank = state.WordBank,
            Progress = finalProgress,
            CurrentWordId = state.CurrentWordId,
            CurrentSegmentationIndex = state.CurrentSegmentationIndex,
            Level1MissedOnCurrentWord = level1MissedOnCurrentWord,
            Phase = "model_prompt",
            Turn = state.Turn,
            Cycle = state.Cycle,
            SeenInCycle = state.SeenInCycle
        };
    }

    public PracticeState CompleteModelAndAdvance(PracticeState state)
    {
        if (!string.Equals(state.Phase, "model_prompt", StringComparison.Ordinal))
        {
            return state;
        }

        return state with { Phase = "attempt" };
    }

    public PracticeState ResetPractice(IReadOnlyList<WordEntry> wordBank)
    {
        return CreateInitialState(wordBank);
    }

    private PracticeState CompleteEncounterAndAdvance(PracticeState state)
    {
        var currentId = state.CurrentWordId;
        var nextProgress = new Dictionary<string, WordProgress>(StringComparer.Ordinal);
        var maxNaturalGap = GetMaxNaturalGap(state.WordBank.Count);

        foreach (var (id, value) in state.Progress)
        {
            if (string.Equals(id, currentId, StringComparison.Ordinal))
            {
                continue;
            }

            nextProgress[id] = value with { DueIn = Math.Max(0, value.DueIn - 1) };
        }

        var current = state.Progress[currentId];
        var withAttempt = current with
        {
            Attempts = current.Attempts + 1,
            LastSeenTurn = state.Turn
        };
        nextProgress[currentId] = ApplyEncounterOutcome(withAttempt, state.Level1MissedOnCurrentWord, maxNaturalGap);

        var seenSet = new HashSet<string>(state.SeenInCycle, StringComparer.Ordinal) { currentId };
        var cycle = state.Cycle;
        var seenInCycle = seenSet.ToList();
        if (seenInCycle.Count == state.WordBank.Count)
        {
            cycle += 1;
            seenInCycle = [];
        }

        var nextWordId = PickNextWordId(state.WordBank, nextProgress, currentId);

        return new PracticeState
        {
            WordBank = state.WordBank,
            Progress = nextProgress,
            CurrentWordId = nextWordId,
            CurrentSegmentationIndex = 0,
            Level1MissedOnCurrentWord = false,
            Phase = "attempt",
            Turn = state.Turn + 1,
            Cycle = cycle,
            SeenInCycle = seenInCycle
        };
    }

    private WordProgress ApplyEncounterOutcome(WordProgress item, bool wasWrong, int maxNaturalGap)
    {
        if (wasWrong)
        {
            return item with
            {
                CorrectedAfterSupport = item.CorrectedAfterSupport + 1,
                Streak = 0,
                Difficulty = ClampDifficulty(item.Difficulty + 2),
                DueIn = Math.Min(1, maxNaturalGap),
                Status = "retry",
                NextFibonacciIndex = 0,
                WasEverWrong = true
            };
        }

        if (item.WasEverWrong)
        {
            var gapIndex = Math.Min(item.NextFibonacciIndex, FibonacciGaps.Length - 1);
            var fibonacciGap = FibonacciGaps[gapIndex];
            return item with
            {
                FirstTryCorrect = item.FirstTryCorrect + 1,
                Streak = item.Streak + 1,
                Difficulty = ClampDifficulty(item.Difficulty - 1),
                DueIn = Math.Min(fibonacciGap, maxNaturalGap),
                Status = "fibonacci",
                NextFibonacciIndex = Math.Min(gapIndex + 1, FibonacciGaps.Length - 1),
                WasEverWrong = true
            };
        }

        return item with
        {
            FirstTryCorrect = item.FirstTryCorrect + 1,
            Streak = item.Streak + 1,
            Difficulty = ClampDifficulty(item.Difficulty - 1),
            DueIn = 0,
            Status = "normal",
            NextFibonacciIndex = 0,
            WasEverWrong = false
        };
    }

    private static string PickNextWordId(
        IReadOnlyList<WordEntry> wordBank,
        Dictionary<string, WordProgress> progress,
        string previousWordId)
    {
        if (wordBank.Count == 1)
        {
            return wordBank[0].Id;
        }

        var dueWords = wordBank.Where(word => progress[word.Id].DueIn == 0).ToList();
        if (dueWords.Count > 0)
        {
            var reviewDueWords = dueWords
                .Where(word =>
                {
                    var status = progress[word.Id].Status;
                    return string.Equals(status, "retry", StringComparison.Ordinal)
                        || string.Equals(status, "fibonacci", StringComparison.Ordinal);
                })
                .ToList();

            var duePool = reviewDueWords.Count > 0 ? reviewDueWords : dueWords;
            return PickLeastRecentlyShown(duePool, progress, previousWordId).Id;
        }

        var unseenWords = wordBank.Where(word => progress[word.Id].Attempts == 0).ToList();
        if (unseenWords.Count > 0)
        {
            return PickLeastRecentlyShown(unseenWords, progress, previousWordId).Id;
        }

        return PickLeastRecentlyShown(wordBank, progress, previousWordId).Id;
    }

    private static WordEntry PickLeastRecentlyShown(
        IReadOnlyList<WordEntry> candidates,
        Dictionary<string, WordProgress> progress,
        string previousWordId)
    {
        var withoutImmediateRepeat = candidates
            .Where(word => !string.Equals(word.Id, previousWordId, StringComparison.Ordinal))
            .ToList();

        var pool = withoutImmediateRepeat.Count > 0 ? withoutImmediateRepeat : candidates;
        return pool
            .OrderBy(word => progress[word.Id].LastSeenTurn)
            .ThenBy(word => word.Word, StringComparer.Ordinal)
            .First();
    }

    private static WordEntry GetCurrentWord(PracticeState state)
    {
        return state.WordBank.FirstOrDefault(word => string.Equals(word.Id, state.CurrentWordId, StringComparison.Ordinal))
               ?? state.WordBank[0];
    }

    private static List<SegmentationStep> GetSegmentationPlan(WordEntry word)
    {
        var segmentation = word.Segmentation;
        var plan = new List<(string Level, string? Value)>
        {
            ("whole_word", word.Word),
            ("morphological_units", segmentation?.MorphologicalUnits),
            ("syllables", segmentation?.Syllables),
            ("onset_rime", segmentation?.OnsetRime),
            ("grapheme_groups", segmentation?.GraphemeGroups ?? BuildFallbackGraphemeGroups(word)),
            ("individual_phonemes", segmentation?.IndividualPhonemes ?? BuildFallbackIndividualPhonemes(word))
        };

        var steps = new List<SegmentationStep>();
        foreach (var item in plan)
        {
            if (string.IsNullOrWhiteSpace(item.Value))
            {
                continue;
            }

            var meta = SegmentationMeta[item.Level];
            steps.Add(new SegmentationStep
            {
                Level = item.Level,
                LevelNumber = meta.LevelNumber,
                Title = meta.Title,
                Value = item.Value
            });
        }

        return steps;
    }

    private static string? BuildFallbackGraphemeGroups(WordEntry word)
    {
        if (word.Phonics.Graphemes.Count <= 1)
        {
            return null;
        }

        var grouped = string.Join(" | ", word.Phonics.Graphemes);
        return string.IsNullOrWhiteSpace(grouped) ? null : grouped;
    }

    private static string? BuildFallbackIndividualPhonemes(WordEntry word)
    {
        if (word.Word.Length <= 1)
        {
            return null;
        }

        return string.Join(" - ", word.Word.Select(ch => ch.ToString()));
    }

    private static Dictionary<string, WordProgress> CloneProgress(Dictionary<string, WordProgress> progress)
    {
        return progress.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
    }

    private static int ClampDifficulty(int value) => Math.Min(DifficultyMax, Math.Max(DifficultyMin, value));

    private static int GetMaxNaturalGap(int wordCount) => Math.Max(0, wordCount - 1);

    private static WordProgress InitProgress(string wordId)
    {
        return new WordProgress
        {
            WordId = wordId,
            Attempts = 0,
            FirstTryCorrect = 0,
            CorrectedAfterSupport = 0,
            FirstAttemptIncorrect = 0,
            HighPriorityIncorrect = 0,
            Status = "normal",
            NextFibonacciIndex = 0,
            WasEverWrong = false,
            Streak = 0,
            Difficulty = 3,
            DueIn = 0,
            LastSeenTurn = -1
        };
    }
}
