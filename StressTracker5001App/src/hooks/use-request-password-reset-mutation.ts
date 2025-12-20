import { requestPasswordReset } from "@/utils/api";
import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { toast } from "sonner";

export function useRequestPasswordResetMutation() {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: requestPasswordReset,
    onSuccess: () => {
      toast.success("Check your email for password reset instructions");
      navigate({ to: "/login", search: { redirect: "/" } });
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
}
