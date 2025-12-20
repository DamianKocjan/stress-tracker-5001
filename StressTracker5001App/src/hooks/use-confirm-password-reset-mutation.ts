import { confirmPasswordReset } from "@/utils/api";
import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { toast } from "sonner";

export function useConfirmPasswordResetMutation() {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: confirmPasswordReset,
    onSuccess: () => {
      toast.success("Password reset successfully!");
      navigate({ to: "/login", search: { redirect: "/" } });
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
}
