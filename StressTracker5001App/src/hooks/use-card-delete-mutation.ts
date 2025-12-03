import type { BoardDetailsDto } from "@/dto/board.dto";
import type { CardDto } from "@/dto/card.dto";
import { deleteCard } from "@/utils/api";
import { boardQueryKey, cardDetailsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useCardDeleteMutation(boardId: number, cardName: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteCard,
    onSuccess(_data, id) {
      toast.success("Card deleted successfully!", {
        description: `Card "${cardName}" has been deleted.`,
      });
      queryClient.setQueryData(
        boardQueryKey(boardId),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) {
            return;
          }

          return {
            ...oldData,
            cards: oldData.cards.filter((card: CardDto) => card.id !== id),
          } satisfies BoardDetailsDto;
        }
      );
      queryClient.removeQueries({
        exact: true,
        queryKey: cardDetailsQueryKey(id),
      });
    },
  });
}
