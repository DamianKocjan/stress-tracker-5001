import z from "zod";

export const LoginFormSchema = z.object({
  email: z.email(),
  password: z.string().min(8, "Password must be at least 8 characters long"),
});

export const RegisterFormSchema = z
  .object({
    email: z.email(),
    username: z.string().min(3, "Username must be at least 3 characters long"),
    password: z.string().min(8, "Password must be at least 8 characters long"),
    confirmPassword: z
      .string()
      .min(8, "Confirm Password must be at least 8 characters long"),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords do not match",
  });

export const UpdateUsernameFormSchema = z.object({
  username: z.string().min(3, "Username must be at least 3 characters long"),
});

export const UpdatePasswordFormSchema = z
  .object({
    currentPassword: z.string().min(1, "Current password is required"),
    newPassword: z
      .string()
      .min(8, "New password must be at least 8 characters long"),
    confirmPassword: z
      .string()
      .min(8, "Confirm password must be at least 8 characters long"),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords do not match",
  });

export const RequestEmailChangeFormSchema = z.object({
  newEmail: z.email("Please enter a valid email address"),
  password: z.string().min(1, "Password is required"),
});

export const ForgotPasswordFormSchema = z.object({
  email: z.email("Please enter a valid email address"),
});

export const ResetPasswordFormSchema = z
  .object({
    newPassword: z
      .string()
      .min(8, "Password must be at least 8 characters long"),
    confirmPassword: z
      .string()
      .min(8, "Confirm password must be at least 8 characters long"),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords do not match",
  });

export const DeleteAccountFormSchema = z.object({
  password: z.string().min(1, "Password is required"),
});
