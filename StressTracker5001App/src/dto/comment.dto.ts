import type { UserDto } from "./user.dto";

export interface CommentDto {
  id: number;
  content: string;
  userId: number;
  user: UserDto;
  createdAt: string;
  updatedAt: string;
}

export interface CommentCreateDto {
  content: string;
}

export interface CommentUpdateDto {
  content: string;
}
