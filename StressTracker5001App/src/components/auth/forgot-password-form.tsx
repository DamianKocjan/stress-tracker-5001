import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { useRequestPasswordResetMutation } from "@/hooks/use-request-password-reset-mutation";
import { cn } from "@/lib/utils";
import { ForgotPasswordFormSchema } from "@/schemas/auth";
import { useForm } from "@tanstack/react-form";
import { Link } from "@tanstack/react-router";
import { Lock } from "lucide-react";

export function ForgotPasswordForm({
  className,
  ...props
}: React.ComponentProps<"div">) {
  const requestPasswordResetMutation = useRequestPasswordResetMutation();

  const form = useForm({
    defaultValues: {
      email: "",
    },
    validators: {
      onSubmit: ForgotPasswordFormSchema,
    },
    async onSubmit({ value }) {
      await requestPasswordResetMutation.mutateAsync(value);
      form.reset();
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
            <FieldGroup>
              <div className="flex flex-col items-center gap-2 text-center">
                <div className="rounded-lg bg-muted p-2">
                  <Lock className="h-6 w-6" />
                </div>
                <h1 className="text-2xl font-bold">Forgot Password?</h1>
                <p className="text-muted-foreground text-balance">
                  Enter your email address and we'll send you a link to reset
                  your password
                </p>
              </div>

              <form.Field
                name="email"
                children={(field) => {
                  const isInvalid =
                    field.state.meta.isTouched && !field.state.meta.isValid;
                  return (
                    <Field data-invalid={isInvalid}>
                      <FieldLabel htmlFor="email">Email</FieldLabel>
                      <Input
                        id="email"
                        name="email"
                        type="email"
                        placeholder="m@example.com"
                        value={field.state.value}
                        onBlur={field.handleBlur}
                        onChange={(e) => field.handleChange(e.target.value)}
                        aria-invalid={isInvalid}
                      />
                      {isInvalid && (
                        <FieldError errors={field.state.meta.errors} />
                      )}
                    </Field>
                  );
                }}
              />

              <form.Subscribe selector={(s) => !s.canSubmit && s.isSubmitting}>
                {(disabled) => (
                  <Button type="submit" disabled={disabled} className="w-full">
                    {requestPasswordResetMutation.isPending
                      ? "Sending..."
                      : "Send Reset Link"}
                  </Button>
                )}
              </form.Subscribe>

              <div className="text-center text-sm">
                <span className="text-muted-foreground">
                  Remember your password?{" "}
                </span>
                <Link
                  to="/login"
                  search={{ redirect: "/" }}
                  className="text-primary hover:underline"
                >
                  Back to login
                </Link>
              </div>
            </FieldGroup>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
