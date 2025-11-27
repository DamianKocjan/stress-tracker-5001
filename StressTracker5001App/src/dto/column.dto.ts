export interface ColumnDto {
  id: number;
  boardId: number;
  name: string;
  position: number;
  wipLimit: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface ColumnCreateDto {
  name: string;
  position: number;
  wipLimit?: number | null;
}

export interface ColumnMoveDto {
  newPosition: number;
}

export interface ColumnUpdateDto {
  name?: string;
  wipLimit?: number | null;
}
