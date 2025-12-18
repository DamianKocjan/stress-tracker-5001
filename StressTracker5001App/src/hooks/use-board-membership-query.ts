import { getBoardMembership } from "@/utils/api";
import { boardMembershipQueryKey } from "@/utils/query-options";
import { useQuery } from "@tanstack/react-query";

export function useBoardMembershipQuery(boardId: number) {
  return useQuery({
    queryKey: boardMembershipQueryKey(boardId),
    queryFn: () => getBoardMembership(boardId),
  });
}
