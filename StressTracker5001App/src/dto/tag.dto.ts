export interface TagDto {
  id: number;
  name: string;
  color: string;
  boardId: number;
}

export interface TagCreateDto {
  name: string;
  color: string;
  boardId: number;
}

export interface TagUpdateDto {
  name: string;
  color: string;
}

// Predefined color palette with hex values
export const TAG_COLORS = [
  "#F43F5E", // Rose
  "#EC4899", // Pink
  "#D946EF", // Fuchsia
  "#A855F7", // Purple
  "#8B5CF6", // Violet
  "#6366F1", // Indigo
  "#3B82F6", // Blue
  "#0EA5E9", // Sky
  "#06B6D4", // Cyan
  "#14B8A6", // Teal
  "#10B981", // Emerald
  "#22C55E", // Green
  "#84CC16", // Lime
  "#EAB308", // Yellow
  "#F59E0B", // Amber
  "#F97316", // Orange
  "#EF4444", // Red
  "#78716C", // Stone
  "#737373", // Neutral
  "#71717A", // Zinc
  "#6B7280", // Gray
  "#64748B", // Slate
] as const;

export type TagColor = (typeof TAG_COLORS)[number];
