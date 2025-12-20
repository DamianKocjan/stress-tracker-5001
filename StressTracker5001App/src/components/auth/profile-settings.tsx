import { Button } from "@/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldSet,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { useUpdatePasswordMutation } from "@/hooks/use-update-password-mutation";
import { useUpdateProfileMutation } from "@/hooks/use-update-profile-mutation";
import { cn } from "@/lib/utils";
import { useAuth } from "@/providers/auth";
import {
  UpdatePasswordFormSchema,
  UpdateUsernameFormSchema,
} from "@/schemas/auth";
import { useForm } from "@tanstack/react-form";
import { PasswordInput } from "../ui/input-password";

export function ProfileSettings({
  className,
  ...props
}: React.ComponentProps<"div">) {
  const { user } = useAuth();
  const { mutate: updateUsername, isPending: isUpdatePending } =
    useUpdateProfileMutation();
  const { mutate: updatePassword, isPending: isPasswordPending } =
    useUpdatePasswordMutation();

  // Username form
  const usernameForm = useForm({
    defaultValues: {
      username: user?.username || "",
    },
    validators: {
      onSubmit: UpdateUsernameFormSchema,
    },
    async onSubmit({ value }) {
      updateUsername(value, {
        onSuccess: () => {
          usernameForm.reset();
        },
      });
    },
  });

  // Password form
  const passwordForm = useForm({
    defaultValues: {
      currentPassword: "",
      newPassword: "",
      confirmPassword: "",
    },
    validators: {
      onSubmit: UpdatePasswordFormSchema,
    },
    async onSubmit({ value }) {
      updatePassword(value, {
        onSuccess: () => {
          passwordForm.reset();
        },
      });
    },
  });

  return (
    <div className={cn("space-y-8", className)} {...props}>
      {/* Username Update Form */}
      <div className="space-y-4">
        <div>
          <h3 className="font-semibold">Username</h3>
          <p className="text-sm text-muted-foreground">How others see you</p>
        </div>

        <form
          onSubmit={(e) => {
            e.preventDefault();
            e.stopPropagation();
            usernameForm.handleSubmit();
          }}
        >
          <FieldSet>
            <FieldGroup>
              <usernameForm.Field
                name="username"
                children={(field) => {
                  const isInvalid =
                    field.state.meta.isTouched && !field.state.meta.isValid;
                  return (
                    <Field data-invalid={isInvalid}>
                      <FieldLabel htmlFor="username">New Username</FieldLabel>
                      <Input
                        id="username"
                        type="text"
                        value={field.state.value}
                        onBlur={field.handleBlur}
                        onChange={(e) => field.handleChange(e.target.value)}
                        aria-invalid={isInvalid}
                        disabled={isUpdatePending}
                      />
                      {isInvalid && (
                        <FieldError errors={field.state.meta.errors} />
                      )}
                    </Field>
                  );
                }}
              />
              <usernameForm.Subscribe
                selector={(s) => [s.canSubmit, s.isSubmitting]}
              >
                {([canSubmit, isSubmitting]) => (
                  <Button
                    type="submit"
                    disabled={!canSubmit || isSubmitting || isUpdatePending}
                    className="w-full"
                  >
                    {isUpdatePending ? "Saving..." : "Save Username"}
                  </Button>
                )}
              </usernameForm.Subscribe>
            </FieldGroup>
          </FieldSet>
        </form>
      </div>

      {/* Password Update Form */}
      <div className="border-t pt-8 space-y-4">
        <div>
          <h3 className="font-semibold">Password</h3>
          <p className="text-sm text-muted-foreground">Change your password</p>
        </div>

        <form
          onSubmit={(e) => {
            e.preventDefault();
            e.stopPropagation();
            passwordForm.handleSubmit();
          }}
        >
          <FieldSet>
            <FieldGroup>
              <passwordForm.Field
                name="currentPassword"
                children={(field) => {
                  const isInvalid =
                    field.state.meta.isTouched && !field.state.meta.isValid;
                  return (
                    <Field data-invalid={isInvalid}>
                      <FieldLabel htmlFor="current-password">
                        Current Password
                      </FieldLabel>
                      <PasswordInput
                        id="current-password"
                        placeholder="Enter current password"
                        value={field.state.value}
                        onBlur={field.handleBlur}
                        onChange={(e) => field.handleChange(e.target.value)}
                        aria-invalid={isInvalid}
                        disabled={isPasswordPending}
                      />
                      {isInvalid && (
                        <FieldError errors={field.state.meta.errors} />
                      )}
                    </Field>
                  );
                }}
              />

              <passwordForm.Field
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
                        placeholder="At least 8 characters"
                        value={field.state.value}
                        onBlur={field.handleBlur}
                        onChange={(e) => field.handleChange(e.target.value)}
                        aria-invalid={isInvalid}
                        disabled={isPasswordPending}
                      />
                      {isInvalid && (
                        <FieldError errors={field.state.meta.errors} />
                      )}
                    </Field>
                  );
                }}
              />

              <passwordForm.Field
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
                        placeholder="Confirm new password"
                        value={field.state.value}
                        onBlur={field.handleBlur}
                        onChange={(e) => field.handleChange(e.target.value)}
                        aria-invalid={isInvalid}
                        disabled={isPasswordPending}
                      />
                      {isInvalid && (
                        <FieldError errors={field.state.meta.errors} />
                      )}
                    </Field>
                  );
                }}
              />

              <passwordForm.Subscribe
                selector={(s) => [s.canSubmit, s.isSubmitting]}
              >
                {([canSubmit, isSubmitting]) => (
                  <Button
                    type="submit"
                    disabled={!canSubmit || isSubmitting || isPasswordPending}
                    className="w-full"
                  >
                    {isPasswordPending ? "Updating..." : "Update Password"}
                  </Button>
                )}
              </passwordForm.Subscribe>
            </FieldGroup>
          </FieldSet>
        </form>
      </div>
    </div>
  );
}
