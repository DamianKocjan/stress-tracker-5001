import { getBoard } from "@/utils/api";
import { boardQueryKey } from "@/utils/query-options";
import { useQuery } from "@tanstack/react-query";

export function useBoardQuery(boardId: number) {
  return useQuery({
    queryKey: boardQueryKey(boardId),
    queryFn: () => getBoard(boardId),
  });
}
