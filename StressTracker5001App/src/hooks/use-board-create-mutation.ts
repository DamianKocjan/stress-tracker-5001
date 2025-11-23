import type { BoardCreateDto, BoardDto } from "@/dto/board.dto";
import { fetch } from "@/utils/fetch";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useBoardCreateMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: BoardCreateDto) => {
      const response = await fetch("/boards", {
        method: "POST",
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error("Failed to create board");
      }

      return response.json() as Promise<BoardDto>;
    },
    onSuccess(data, variables) {
      toast.success("Board created successfully!", {
        description: `Board "${variables.Name}" has been created.`,
      });
      queryClient.setQueryData(
        ["boards"],
        (oldData: BoardDto[] | undefined) => {
          if (!oldData) {
            return [data];
          }
          return [...oldData, data];
        }
      );
    },
  });
}
