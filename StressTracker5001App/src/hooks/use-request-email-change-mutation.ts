import { requestEmailChange } from "@/utils/api";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useRequestEmailChangeMutation() {
  return useMutation({
    mutationFn: requestEmailChange,
    onSuccess: () => {
      toast.success("Verification email sent to your new address");
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
}
