import { getCardCommentsPaged } from "@/utils/api";
import { cardCommentsQueryKey } from "@/utils/query-options";
import { useInfiniteQuery } from "@tanstack/react-query";

export const COMMENTS_PAGE_SIZE = 10;

export function useCardCommentsInfiniteQuery(cardId: number) {
  return useInfiniteQuery({
    queryKey: cardCommentsQueryKey(cardId, COMMENTS_PAGE_SIZE),
    queryFn: ({ pageParam = 1 }) =>
      getCardCommentsPaged(cardId, pageParam, COMMENTS_PAGE_SIZE),
    initialPageParam: 1,
    getNextPageParam: (lastPage, allPages) => {
      if (lastPage.hasMore) {
        return allPages.length + 1;
      }
      return undefined;
    },
  });
}
