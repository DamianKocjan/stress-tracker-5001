import type { BoardDetailsDto } from "@/dto/board.dto";
import type { ColumnDto, ColumnMoveDto } from "@/dto/column.dto";
import { moveColumn } from "@/utils/api";
import { boardQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export function useColumnMoveMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ columnId, ...data }: ColumnMoveDto & { columnId: number }) =>
      moveColumn(columnId, data),
    onMutate: async ({ columnId, newPosition }) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({ queryKey: boardQueryKey(boardId) });

      // Snapshot the previous value
      const previousBoard = queryClient.getQueryData<BoardDetailsDto>(
        boardQueryKey(boardId)
      );

      // Optimistically update the cache
      if (previousBoard) {
        queryClient.setQueryData(
          boardQueryKey(boardId),
          (oldData: BoardDetailsDto | undefined) => {
            if (!oldData) return oldData;

            const columns = [...oldData.columns];
            const activeIndex = columns.findIndex((c) => c.id === columnId);
            if (activeIndex === -1) return oldData;

            // Move column to new position
            const [movedColumn] = columns.splice(activeIndex, 1);
            columns.splice(newPosition, 0, movedColumn);

            // Recalculate positions
            const updatedColumns = columns.map((col, index) => ({
              ...col,
              position: index,
            }));

            return {
              ...oldData,
              columns: updatedColumns,
            };
          }
        );
      }

      return { previousBoard };
    },
    onError: (_err, _variables, context) => {
      // Rollback to previous state on error
      if (context?.previousBoard) {
        queryClient.setQueryData(boardQueryKey(boardId), context.previousBoard);
      }
    },
    onSuccess(data, { columnId }) {
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
    onSettled: () => {
      // Invalidate to refetch the latest data
      queryClient.invalidateQueries({ queryKey: boardQueryKey(boardId) });
    },
  });
}
