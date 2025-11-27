import { getBoards } from "@/utils/api";
import { boardsQueryKey } from "@/utils/query-options";
import { useQuery } from "@tanstack/react-query";

export function useBoardsQuery() {
  return useQuery({
    queryKey: boardsQueryKey,
    queryFn: getBoards,
  });
}
