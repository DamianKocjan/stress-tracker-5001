import type { BoardMemberRole } from "@/dto/board-member.dto";
import { hasMinRole } from "@/dto/board-member.dto";
import { useUserBoardRole } from "@/hooks/use-user-board-role";
import type { ReactNode } from "react";

interface RoleGuardProps {
  minRole: BoardMemberRole;
  children: ReactNode;
  fallback?: ReactNode;
}

export function RoleGuard({
  minRole,
  children,
  fallback = null,
}: RoleGuardProps) {
  const role = useUserBoardRole();

  if (hasMinRole(role, minRole)) {
    return children;
  }

  return fallback;
}
