import type { UserDto } from "./user.dto";

/**
 * Define board member roles
 * (0 - Viewer, 1 - Member, 2 - Admin, 3 - Owner)
 */
export type BoardMemberRoleDto = 0 | 1 | 2 | 3;

// Role hierarchy levels for permission checks
export const ROLE = {
  Viewer: 0,
  Member: 1,
  Admin: 2,
  Owner: 3,
} as const;

export const ROLES = Object.values(ROLE);

export const ROLE_NAMES: Record<BoardMemberRoleDto, string> = {
  0: "Viewer",
  1: "Member",
  2: "Admin",
  3: "Owner",
};

export type BoardMemberRole = keyof typeof ROLE;

// Check if user has minimum required role
export function hasMinRole(
  userRole: BoardMemberRoleDto | null,
  requiredRole: BoardMemberRole
): boolean {
  if (userRole === null) return false;
  return userRole >= ROLE[requiredRole];
}

export interface BoardMemberDto {
  id: number;
  boardId: number;
  userId: number;
  user: UserDto;
  role: BoardMemberRoleDto;
  createdAt: string;
  updatedAt: string;
}

export interface BoardMemberCreateDto {
  userId: number;
  role: BoardMemberRoleDto;
}

export interface BoardMemberUpdateDto {
  role: BoardMemberRoleDto;
}
