import type { BoardDetailsDto } from "@/dto/board.dto";
import { deleteColumn } from "@/utils/api";
import { boardQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useColumnDeleteMutation(
  boardId: number,
  columnId: number,
  columnName: string
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => deleteColumn(columnId),
    onSuccess() {
      toast.success("Column deleted successfully!", {
        description: `Column "${columnName}" has been deleted.`,
      });

      queryClient.setQueryData(
        boardQueryKey(boardId),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) {
            return;
          }

          return {
            ...oldData,
            cards: oldData.cards.filter((card) => card.columnId !== columnId),
            columns: oldData.columns.filter((column) => column.id !== columnId),
          } satisfies BoardDetailsDto;
        }
      );
    },
  });
}
