import { getCardComments } from "@/utils/api";
import { cardCommentsQueryKey } from "@/utils/query-options";
import { useQuery } from "@tanstack/react-query";

export function useCommentQuery(cardId: number, commentId?: number) {
  return useQuery({
    queryKey: cardCommentsQueryKey(cardId),
    queryFn: () => getCardComments(cardId),
    select: (data) => data.find((comment) => comment.id === commentId),
    enabled: !!commentId,
  });
}
