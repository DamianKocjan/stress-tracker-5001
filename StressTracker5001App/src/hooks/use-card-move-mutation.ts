import type { BoardDetailsDto } from "@/dto/board.dto";
import type { CardDetailsDto, CardMoveDto } from "@/dto/card.dto";
import { moveCard } from "@/utils/api";
import { boardQueryKey, cardDetailsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export function useCardMoveMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ cardId, ...data }: CardMoveDto & { cardId: number }) =>
      moveCard(cardId, data),
    onMutate: async ({ cardId, newColumnId, newPosition }) => {
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
                card.id === cardId
                  ? { ...card, columnId: newColumnId, position: newPosition }
                  : card
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
    onSuccess(data) {
      // Update the board's cards to reflect the moved card
      queryClient.setQueryData(
        boardQueryKey(boardId),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) {
            return;
          }

          return {
            ...oldData,
            cards: oldData.cards.map((card) =>
              card.id === data.id ? data : card
            ),
          } satisfies BoardDetailsDto;
        }
      );

      // Update the card so that its position and columnId are correct
      queryClient.setQueryData(
        cardDetailsQueryKey(data.id),
        (oldData: CardDetailsDto | undefined) => {
          if (!oldData) {
            return data;
          }

          return { ...oldData, ...data };
        }
      );
    },
    onSettled: () => {
      // Invalidate to refetch the latest data
      queryClient.invalidateQueries({ queryKey: boardQueryKey(boardId) });
    },
  });
}
