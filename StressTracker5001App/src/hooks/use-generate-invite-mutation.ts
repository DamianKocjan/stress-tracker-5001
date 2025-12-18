import type { BoardInviteCreateDto } from "@/dto/board-invite.dto";
import { generateBoardInvite } from "@/utils/api";
import { boardInvitesQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useGenerateInviteMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: BoardInviteCreateDto) =>
      generateBoardInvite(boardId, data),
    onSuccess() {
      toast.success("Invite generated successfully!", {
        description:
          "Share this invite link with users to add them to the board.",
      });
      queryClient.invalidateQueries({
        queryKey: boardInvitesQueryKey(boardId),
      });
    },
    onError(error) {
      toast.error("Failed to generate invite", {
        description: error instanceof Error ? error.message : "Unknown error",
      });
    },
  });
}
