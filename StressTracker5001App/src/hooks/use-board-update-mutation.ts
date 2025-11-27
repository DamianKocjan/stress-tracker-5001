import type {
  BoardDetailsDto,
  BoardDto,
  BoardUpdateDto,
} from "@/dto/board.dto";
import { updateBoard } from "@/utils/api";
import { boardQueryKey, boardsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useBoardUpdateMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: BoardUpdateDto) => updateBoard(boardId, data),
    onSuccess(data, variables) {
      toast.success("Board updated successfully!", {
        description: `Board "${variables.name}" has been updated.`,
      });
      queryClient.setQueryData(
        boardsQueryKey,
        (oldData: BoardDto[] | undefined) => {
          if (!oldData) {
            return [data];
          }
          return oldData.map((item) => (item.id === data.id ? data : item));
        }
      );
      queryClient.setQueryData(
        boardQueryKey(boardId),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) {
            return data;
          }
          return { ...oldData, ...data };
        }
      );
    },
  });
}
