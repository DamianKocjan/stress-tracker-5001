import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldSet,
} from "@/components/ui/field";
import { useConfirmPasswordResetMutation } from "@/hooks/use-confirm-password-reset-mutation";
import { cn } from "@/lib/utils";
import { ResetPasswordFormSchema } from "@/schemas/auth";
import { useForm } from "@tanstack/react-form";
import { Lock } from "lucide-react";
import { PasswordInput } from "../ui/input-password";

export function ResetPasswordForm({
  token,
  className,
  ...props
}: {
  token: string;
} & React.ComponentProps<"div">) {
  const { mutate, isPending } = useConfirmPasswordResetMutation();

  const form = useForm({
    defaultValues: {
      newPassword: "",
      confirmPassword: "",
    },
    validators: {
      onSubmit: ResetPasswordFormSchema,
    },
    async onSubmit({ value }) {
      mutate({
        token,
        newPassword: value.newPassword,
        confirmPassword: value.confirmPassword,
      });
    },
  });

  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <Card className="overflow-hidden p-0">
        <CardContent className="grid p-0">
          <form
            className="p-6 md:p-8"
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();
              form.handleSubmit();
            }}
          >
            <FieldSet>
              <FieldGroup>
                <div className="flex flex-col items-center gap-2 text-center">
                  <div className="rounded-lg bg-muted p-2">
                    <Lock className="h-6 w-6" />
                  </div>
                  <h1 className="text-2xl font-bold">Reset Password</h1>
                  <p className="text-muted-foreground text-balance">
                    Enter your new password below
                  </p>
                </div>

                <form.Field
                  name="newPassword"
                  children={(field) => {
                    const isInvalid =
                      field.state.meta.isTouched && !field.state.meta.isValid;
                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor="new-password">
                          New Password
                        </FieldLabel>
                        <PasswordInput
                          id="new-password"
                          name="new-password"
                          placeholder="••••••••"
                          value={field.state.value}
                          onBlur={field.handleBlur}
                          onChange={(e) => field.handleChange(e.target.value)}
                          aria-invalid={isInvalid}
                          disabled={isPending}
                        />
                        {isInvalid && (
                          <FieldError errors={field.state.meta.errors} />
                        )}
                      </Field>
                    );
                  }}
                />

                <form.Field
                  name="confirmPassword"
                  children={(field) => {
                    const isInvalid =
                      field.state.meta.isTouched && !field.state.meta.isValid;
                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor="confirm-password">
                          Confirm Password
                        </FieldLabel>
                        <PasswordInput
                          id="confirm-password"
                          name="confirm-password"
                          placeholder="••••••••"
                          value={field.state.value}
                          onBlur={field.handleBlur}
                          onChange={(e) => field.handleChange(e.target.value)}
                          aria-invalid={isInvalid}
                          disabled={isPending}
                        />
                        {isInvalid && (
                          <FieldError errors={field.state.meta.errors} />
                        )}
                      </Field>
                    );
                  }}
                />

                <form.Subscribe selector={(s) => [s.canSubmit, s.isSubmitting]}>
                  {([canSubmit, isSubmitting]) => (
                    <Button
                      type="submit"
                      disabled={!canSubmit || isSubmitting || isPending}
                      className="w-full"
                    >
                      {isPending ? "Resetting..." : "Reset Password"}
                    </Button>
                  )}
                </form.Subscribe>
              </FieldGroup>
            </FieldSet>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
