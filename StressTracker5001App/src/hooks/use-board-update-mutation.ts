import type { BoardCreateDto, BoardDto } from "@/dto/board.dto";
import { fetch } from "@/utils/fetch";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useBoardUpdateMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: BoardCreateDto) => {
      const response = await fetch(`/boards/${boardId}`, {
        method: "PUT",
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error("Failed to update board");
      }

      return response.json() as Promise<BoardDto>;
    },
    onSuccess(data, variables) {
      toast.success("Board updated successfully!", {
        description: `Board "${variables.name}" has been updated.`,
      });
      queryClient.setQueryData(
        ["boards"],
        (oldData: BoardDto[] | undefined) => {
          if (!oldData) {
            return [data];
          }
          return oldData.map((item) => (item.id === data.id ? data : item));
        }
      );
      queryClient.setQueryData(["boards", data.id], data);
    },
  });
}
