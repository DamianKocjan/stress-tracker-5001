import type { CommentDto, CommentUpdateDto } from "@/dto/comment.dto";
import type { PagedResultDto } from "@/dto/common.dto";
import { updateComment } from "@/utils/api";
import { cardCommentsQueryKey } from "@/utils/query-options";
import {
  useMutation,
  useQueryClient,
  type InfiniteData,
} from "@tanstack/react-query";
import { COMMENTS_PAGE_SIZE } from "./use-comment-infinite-query";

export function useCommentUpdateMutation(cardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, content }: CommentUpdateDto & { id: number }) =>
      updateComment(id, content),
    onSuccess(data) {
      // Update the comments in its card
      queryClient.setQueryData(
        cardCommentsQueryKey(cardId, COMMENTS_PAGE_SIZE),
        (
          oldData: InfiniteData<PagedResultDto<CommentDto>> | undefined
        ): InfiniteData<PagedResultDto<CommentDto>> | undefined => {
          if (!oldData) {
            return;
          }

          // Update the specific comment in all pages
          const newPages = oldData.pages.map((page) => {
            return {
              ...page,
              items: page.items.map((comment: CommentDto) =>
                comment.id === data.id ? data : comment
              ),
            };
          });
          return {
            pages: newPages,
            pageParams: oldData.pageParams,
          };
        }
      );
    },
  });
}
