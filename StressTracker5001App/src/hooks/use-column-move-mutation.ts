import type { BoardDetailsDto } from "@/dto/board.dto";
import type { ColumnDto, ColumnMoveDto } from "@/dto/column.dto";
import { moveColumn } from "@/utils/api";
import { boardQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export function useColumnMoveMutation(boardId: number, columnId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: ColumnMoveDto) => moveColumn(columnId, data),
    onSuccess(data) {
      // Update all columns that might contain the column
      queryClient.setQueryData(
        boardQueryKey(boardId),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) {
            return;
          }

          return {
            ...oldData,
            columns: oldData.columns.map((column: ColumnDto) => {
              if (column.id === columnId) {
                return { ...column, ...data };
              }
              return column;
            }),
          };
        }
      );
    },
  });
}
