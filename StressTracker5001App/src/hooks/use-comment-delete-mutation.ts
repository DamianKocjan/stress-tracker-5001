import type { CommentDto } from "@/dto/comment.dto";
import { deleteComment } from "@/utils/api";
import { cardCommentsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export function useCommentDeleteMutation(cardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (commentId: number) => deleteComment(commentId),
    onSuccess(_data, commentId) {
      // Update the comments in its card
      queryClient.setQueryData(
        cardCommentsQueryKey(cardId),
        (oldData: CommentDto[] | undefined) => {
          if (!oldData) {
            return;
          }

          return oldData.filter(
            (comment: CommentDto) => comment.id !== commentId
          );
        }
      );
    },
  });
}
