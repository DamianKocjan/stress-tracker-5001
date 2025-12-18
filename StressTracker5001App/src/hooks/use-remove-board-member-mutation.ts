import { removeBoardMember } from "@/utils/api";
import { boardMembersQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useRemoveBoardMemberMutation(
  boardId: number,
  memberId: number
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => removeBoardMember(boardId, memberId),
    onSuccess() {
      toast.success("Member removed successfully!");
      queryClient.invalidateQueries({
        queryKey: boardMembersQueryKey(boardId),
      });
    },
    onError(error) {
      toast.error("Failed to remove member", {
        description: error instanceof Error ? error.message : "Unknown error",
      });
    },
  });
}
