import type { UserDto } from "./user.dto";

export interface CardDetailsDto {
  id: number;
  columnId: number;
  title: string;
  description: string;
  position: number;
  dueDate: string | null;
  createdById: number;
  createdBy: UserDto;
  createdAt: string;
  updatedAt: string;
}

export interface CardDto {
  id: number;
  columnId: number;
  title: string;
  description: string;
  position: number;
  dueDate: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CardCreateDto {
  title: string;
  description?: string;
  dueDate?: string | null;
}

export interface CardMoveDto {
  newPosition: number;
  newColumnId: number;
}

export interface CardUpdateDto {
  title?: string;
  description?: string;
  dueDate?: string | null;
}
