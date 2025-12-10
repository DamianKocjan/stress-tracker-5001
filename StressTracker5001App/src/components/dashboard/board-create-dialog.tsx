import { useBoardCreateMutation } from "@/hooks/use-board-create-mutation";
import { useMediaQuery } from "@/hooks/use-media-query";
import { cn } from "@/lib/utils";
import { BoardFormSchema } from "@/schemas/board";
import { useBoardCreateDialogStore } from "@/stores/board-create-dialog-store";
import { showErrorToast } from "@/utils/handle-error";
import { useForm } from "@tanstack/react-form";
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
import {
  InputGroup,
  InputGroupAddon,
  InputGroupText,
  InputGroupTextarea,
} from "../ui/input-group";

export function BoardCreateDialog() {
  const { isOpen, setIsOpen } = useBoardCreateDialogStore();
  const isDesktop = useMediaQuery("(min-width: 768px)");

  if (isDesktop) {
    return (
      <Dialog open={isOpen} onOpenChange={setIsOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Create Board</DialogTitle>
            <DialogDescription>
              Create a new board by filling out the details below.
            </DialogDescription>
          </DialogHeader>
          <BoardForm />
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Drawer open={isOpen} onOpenChange={setIsOpen}>
      <DrawerContent>
        <DrawerHeader className="text-left">
          <DrawerTitle>Create Board</DrawerTitle>
          <DrawerDescription>
            Create a new board by filling out the details below.
          </DrawerDescription>
        </DrawerHeader>
        <BoardForm className="px-4" />
        <DrawerFooter className="pt-2">
          <DrawerClose asChild>
            <Button variant="outline">Cancel</Button>
          </DrawerClose>
        </DrawerFooter>
      </DrawerContent>
    </Drawer>
  );
}

function BoardForm({ className }: { className?: string }) {
  const boardCreateMutation = useBoardCreateMutation();

  const form = useForm({
    defaultValues: {
      name: "",
      description: "",
    },
    validators: {
      onSubmit: BoardFormSchema,
    },
    async onSubmit({ value }) {
      try {
        await boardCreateMutation.mutateAsync(value);

        form.reset();
        useBoardCreateDialogStore.getState().setIsOpen(false);
      } catch (error) {
        console.error(error);
        showErrorToast(error);
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
          name="name"
          children={(field) => {
            const isInvalid =
              field.state.meta.isTouched && !field.state.meta.isValid;
            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Name</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  value={field.state.value}
                  onBlur={field.handleBlur}
                  onChange={(e) => field.handleChange(e.target.value)}
                  aria-invalid={isInvalid}
                  placeholder="E.g., Stress Tracking Board"
                  autoComplete="off"
                />
                {isInvalid && <FieldError errors={field.state.meta.errors} />}
              </Field>
            );
          }}
        />
        <form.Field
          name="description"
          children={(field) => {
            const isInvalid =
              field.state.meta.isTouched && !field.state.meta.isValid;
            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Description</FieldLabel>
                <InputGroup>
                  <InputGroupTextarea
                    id={field.name}
                    name={field.name}
                    value={field.state.value}
                    onBlur={field.handleBlur}
                    onChange={(e) => field.handleChange(e.target.value)}
                    placeholder="Describe the purpose of this board"
                    rows={6}
                    className="min-h-24 resize-none"
                    aria-invalid={isInvalid}
                  />
                  <InputGroupAddon align="block-end">
                    <InputGroupText className="tabular-nums">
                      {field.state.value.length}/500 characters
                    </InputGroupText>
                  </InputGroupAddon>
                </InputGroup>
                <FieldDescription>
                  Optional - provide a brief description of the board.
                </FieldDescription>
                {isInvalid && <FieldError errors={field.state.meta.errors} />}
              </Field>
            );
          }}
        />
      </FieldGroup>

      <form.Subscribe selector={(s) => !s.canSubmit && s.isSubmitting}>
        {(isSubmitting) => (
          <Button type="submit" disabled={isSubmitting}>
            {boardCreateMutation.isPending ? "Creating..." : "Create Board"}
          </Button>
        )}
      </form.Subscribe>
    </form>
  );
}
