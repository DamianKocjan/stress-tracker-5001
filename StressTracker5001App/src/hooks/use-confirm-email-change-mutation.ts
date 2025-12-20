import { confirmEmailChange } from "@/utils/api";
import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { toast } from "sonner";

export function useConfirmEmailChangeMutation() {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: confirmEmailChange,
    onSuccess: () => {
      toast.success("Email verified successfully!");
      setTimeout(() => {
        navigate({ to: "/dashboard" });
      }, 2000);
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
}
