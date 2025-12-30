import type { AttachmentDto } from "./attachment.dto";
import type { CardAssignmentDto } from "./card-assignment.dto";
import type { UserDto } from "./user.dto";

export interface CardDetailsDto {
  id: number;
  columnId: number;
  title: string;
  description: string;
  position: number;
  dueDate: string | null;
  createdById: number;
  tags: number[];
  assignments: CardAssignmentDto[];
  createdBy: UserDto;
  attachments: AttachmentDto[];
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
  createdById: number;
  createdAt: string;
  updatedAt: string;
  tags: number[];
  assignments: CardAssignmentDto[];
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

export interface CardAssignTagsDto {
  tags: number[];
}

export interface CardUpdateDto {
  title?: string;
  description?: string;
  dueDate?: string | null;
}
