import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldSet,
} from "@/components/ui/field";
import { useDeleteAccountMutation } from "@/hooks/use-delete-account-mutation";
import { cn } from "@/lib/utils";
import { DeleteAccountFormSchema } from "@/schemas/auth";
import { useForm } from "@tanstack/react-form";
import { useState } from "react";
import { PasswordInput } from "../ui/input-password";

export function DangerZone({
  className,
  ...props
}: React.ComponentProps<"div">) {
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);
  const { mutate, isPending } = useDeleteAccountMutation();

  const form = useForm({
    defaultValues: {
      password: "",
    },
    validators: {
      onSubmit: DeleteAccountFormSchema,
    },
    async onSubmit({ value }) {
      mutate({ password: value.password, confirmDeletion: true });
    },
  });

  const handleDialogClose = (open: boolean) => {
    setShowConfirmDialog(open);
    if (!open) {
      form.reset();
    }
  };

  return (
    <div className={cn("space-y-6", className)} {...props}>
      <div className="rounded-lg border border-destructive/50 p-4 space-y-3">
        <div>
          <h3 className="font-semibold text-destructive">Delete Account</h3>
          <p className="text-sm text-muted-foreground mt-1">
            Permanently delete your account and all associated data. This action
            cannot be undone.
          </p>
        </div>
        <Button
          variant="destructive"
          onClick={() => setShowConfirmDialog(true)}
          className="w-full"
        >
          Delete My Account
        </Button>
      </div>

      <Dialog open={showConfirmDialog} onOpenChange={handleDialogClose}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Account</DialogTitle>
            <DialogDescription>
              This action is permanent. Your account and all data will be
              deleted.
            </DialogDescription>
          </DialogHeader>

          <form
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();
              form.handleSubmit();
            }}
          >
            <FieldSet>
              <FieldGroup>
                <div className="rounded-lg border border-destructive/50 p-3">
                  <p className="text-sm font-medium text-destructive">
                    This cannot be reversed.
                  </p>
                </div>

                <form.Field
                  name="password"
                  children={(field) => {
                    const isInvalid =
                      field.state.meta.isTouched && !field.state.meta.isValid;
                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor="delete-password">
                          Enter Your Password
                        </FieldLabel>
                        <PasswordInput
                          id="delete-password"
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
              </FieldGroup>

              <DialogFooter className="mt-6">
                <Button
                  variant="outline"
                  onClick={() => handleDialogClose(false)}
                  disabled={isPending}
                >
                  Cancel
                </Button>
                <form.Subscribe selector={(s) => [s.canSubmit, s.isSubmitting]}>
                  {([canSubmit, isSubmitting]) => (
                    <Button
                      type="submit"
                      variant="destructive"
                      disabled={!canSubmit || isSubmitting || isPending}
                    >
                      {isPending ? "Deleting..." : "Delete Account"}
                    </Button>
                  )}
                </form.Subscribe>
              </DialogFooter>
            </FieldSet>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
