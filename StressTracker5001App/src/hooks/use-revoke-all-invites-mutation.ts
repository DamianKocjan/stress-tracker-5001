import { revokeAllBoardInvites } from "@/utils/api";
import { boardInvitesQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useRevokeAllInvitesMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => revokeAllBoardInvites(boardId),
    onSuccess() {
      toast.success("All invites revoked successfully!");
      queryClient.invalidateQueries({
        queryKey: boardInvitesQueryKey(boardId),
      });
    },
    onError(error) {
      toast.error("Failed to revoke all invites", {
        description: error instanceof Error ? error.message : "Unknown error",
      });
    },
  });
}
