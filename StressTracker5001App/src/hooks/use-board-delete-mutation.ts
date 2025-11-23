import type { BoardDto } from "@/dto/board.dto";
import { fetch } from "@/utils/fetch";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useBoardDeleteMutation(boardId: number, boardName: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      const response = await fetch(`/boards/${boardId}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        throw new Error("Failed to delete board");
      }
    },
    onSuccess() {
      toast.success("Board deleted successfully!", {
        description: `Board "${boardName}" has been deleted.`,
      });
      queryClient.setQueryData(
        ["boards"],
        (oldData: BoardDto[] | undefined) => {
          if (!oldData) {
            return [];
          }
          return oldData.filter((item) => item.id !== boardId);
        }
      );
      queryClient.removeQueries({
        exact: true,
        queryKey: ["boards", boardId],
      });
    },
  });
}
