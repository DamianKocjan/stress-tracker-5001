import type { BoardDetailsDto } from "@/dto/board.dto";
import type { CardAssignTagsDto, CardDetailsDto } from "@/dto/card.dto";
import { assignTagsToCard } from "@/utils/api";
import { boardQueryKey, cardDetailsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export function useCardAssignTagsMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ cardId, tags }: CardAssignTagsDto & { cardId: number }) =>
      assignTagsToCard(cardId, tags),
    onMutate: async ({ cardId, tags }) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({ queryKey: boardQueryKey(boardId) });

      // Snapshot the previous value
      const previousBoard = queryClient.getQueryData<BoardDetailsDto>(
        boardQueryKey(boardId)
      );

      // Optimistically update the cache
      if (previousBoard) {
        queryClient.setQueryData(
          boardQueryKey(boardId),
          (oldData: BoardDetailsDto | undefined) => {
            if (!oldData) return oldData;

            return {
              ...oldData,
              cards: oldData.cards.map((card) =>
                card.id === cardId ? { ...card, tags } : card
              ),
            };
          }
        );
      }

      return { previousBoard };
    },
    onError: (_err, _variables, context) => {
      // Rollback to previous state on error
      if (context?.previousBoard) {
        queryClient.setQueryData(boardQueryKey(boardId), context.previousBoard);
      }
    },
    onSuccess(_data, { cardId, tags }) {
      // Update the board's cards to reflect the assigned tags
      queryClient.setQueryData(
        boardQueryKey(boardId),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) {
            return;
          }

          return {
            ...oldData,
            cards: oldData.cards.map((card) =>
              card.id === cardId ? { ...card, tags } : card
            ),
          } satisfies BoardDetailsDto;
        }
      );

      // Update the card details cache if it exists
      queryClient.setQueryData(
        cardDetailsQueryKey(cardId),
        (oldData: CardDetailsDto | undefined) => {
          if (!oldData) {
            return;
          }

          return { ...oldData, tags };
        }
      );
    },
    onSettled: () => {
      // Invalidate to refetch the latest data
      queryClient.invalidateQueries({ queryKey: boardQueryKey(boardId) });
    },
  });
}
