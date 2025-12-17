import type { UserDto } from "./user.dto";

export type BoardMemberRole = "Viewer" | "Member" | "Admin" | "Owner";

export interface BoardMemberDto {
  id: number;
  boardId: number;
  userId: number;
  user: UserDto;
  role: BoardMemberRole;
  createdAt: string;
  updatedAt: string;
}

export interface BoardMemberCreateDto {
  userId: number;
  role: BoardMemberRole;
}

export interface BoardMemberUpdateDto {
  role: BoardMemberRole;
}
