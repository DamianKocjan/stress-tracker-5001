import { getBoard } from "@/utils/api";
import { boardQueryKey } from "@/utils/query-options";
import { useQuery } from "@tanstack/react-query";
import { useParams } from "@tanstack/react-router";

export function useTagsQuery() {
  const { boardId } = useParams({ from: "/_authenticated/board/$boardId" });
  const boardIdNumber = Number(boardId);

  return useQuery({
    queryKey: boardQueryKey(boardIdNumber),
    queryFn: () => getBoard(boardIdNumber),
    select: (data) => data.tags,
  });
}
