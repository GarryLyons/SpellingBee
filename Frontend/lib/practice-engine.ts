import { PracticeState, SegmentationLevel, SegmentationStep, WordEntry, WordProgress } from "./types";

const SEGMENTATION_LEVEL_META: Record<
  SegmentationLevel,
  { levelNumber: 1 | 2 | 3 | 4 | 5 | 6; title: string }
> = {
  whole_word: { levelNumber: 1, title: "Whole Word" },
  morphological_units: { levelNumber: 2, title: "Morphological Units" },
  syllables: { levelNumber: 3, title: "Syllables" },
  onset_rime: { levelNumber: 4, title: "Onset-Rime" },
  grapheme_groups: { levelNumber: 5, title: "Grapheme Groups" },
  individual_phonemes: { levelNumber: 6, title: "Individual Phonemes" }
};

function buildFallbackGraphemeGroups(word: WordEntry): string | undefined {
  if (word.phonics.graphemes.length <= 1) {
    return undefined;
  }

  const grouped = word.phonics.graphemes.join(" | ");
  return grouped.trim().length > 0 ? grouped : undefined;
}

function buildFallbackIndividualPhonemes(word: WordEntry): string | undefined {
  const letters = word.word.split("");
  if (letters.length <= 1) {
    return undefined;
  }
  return letters.join(" - ");
}

export function getSegmentationPlan(word: WordEntry): SegmentationStep[] {
  const segmentation = word.segmentation ?? {};
  const plan: Array<{ level: SegmentationLevel; value: string | undefined }> = [
    { level: "whole_word", value: word.word },
    { level: "morphological_units", value: segmentation.morphologicalUnits },
    { level: "syllables", value: segmentation.syllables },
    { level: "onset_rime", value: segmentation.onsetRime },
    {
      level: "grapheme_groups",
      value: segmentation.graphemeGroups ?? buildFallbackGraphemeGroups(word)
    },
    {
      level: "individual_phonemes",
      value: segmentation.individualPhonemes ?? buildFallbackIndividualPhonemes(word)
    }
  ];

  const steps: SegmentationStep[] = [];
  for (const item of plan) {
    if (!item.value || item.value.trim().length === 0) {
      continue;
    }

    const meta = SEGMENTATION_LEVEL_META[item.level];
    steps.push({
      level: item.level,
      levelNumber: meta.levelNumber,
      title: meta.title,
      value: item.value
    });
  }

  return steps;
}

export function getCurrentWord(state: PracticeState): WordEntry {
  return state.wordBank.find((word) => word.id === state.currentWordId) ?? state.wordBank[0];
}

export function getCurrentSegmentationStep(state: PracticeState): SegmentationStep {
  const word = getCurrentWord(state);
  const plan = getSegmentationPlan(word);
  const safeIndex = Math.max(0, Math.min(state.currentSegmentationIndex, plan.length - 1));
  return plan[safeIndex];
}

export function getPriorityLabel(progress: WordProgress): "High" | "Medium" | "Low" {
  if (progress.status === "retry") {
    return "High";
  }

  if (progress.status === "fibonacci" && progress.dueIn <= 1) {
    return "Medium";
  }

  if (progress.difficulty >= 6 || progress.firstAttemptIncorrect > progress.firstTryCorrect) {
    return "Medium";
  }

  return "Low";
}
