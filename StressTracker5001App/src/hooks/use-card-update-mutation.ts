import type { BoardDetailsDto } from "@/dto/board.dto";
import type { CardDto, CardUpdateDto } from "@/dto/card.dto";
import { updateCard } from "@/utils/api";
import { boardQueryKey, cardDetailsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export function useCardUpdateMutation(boardId: number, cardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CardUpdateDto) => updateCard(cardId, data),
    onSuccess(data) {
      // Update the card in its board
      queryClient.setQueryData(
        boardQueryKey(boardId),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) {
            return;
          }

          return {
            ...oldData,
            cards: oldData.cards.map((card: CardDto) =>
              card.id === data.id ? data : card
            ),
          } satisfies BoardDetailsDto;
        }
      );

      // Update the card so that its position and columnId are correct
      queryClient.setQueryData(
        cardDetailsQueryKey(data.id),
        (oldData: CardDto | undefined) => {
          if (!oldData) {
            return data;
          }

          return { ...oldData, ...data };
        }
      );
    },
  });
}
