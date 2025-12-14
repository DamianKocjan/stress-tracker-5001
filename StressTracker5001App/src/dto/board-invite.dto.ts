import type { BoardMemberRole } from "./board-member.dto";
import type { UserDto } from "./user.dto";

export interface BoardInviteDto {
  id: number;
  boardId: number;
  token: string;
  isRevoked: boolean;
  hasBeenUsed: boolean;
  role: BoardMemberRole;
  generatedByUserId: number;
  generatedByUser: UserDto;
  expiresAt: string;
  createdAt: string;
  updatedAt: string;
}

export interface BoardInviteCreateDto {
  role: BoardMemberRole;
  expiresAt: string;
}
