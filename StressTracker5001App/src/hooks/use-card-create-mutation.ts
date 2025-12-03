import type { BoardDetailsDto } from "@/dto/board.dto";
import type { CardCreateDto } from "@/dto/card.dto";
import { createCard } from "@/utils/api";
import { boardQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useCardCreateMutation(boardId: number, columnId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CardCreateDto) => createCard(columnId, data),
    onSuccess(data, variables) {
      toast.success("Card created successfully!", {
        description: `Card "${variables.title}" has been created.`,
      });
      queryClient.setQueryData(
        boardQueryKey(boardId),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) {
            return;
          }

          return {
            ...oldData,
            cards: [...oldData.cards, data],
          } satisfies BoardDetailsDto;
        }
      );
    },
  });
}
