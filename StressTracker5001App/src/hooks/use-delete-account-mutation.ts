import { deleteAccount } from "@/utils/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { toast } from "sonner";

export function useDeleteAccountMutation() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteAccount,
    onSuccess: () => {
      toast.success("Account deleted successfully");
      queryClient.clear();
      navigate({ to: "/login", search: { redirect: "/" } });
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
}
