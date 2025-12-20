import { getBoardActivityLogs } from "@/utils/api";
import { boardActivityLogsQueryKey } from "@/utils/query-options";
import { useQuery } from "@tanstack/react-query";

interface UseBoardActivityLogsQueryParams {
  boardId: number;
  page?: number;
  pageSize?: number;
  entityType?: number;
  actionType?: number;
}

export function useBoardActivityLogsQuery({
  boardId,
  page = 1,
  pageSize = 10,
  entityType,
  actionType,
}: UseBoardActivityLogsQueryParams) {
  return useQuery({
    queryKey: boardActivityLogsQueryKey(
      boardId,
      page,
      pageSize,
      entityType,
      actionType
    ),
    queryFn: () =>
      getBoardActivityLogs(boardId, {
        page,
        pageSize,
        entityType,
        actionType,
      }),
  });
}
