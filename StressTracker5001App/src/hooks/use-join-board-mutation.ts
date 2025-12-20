import { joinBoardWithInvite } from "@/utils/api";
import { boardsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useJoinBoardMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (token: string) => joinBoardWithInvite(token),
    onSuccess(board) {
      toast.success("Joined board successfully!", {
        description: `You are now a member of ${board.name}`,
      });
      queryClient.invalidateQueries({
        queryKey: boardsQueryKey,
      });
    },
    onError(error) {
      toast.error("Failed to join board", {
        description: error instanceof Error ? error.message : "Unknown error",
      });
    },
  });
}
