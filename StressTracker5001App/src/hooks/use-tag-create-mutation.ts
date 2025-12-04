import type { BoardDetailsDto } from "@/dto/board.dto";
import type { TagCreateDto } from "@/dto/tag.dto";
import { createTag } from "@/utils/api";
import { boardQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useTagCreateMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: TagCreateDto) => createTag(boardId, data),
    onSuccess(data, variables) {
      toast.success("Tag created successfully!", {
        description: `Tag "${variables.name}" has been created.`,
      });
      queryClient.setQueryData(
        boardQueryKey(boardId),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) {
            return;
          }
          return {
            ...oldData,
            tags: [...oldData.tags, data],
          } satisfies BoardDetailsDto;
        }
      );
    },
  });
}
