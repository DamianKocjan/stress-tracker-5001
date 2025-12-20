import type { BoardDetailsDto } from "@/dto/board.dto";
import type { CardDetailsDto } from "@/dto/card.dto";
import { assignUserToCard } from "@/utils/api";
import { boardQueryKey, cardDetailsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export function useCardAssignUserMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ cardId, userId }: { cardId: number; userId: number }) =>
      assignUserToCard(cardId, userId),
    onMutate: async ({ cardId }) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({ queryKey: boardQueryKey(boardId) });
      await queryClient.cancelQueries({
        queryKey: cardDetailsQueryKey(cardId),
      });

      // Snapshot the previous value
      const previousBoard = queryClient.getQueryData<BoardDetailsDto>(
        boardQueryKey(boardId)
      );
      const previousCardDetails = queryClient.getQueryData<CardDetailsDto>(
        cardDetailsQueryKey(cardId)
      );

      // Note: We'll refetch the actual user data from the server on success
      // rather than trying to construct it optimistically without full data
      return { previousBoard, previousCardDetails };
    },
    onError: (_err, _variables, context) => {
      // Rollback to previous state on error
      if (context?.previousBoard) {
        queryClient.setQueryData(boardQueryKey(boardId), context.previousBoard);
      }
      if (context?.previousCardDetails) {
        queryClient.setQueryData(
          cardDetailsQueryKey(_variables.cardId),
          context.previousCardDetails
        );
      }
    },
    onSuccess(_data, { cardId }) {
      // Refetch the card details to get the latest assignments with user data
      queryClient.invalidateQueries({ queryKey: cardDetailsQueryKey(cardId) });
      queryClient.invalidateQueries({ queryKey: boardQueryKey(boardId) });
    },
  });
}
