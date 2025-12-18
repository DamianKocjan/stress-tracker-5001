import { revokeInvite } from "@/utils/api";
import { boardInvitesQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useRevokeInviteMutation(boardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (inviteId: number) => revokeInvite(inviteId),
    onSuccess() {
      toast.success("Invite revoked successfully!");
      queryClient.invalidateQueries({
        queryKey: boardInvitesQueryKey(boardId),
      });
    },
    onError(error) {
      toast.error("Failed to revoke invite", {
        description: error instanceof Error ? error.message : "Unknown error",
      });
    },
  });
}
