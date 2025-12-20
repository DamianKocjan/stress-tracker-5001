import type { CommentCreateDto, CommentDto } from "@/dto/comment.dto";
import type { PagedResultDto } from "@/dto/common.dto";
import { createComment } from "@/utils/api";
import { cardCommentsQueryKey } from "@/utils/query-options";
import {
  useMutation,
  useQueryClient,
  type InfiniteData,
} from "@tanstack/react-query";
import { COMMENTS_PAGE_SIZE } from "./use-comment-infinite-query";

export function useCommentCreateMutation(cardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ content }: CommentCreateDto) =>
      createComment(cardId, content),
    onSuccess(data) {
      // Update the comments in its card
      queryClient.setQueryData(
        cardCommentsQueryKey(cardId),
        (
          oldData: InfiniteData<PagedResultDto<CommentDto>> | undefined
        ): InfiniteData<PagedResultDto<CommentDto>> => {
          if (!oldData) {
            return {
              pages: [
                {
                  hasMore: false,
                  items: [data],
                  page: 1,
                  pageSize: COMMENTS_PAGE_SIZE,
                  nextPage: 1,
                  previousPage: 1,
                },
              ],
              pageParams: [1],
            };
          }

          // Insert the new comment at the end of the last page
          const newPages = [...oldData.pages];
          const lastPageIndex = newPages.length - 1;
          const lastPage = newPages[lastPageIndex];
          if (lastPage.items.length < COMMENTS_PAGE_SIZE) {
            // There's space in the last page
            newPages[lastPageIndex] = {
              ...lastPage,
              items: [...lastPage.items, data],
            };
          } else {
            // Need to create a new page
            newPages.push({
              hasMore: false,
              items: [data],
              page: newPages.length + 1,
              pageSize: COMMENTS_PAGE_SIZE,
              nextPage: newPages.length + 1,
              previousPage: newPages.length,
            });
          }

          return {
            pages: newPages,
            pageParams: new Array(newPages.length).map((_, i) => i + 1),
          };
        }
      );
    },
  });
}
