import type { BoardMemberUpdateDto } from "@/dto/board-member.dto";
import { updateMemberRole } from "@/utils/api";
import { boardMembersQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useUpdateMemberRoleMutation(boardId: number, memberId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: BoardMemberUpdateDto) =>
      updateMemberRole(boardId, memberId, data),
    onSuccess(data) {
      toast.success("Member role updated!", {
        description: `${data.user.username}'s role has been changed to ${data.role}.`,
      });
      queryClient.invalidateQueries({
        queryKey: boardMembersQueryKey(boardId),
      });
    },
    onError(error) {
      toast.error("Failed to update member role", {
        description: error instanceof Error ? error.message : "Unknown error",
      });
    },
  });
}
