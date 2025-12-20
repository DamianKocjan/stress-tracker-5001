import type { CommentDto } from "@/dto/comment.dto";
import type { PagedResultDto } from "@/dto/common.dto";
import { deleteComment } from "@/utils/api";
import { cardCommentsQueryKey } from "@/utils/query-options";
import {
  useMutation,
  useQueryClient,
  type InfiniteData,
} from "@tanstack/react-query";
import { COMMENTS_PAGE_SIZE } from "./use-comment-infinite-query";

export function useCommentDeleteMutation(cardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (commentId: number) => deleteComment(commentId),
    onSuccess(_data, commentId) {
      // Update the comments in its card
      queryClient.setQueryData(
        cardCommentsQueryKey(cardId),
        (
          oldData: InfiniteData<PagedResultDto<CommentDto>> | undefined
        ): InfiniteData<PagedResultDto<CommentDto>> | undefined => {
          if (!oldData) {
            return;
          }

          // Filter out the deleted comment from all pages
          const newPages = oldData.pages.map((page) => {
            return {
              ...page,
              items: page.items.filter(
                (comment: CommentDto) => comment.id !== commentId
              ),
            };
          });

          // Redistribute comments across pages if needed
          const totalComments = newPages.reduce(
            (acc, page) => acc + page.items.length,
            0
          );
          const totalPages = Math.ceil(totalComments / COMMENTS_PAGE_SIZE);

          if (totalPages !== oldData.pages.length) {
            const allComments = newPages.flatMap((page) => page.items);
            const redistributedPages: PagedResultDto<CommentDto>[] = [];

            for (let i = 0; i < totalPages; i++) {
              redistributedPages.push({
                items: allComments.slice(
                  i * COMMENTS_PAGE_SIZE,
                  (i + 1) * COMMENTS_PAGE_SIZE
                ),
                hasMore: i < totalPages - 1,
                previousPage: i > 0 ? i : 1,
                page: i + 1,
                nextPage: i < totalPages - 1 ? i + 2 : i + 1,
                pageSize: COMMENTS_PAGE_SIZE,
              });
            }

            return {
              pages: redistributedPages,
              pageParams: new Array(totalPages).map((_, i) => i + 1),
            };
          }
        }
      );
    },
  });
}
