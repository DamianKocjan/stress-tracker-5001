import type { BoardDto } from "@/dto/board.dto";
import { createBoard } from "@/utils/api";
import { boardQueryKey, boardsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useBoardCreateMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createBoard,
    onSuccess(data, variables) {
      toast.success("Board created successfully!", {
        description: `Board "${variables.name}" has been created.`,
      });
      queryClient.setQueryData(
        boardsQueryKey,
        (oldData: BoardDto[] | undefined) => {
          if (!oldData) {
            return [data];
          }
          return [...oldData, data];
        }
      );
      queryClient.setQueryData(boardQueryKey(data.id), data);
    },
  });
}
