import type { BoardMemberRoleDto } from "./board-member.dto";
import type { UserDto } from "./user.dto";

export interface BoardInviteDto {
  id: number;
  boardId: number;
  token: string;
  isRevoked: boolean;
  hasBeenUsed: boolean;
  role: BoardMemberRoleDto;
  generatedByUserId: number;
  generatedByUser: UserDto;
  expiresAt: string;
  createdAt: string;
  updatedAt: string;
}

export interface BoardInviteCreateDto {
  role: BoardMemberRoleDto;
  expiresAt: string;
}
