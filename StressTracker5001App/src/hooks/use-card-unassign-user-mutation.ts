import type { BoardDetailsDto } from "@/dto/board.dto";
import type { CardDetailsDto } from "@/dto/card.dto";
import { unassignUserFromCard } from "@/utils/api";
import { boardQueryKey, cardDetailsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export function useCardUnassignUserMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ cardId, userId }: { cardId: number; userId: number }) =>
      unassignUserFromCard(cardId, userId),
    onMutate: async ({ cardId, userId }) => {
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

      // Optimistically update the cache by removing the assignment
      if (previousBoard) {
        queryClient.setQueryData(
          boardQueryKey(boardId),
          (oldData: BoardDetailsDto | undefined) => {
            if (!oldData) return oldData;

            return {
              ...oldData,
              cards: oldData.cards.map((card) =>
                card.id === cardId
                  ? {
                      ...card,
                      assignments: card.assignments.filter(
                        (a) => a.userId !== userId
                      ),
                    }
                  : card
              ),
            };
          }
        );
      }

      if (previousCardDetails) {
        queryClient.setQueryData(
          cardDetailsQueryKey(cardId),
          (oldData: CardDetailsDto | undefined) => {
            if (!oldData) return oldData;

            return {
              ...oldData,
              assignments: oldData.assignments.filter(
                (a) => a.userId !== userId
              ),
            };
          }
        );
      }

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
      // Refetch the card details to get the latest assignments
      queryClient.invalidateQueries({ queryKey: cardDetailsQueryKey(cardId) });
      queryClient.invalidateQueries({ queryKey: boardQueryKey(boardId) });
    },
  });
}
