import { updatePassword } from "@/utils/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useUpdatePasswordMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updatePassword,
    onSuccess: () => {
      toast.success("Password updated successfully");
      queryClient.invalidateQueries({ queryKey: ["user"] });
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
}
