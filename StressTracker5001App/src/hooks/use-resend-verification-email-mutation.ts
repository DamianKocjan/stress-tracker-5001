import { resendVerificationEmail } from "@/utils/api";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useResendVerificationEmailMutation() {
  return useMutation({
    mutationFn: resendVerificationEmail,
    onSuccess: () => {
      toast.success("Verification email sent");
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
}
