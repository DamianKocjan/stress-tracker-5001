import type { BoardDetailsDto } from "@/dto/board.dto";
import type { TagDto } from "@/dto/tag.dto";
import { deleteTag } from "@/utils/api";
import { boardQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useTagDeleteMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: TagDto) => deleteTag(data.id),
    onSuccess(_data, { id, name }) {
      toast.success("Tag deleted successfully!", {
        description: `Tag "${name}" has been deleted.`,
      });

      queryClient.setQueryData(
        boardQueryKey(boardId),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) {
            return;
          }

          return {
            ...oldData,
            tags: oldData.tags.filter((tag) => tag.id !== id),
          } satisfies BoardDetailsDto;
        }
      );
    },
  });
}
