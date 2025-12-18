import type {
  BoardMemberRole,
  BoardMemberRoleDto,
} from "@/dto/board-member.dto";
import { hasMinRole } from "@/dto/board-member.dto";
import { useParams } from "@tanstack/react-router";
import { useBoardMembershipQuery } from "./use-board-membership-query";

export interface UserBoardRole {
  role: BoardMemberRoleDto | null;
}

export function useUserBoardRole() {
  const { boardId } = useParams({
    from: "/_authenticated/board/$boardId",
  });

  const { data: boardMembership } = useBoardMembershipQuery(Number(boardId));

  return boardMembership?.role ?? null;
}

export function useHasMinBoardRole(requiredRole: BoardMemberRole): boolean {
  const role = useUserBoardRole();
  return hasMinRole(role, requiredRole);
}
