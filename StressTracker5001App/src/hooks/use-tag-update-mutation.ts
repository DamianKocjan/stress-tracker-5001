import type { BoardDetailsDto } from "@/dto/board.dto";
import type { TagUpdateDto } from "@/dto/tag.dto";
import { updateTag } from "@/utils/api";
import { boardQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useTagUpdateMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ tagId, ...data }: TagUpdateDto & { tagId: number }) =>
      updateTag(tagId, data),
    onSuccess(data, { name, tagId }) {
      toast.success("Tag updated successfully!", {
        description: `Tag "${name}" has been updated.`,
      });
      queryClient.setQueryData(
        boardQueryKey(boardId),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) {
            return;
          }

          return {
            ...oldData,
            tags: oldData.tags.map((tag) => (tag.id === tagId ? data : tag)),
          } satisfies BoardDetailsDto;
        }
      );
    },
  });
}
