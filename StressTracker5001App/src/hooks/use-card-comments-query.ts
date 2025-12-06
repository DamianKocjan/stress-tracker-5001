import { getCardComments } from "@/utils/api";
import { cardCommentsQueryKey } from "@/utils/query-options";
import { useQuery } from "@tanstack/react-query";

export function useCardCommentsQuery(cardId: number) {
  return useQuery({
    queryKey: cardCommentsQueryKey(cardId),
    queryFn: () => getCardComments(cardId),
  });
}
