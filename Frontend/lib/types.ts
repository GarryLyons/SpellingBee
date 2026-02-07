export interface PhonicsBreakdown {
  phonemes: string[];
  graphemes: string[];
  segmenting: string;
  blending: string;
  digraphs: string[];
  splitDigraphs: string[];
  rules: string[];
}

export type SegmentationLevel =
  | "whole_word"
  | "morphological_units"
  | "syllables"
  | "onset_rime"
  | "grapheme_groups"
  | "individual_phonemes";

export interface SegmentationOptions {
  morphologicalUnits?: string;
  syllables?: string;
  onsetRime?: string;
  graphemeGroups?: string;
  individualPhonemes?: string;
}

export interface SegmentationStep {
  level: SegmentationLevel;
  levelNumber: 1 | 2 | 3 | 4 | 5 | 6;
  title: string;
  value: string;
}

export interface WordEntry {
  id: string;
  word: string;
  phonics: PhonicsBreakdown;
  segmentation?: SegmentationOptions;
}

export interface WordProgress {
  wordId: string;
  attempts: number;
  firstTryCorrect: number;
  correctedAfterSupport: number;
  firstAttemptIncorrect: number;
  highPriorityIncorrect: number;
  status: "normal" | "retry" | "fibonacci";
  nextFibonacciIndex: number;
  wasEverWrong: boolean;
  streak: number;
  difficulty: number;
  dueIn: number;
  lastSeenTurn: number;
}

export type PracticePhase = "attempt" | "model_prompt";

export type AttemptOutcome = "correct" | "incorrect";

export interface PracticeState {
  wordBank: WordEntry[];
  progress: Record<string, WordProgress>;
  currentWordId: string;
  currentSegmentationIndex: number;
  level1MissedOnCurrentWord: boolean;
  phase: PracticePhase;
  turn: number;
  cycle: number;
  seenInCycle: string[];
}

export interface PracticeSessionResponse {
  sessionId: string;
  state: PracticeState;
}
