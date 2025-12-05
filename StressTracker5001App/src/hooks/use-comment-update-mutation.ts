import type { CommentDto, CommentUpdateDto } from "@/dto/comment.dto";
import { updateComment } from "@/utils/api";
import { cardCommentsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export function useCommentUpdateMutation(cardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, content }: CommentUpdateDto & { id: number }) =>
      updateComment(id, content),
    onSuccess(data) {
      // Update the comments in its card
      queryClient.setQueryData(
        cardCommentsQueryKey(cardId),
        (oldData: CommentDto[] | undefined) => {
          if (!oldData) {
            return;
          }

          return oldData.map((comment: CommentDto) =>
            comment.id === data.id ? data : comment
          );
        }
      );
    },
  });
}
