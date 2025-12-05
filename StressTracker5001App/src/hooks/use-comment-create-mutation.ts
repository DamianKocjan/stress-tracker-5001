import type { CommentCreateDto, CommentDto } from "@/dto/comment.dto";
import { createComment } from "@/utils/api";
import { cardCommentsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export function useCommentCreateMutation(cardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ content }: CommentCreateDto) =>
      createComment(cardId, content),
    onSuccess(data) {
      // Update the comments in its card
      queryClient.setQueryData(
        cardCommentsQueryKey(cardId),
        (oldData: CommentDto[] | undefined) => {
          if (!oldData) {
            return [data];
          }

          return [...oldData, data];
        }
      );
    },
  });
}
