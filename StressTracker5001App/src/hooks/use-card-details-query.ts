import { getCardDetails } from "@/utils/api";
import { cardDetailsQueryKey } from "@/utils/query-options";
import { useQuery } from "@tanstack/react-query";

export function useCardDetailsQuery(cardId: number) {
  return useQuery({
    queryKey: cardDetailsQueryKey(cardId),
    queryFn: () => getCardDetails(cardId),
  });
}
