import { updateProfile } from "@/utils/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useUpdateProfileMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateProfile,
    onSuccess: () => {
      toast.success("Profile updated successfully");
      queryClient.invalidateQueries({ queryKey: ["user"] });
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
}
