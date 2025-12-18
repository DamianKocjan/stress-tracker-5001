import { getBoardMembers } from "@/utils/api";
import { boardMembersQueryKey } from "@/utils/query-options";
import { useQuery } from "@tanstack/react-query";

export function useBoardMembersQuery(boardId: number) {
  return useQuery({
    queryKey: boardMembersQueryKey(boardId),
    queryFn: () => getBoardMembers(boardId),
  });
}
