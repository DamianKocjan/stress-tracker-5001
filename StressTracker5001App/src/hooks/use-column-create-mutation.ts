import type { BoardDetailsDto } from "@/dto/board.dto";
import type { ColumnCreateDto } from "@/dto/column.dto";
import { createColumn } from "@/utils/api";
import { boardQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useColumnCreateMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: ColumnCreateDto) => createColumn(boardId, data),
    onSuccess(data, variables) {
      toast.success("Column created successfully!", {
        description: `Column "${variables.name}" has been created.`,
      });
      queryClient.setQueryData(
        boardQueryKey(boardId),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) {
            return;
          }
          return {
            ...oldData,
            columns: [...oldData.columns, data],
          } satisfies BoardDetailsDto;
        }
      );
    },
  });
}
