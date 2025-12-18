import type { BoardMemberRoleDto } from "./board-member.dto";
import type { UserDto } from "./user.dto";

export interface BoardInviteDto {
  id: number;
  token: string;
  role: BoardMemberRoleDto;
  createdAt: string;
  generatedByUser: UserDto;
}

export interface BoardInviteCreateDto {
  role: BoardMemberRoleDto;
}
