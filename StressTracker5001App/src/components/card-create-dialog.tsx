import { useCardCreateMutation } from "@/hooks/use-card-create-mutation";
import { useMediaQuery } from "@/hooks/use-media-query";
import { cn } from "@/lib/utils";
import { CardFormSchema } from "@/schemas/card";
import { useKanbanStore } from "@/stores/kanban-store";
import { showErrorToast } from "@/utils/handle-error";
import { useForm } from "@tanstack/react-form";
import { Button } from "./ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "./ui/dialog";
import {
  Drawer,
  DrawerClose,
  DrawerContent,
  DrawerDescription,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
} from "./ui/drawer";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "./ui/field";
import { Input } from "./ui/input";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupText,
  InputGroupTextarea,
} from "./ui/input-group";

interface CardCreateDialogProps {
  boardId: number;
}

export function CardCreateDialog({ boardId }: CardCreateDialogProps) {
  const isOpen = useKanbanStore((s) => s.isCardDialogCreateDialogOpen);
  const setIsOpen = useKanbanStore((s) => s.setIsCardDialogCreateDialogOpen);
  const columnId = useKanbanStore((s) => s.columnId);
  const isDesktop = useMediaQuery("(min-width: 768px)");

  if (columnId === null) {
    return null;
  }

  if (isDesktop) {
    return (
      <Dialog open={isOpen} onOpenChange={setIsOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Create card</DialogTitle>
            <DialogDescription>
              Create a new card by filling out the details below.
            </DialogDescription>
          </DialogHeader>
          <CardForm boardId={boardId} columnId={columnId} />
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Drawer open={isOpen} onOpenChange={setIsOpen}>
      <DrawerContent>
        <DrawerHeader className="text-left">
          <DrawerTitle>Create card</DrawerTitle>
          <DrawerDescription>
            Create a new card by filling out the details below.
          </DrawerDescription>
        </DrawerHeader>
        <CardForm className="px-4" boardId={boardId} columnId={columnId} />
        <DrawerFooter className="pt-2">
          <DrawerClose asChild>
            <Button variant="outline">Cancel</Button>
          </DrawerClose>
        </DrawerFooter>
      </DrawerContent>
    </Drawer>
  );
}

interface CardFormProps {
  className?: string;
  boardId: number;
  columnId: number;
}

function CardForm({ className, boardId, columnId }: CardFormProps) {
  const cardCreateMutation = useCardCreateMutation(boardId, columnId);

  const form = useForm({
    defaultValues: {
      title: "",
      description: "",
      dueDate: "",
    },
    validators: {
      onSubmit: CardFormSchema,
    },
    async onSubmit({ value }) {
      try {
        await cardCreateMutation.mutateAsync({
          ...value,
          dueDate: value.dueDate === "" ? null : value.dueDate,
        });

        form.reset();
        useKanbanStore.getState().setIsCardDialogCreateDialogOpen(false);
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
          name="title"
          children={(field) => {
            const isInvalid =
              field.state.meta.isTouched && !field.state.meta.isValid;
            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Title</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  value={field.state.value}
                  onBlur={field.handleBlur}
                  onChange={(e) => field.handleChange(e.target.value)}
                  aria-invalid={isInvalid}
                  placeholder="E.g., Implement authentication"
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
                    placeholder="E.g., Implement user authentication using JWT."
                    rows={6}
                    className="min-h-24 resize-none"
                    aria-invalid={isInvalid}
                  />
                  <InputGroupAddon align="block-end">
                    <InputGroupText className="tabular-nums">
                      {field.state.value.length}/1000 characters
                    </InputGroupText>
                  </InputGroupAddon>
                </InputGroup>
                <FieldDescription>
                  A brief description of the card (max 1000 characters).
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
            {cardCreateMutation.isPending ? "Creating..." : "Create card"}
          </Button>
        )}
      </form.Subscribe>
    </form>
  );
}
