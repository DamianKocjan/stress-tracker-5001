import type { BoardMemberRoleDto } from "./board-member.dto";

export interface BoardInviteDto {
  token: string;
}

export interface BoardInviteCreateDto {
  role: BoardMemberRoleDto;
}
