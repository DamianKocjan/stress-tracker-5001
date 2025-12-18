import { useJoinBoardMutation } from "@/hooks/use-join-board-mutation";
import { useMediaQuery } from "@/hooks/use-media-query";
import { cn } from "@/lib/utils";
import { JoinBoardSchema } from "@/schemas/invite";
import { useJoinBoardDialogStore } from "@/stores/join-board-dialog-store";
import { showErrorToast } from "@/utils/handle-error";
import { useForm } from "@tanstack/react-form";
import { redirect } from "@tanstack/react-router";
import { useState } from "react";
import { Button } from "../ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "../ui/dialog";
import {
  Drawer,
  DrawerClose,
  DrawerContent,
  DrawerDescription,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
} from "../ui/drawer";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "../ui/field";
import { Input } from "../ui/input";

export function JoinBoard() {
  const { isOpen, setIsOpen } = useJoinBoardDialogStore();
  const isDesktop = useMediaQuery("(min-width: 768px)");

  if (isDesktop) {
    return (
      <Dialog open={isOpen} onOpenChange={setIsOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Join Board</DialogTitle>
            <DialogDescription>
              Enter the invite token to join an existing board.
            </DialogDescription>
          </DialogHeader>
          <JoinBoardForm />
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Drawer open={isOpen} onOpenChange={setIsOpen}>
      <DrawerContent>
        <DrawerHeader className="text-left">
          <DrawerTitle>Join Board</DrawerTitle>
          <DrawerDescription>
            Enter the invite token to join an existing board.
          </DrawerDescription>
        </DrawerHeader>
        <JoinBoardForm className="px-4" />
        <DrawerFooter className="pt-2">
          <DrawerClose asChild>
            <Button variant="outline">Cancel</Button>
          </DrawerClose>
        </DrawerFooter>
      </DrawerContent>
    </Drawer>
  );
}

interface JoinBoardFormProps {
  className?: string;
}

function JoinBoardForm({ className }: JoinBoardFormProps) {
  const joinBoardMutation = useJoinBoardMutation();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const form = useForm({
    defaultValues: {
      token: "",
    },
    validators: {
      onSubmit: JoinBoardSchema,
    },
    async onSubmit({ value }) {
      setIsSubmitting(true);
      try {
        await joinBoardMutation.mutateAsync(value.token);

        form.reset();
        useJoinBoardDialogStore.getState().setIsOpen(false);

        // Redirect to the newly joined board
        redirect({
          to: "/board/$boardId",
          params: {
            boardId: joinBoardMutation.data!.id.toString(),
          },
        });
      } catch (error) {
        console.error(error);
        showErrorToast(error);
      } finally {
        setIsSubmitting(false);
      }
    },
  });

  return (
    <form
      className={cn("grid items-start gap-6", className)}
      onSubmit={(e) => {
        e.preventDefault();
        form.handleSubmit();
      }}
    >
      <FieldGroup>
        <form.Field
          name="token"
          children={(field) => {
            const isInvalid =
              field.state.meta.isTouched && !field.state.meta.isValid;
            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Invite Token</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  value={field.state.value}
                  onBlur={field.handleBlur}
                  onChange={(e) => field.handleChange(e.target.value)}
                  aria-invalid={isInvalid}
                  placeholder="Paste the invite token here"
                  autoComplete="off"
                  disabled={isSubmitting}
                />
                <FieldDescription>
                  You can get this token from someone who already has access to
                  the board.
                </FieldDescription>
                {isInvalid && <FieldError errors={field.state.meta.errors} />}
              </Field>
            );
          }}
        />
      </FieldGroup>

      <Button
        type="submit"
        disabled={isSubmitting || joinBoardMutation.isPending}
      >
        {joinBoardMutation.isPending ? "Joining..." : "Join Board"}
      </Button>
    </form>
  );
}
