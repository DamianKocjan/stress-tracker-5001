import {
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { toast } from "sonner";
import type {
  ConfirmEmailChangeDto,
  ConfirmPasswordResetDto,
  DeleteAccountDto,
  RequestEmailChangeDto,
  RequestPasswordResetDto,
  ResendVerificationEmailDto,
} from "@/dto/auth.dto";

const API_BASE = "/api/auth";

export const useRequestPasswordReset = () => {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: async (data: RequestPasswordResetDto) => {
      const response = await fetch(`${API_BASE}/request-password-reset`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(
          error.errorMessage || "Failed to request password reset"
        );
      }

      return response.json();
    },
    onSuccess: () => {
      toast.success("Check your email for password reset instructions");
      navigate({ to: "/login", search: { redirect: "/" } });
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
};

export const useConfirmPasswordReset = () => {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: async (data: ConfirmPasswordResetDto) => {
      const response = await fetch(`${API_BASE}/confirm-password-reset`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.errorMessage || "Failed to reset password");
      }

      return response.json();
    },
    onSuccess: () => {
      toast.success("Password reset successfully!");
      navigate({ to: "/login", search: { redirect: "/" } });
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
};

export const useConfirmEmailChange = () => {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: async (data: ConfirmEmailChangeDto) => {
      const response = await fetch(`${API_BASE}/confirm-email-change`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.errorMessage || "Failed to verify email");
      }

      return response.json();
    },
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
};

export const useRequestEmailChange = () => {
  return useMutation({
    mutationFn: async (data: RequestEmailChangeDto) => {
      const response = await fetch(`${API_BASE}/request-email-change`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.errorMessage || "Failed to request email change");
      }

      return response.json();
    },
    onSuccess: () => {
      toast.success("Verification email sent to your new address");
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
};

export const useUpdateProfile = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (
      payload: Record<string, unknown>
    ): Promise<{ message: string }> => {
      const response = await fetch("/api/auth/profile/update", {
        method: "PATCH",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || "Failed to update profile");
      }

      return response.json();
    },
    onSuccess: () => {
      toast.success("Profile updated successfully");
      queryClient.invalidateQueries({ queryKey: ["user"] });
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
};

export const useDeleteAccount = () => {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: DeleteAccountDto) => {
      const response = await fetch(`${API_BASE}/delete-account`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.errorMessage || "Failed to delete account");
      }

      return response.json();
    },
    onSuccess: () => {
      toast.success("Account deleted successfully");
      queryClient.clear();
      navigate({ to: "/login", search: { redirect: "/" } });
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
};

export const useResendVerificationEmail = () => {
  return useMutation({
    mutationFn: async (data: ResendVerificationEmailDto) => {
      const response = await fetch(`${API_BASE}/resend-verification-email`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.errorMessage || "Failed to resend email");
      }

      return response.json();
    },
    onSuccess: () => {
      toast.success("Verification email sent");
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
};
