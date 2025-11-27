import type { BoardDto } from "@/dto/board.dto";
import { deleteBoard } from "@/utils/api";
import { boardQueryKey, boardsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useBoardDeleteMutation(boardName: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteBoard,
    onSuccess(_data, boardId) {
      toast.success("Board deleted successfully!", {
        description: `Board "${boardName}" has been deleted.`,
      });
      queryClient.setQueryData(
        boardsQueryKey,
        (oldData: BoardDto[] | undefined) => {
          if (!oldData) {
            return [];
          }
          return oldData.filter((item) => item.id !== boardId);
        }
      );
      queryClient.removeQueries({
        exact: true,
        queryKey: boardQueryKey(boardId),
      });
    },
  });
}
