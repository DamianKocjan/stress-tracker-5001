import { getBoardInvites } from "@/utils/api";
import { boardInvitesQueryKey } from "@/utils/query-options";
import { useQuery } from "@tanstack/react-query";

export function useBoardInvitesQuery(boardId: number) {
  return useQuery({
    queryKey: boardInvitesQueryKey(boardId),
    queryFn: () => getBoardInvites(boardId),
  });
}
