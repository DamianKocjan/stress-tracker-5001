import type { BoardDetailsDto } from "@/dto/board.dto";
import type { ColumnUpdateDto } from "@/dto/column.dto";
import { updateColumn } from "@/utils/api";
import { boardQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useColumnUpdateMutation(boardId: number, columnId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: ColumnUpdateDto) => updateColumn(columnId, data),
    onSuccess(data, variables) {
      toast.success("Column updated successfully!", {
        description: `Column "${variables.name}" has been updated.`,
      });
      queryClient.setQueryData(
        boardQueryKey(boardId),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) {
            return;
          }

          return {
            ...oldData,
            columns: oldData.columns.map((column) =>
              column.id === data.id ? data : column
            ),
          } satisfies BoardDetailsDto;
        }
      );
    },
  });
}
