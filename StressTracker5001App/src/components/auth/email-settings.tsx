import { Button } from "@/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldSet,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { useRequestEmailChangeMutation } from "@/hooks/use-request-email-change-mutation";
import { cn } from "@/lib/utils";
import { RequestEmailChangeFormSchema } from "@/schemas/auth";
import { useForm } from "@tanstack/react-form";
import { PasswordInput } from "../ui/input-password";

export function EmailSettings({
  className,
  ...props
}: React.ComponentProps<"div">) {
  const requestEmailChangeMutation = useRequestEmailChangeMutation();

  const form = useForm({
    defaultValues: {
      newEmail: "",
      password: "",
    },
    validators: {
      onSubmit: RequestEmailChangeFormSchema,
    },
    async onSubmit({ value }) {
      await requestEmailChangeMutation.mutateAsync(value);
      form.reset();
    },
  });

  return (
    <div className={cn("space-y-6", className)} {...props}>
      {form.state.isSubmitted ? (
        <div className="rounded-lg border p-4 space-y-3">
          <h3 className="font-semibold">Verification email sent</h3>
          <p className="text-sm text-muted-foreground">
            Check your new email address for a verification link. Click it to
            confirm the change.
          </p>
        </div>
      ) : (
        <form
          onSubmit={(e) => {
            e.preventDefault();
            e.stopPropagation();
            form.handleSubmit();
          }}
        >
          <FieldSet>
            <FieldGroup>
              <div>
                <h3 className="font-semibold">Change Email</h3>
                <p className="text-sm text-muted-foreground">
                  Update your email address
                </p>
              </div>
              <form.Field
                name="newEmail"
                children={(field) => {
                  const isInvalid =
                    field.state.meta.isTouched && !field.state.meta.isValid;
                  return (
                    <Field data-invalid={isInvalid}>
                      <FieldLabel htmlFor="new-email">
                        New Email Address
                      </FieldLabel>
                      <Input
                        id="new-email"
                        type="email"
                        placeholder="your.email@example.com"
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
              <form.Field
                name="password"
                children={(field) => {
                  const isInvalid =
                    field.state.meta.isTouched && !field.state.meta.isValid;
                  return (
                    <Field data-invalid={isInvalid}>
                      <FieldLabel htmlFor="password">Password</FieldLabel>
                      <PasswordInput
                        id="password"
                        placeholder="Confirm your password"
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
                    {disabled ? "Sending..." : "Request Email Change"}
                  </Button>
                )}
              </form.Subscribe>
            </FieldGroup>
          </FieldSet>
        </form>
      )}
    </div>
  );
}
