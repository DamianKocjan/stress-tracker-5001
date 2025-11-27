import type { BoardDetailsDto } from "@/dto/board.dto";
import type { CardDetailsDto, CardMoveDto } from "@/dto/card.dto";
import { moveCard } from "@/utils/api";
import { boardQueryKey, cardDetailsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export function useCardMoveMutation(boardId: number, cardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CardMoveDto) => moveCard(cardId, data),
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
              card.id === cardId ? data : card
            ),
          } satisfies BoardDetailsDto;
        }
      );

      // Update the card so that its position and columnId are correct
      queryClient.setQueryData(
        cardDetailsQueryKey(cardId),
        (oldData: CardDetailsDto | undefined) => {
          if (!oldData) {
            return data;
          }

          return { ...oldData, ...data };
        }
      );
    },
  });
}
