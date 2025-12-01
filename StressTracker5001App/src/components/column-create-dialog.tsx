import { useColumnCreateMutation } from "@/hooks/use-column-create-mutation";
import { useMediaQuery } from "@/hooks/use-media-query";
import { cn } from "@/lib/utils";
import { ColumnFormSchema } from "@/schemas/column";
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

interface ColumnCreateDialogProps {
  boardId: number;
}

export function ColumnCreateDialog({ boardId }: ColumnCreateDialogProps) {
  const isOpen = useKanbanStore((s) => s.isColumnCreateDialogOpen);
  const setIsOpen = useKanbanStore((s) => s.setIsColumnCreateDialogOpen);
  const isDesktop = useMediaQuery("(min-width: 768px)");

  if (isDesktop) {
    return (
      <Dialog open={isOpen} onOpenChange={setIsOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Create Column</DialogTitle>
            <DialogDescription>
              Create a new column by filling out the details below.
            </DialogDescription>
          </DialogHeader>
          <ColumnForm boardId={boardId} />
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Drawer open={isOpen} onOpenChange={setIsOpen}>
      <DrawerContent>
        <DrawerHeader className="text-left">
          <DrawerTitle>Create Column</DrawerTitle>
          <DrawerDescription>
            Create a new column by filling out the details below.
          </DrawerDescription>
        </DrawerHeader>
        <ColumnForm className="px-4" boardId={boardId} />
        <DrawerFooter className="pt-2">
          <DrawerClose asChild>
            <Button variant="outline">Cancel</Button>
          </DrawerClose>
        </DrawerFooter>
      </DrawerContent>
    </Drawer>
  );
}

interface ColumnFormProps {
  className?: string;
  boardId: number;
}

function ColumnForm({ className, boardId }: ColumnFormProps) {
  const columnCreateMutation = useColumnCreateMutation(boardId);

  const form = useForm({
    defaultValues: {
      name: "",
      wipLimit: 0,
    },
    validators: {
      onSubmit: ColumnFormSchema,
    },
    async onSubmit({ value }) {
      try {
        await columnCreateMutation.mutateAsync({
          ...value,
          position: 0,
          wipLimit: value.wipLimit === 0 ? null : value.wipLimit,
        });

        form.reset();
        useKanbanStore.getState().setIsColumnCreateDialogOpen(false);
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
                  placeholder="E.g., To Do"
                  autoComplete="off"
                />
                {isInvalid && <FieldError errors={field.state.meta.errors} />}
              </Field>
            );
          }}
        />
        <form.Field
          name="wipLimit"
          children={(field) => {
            const isInvalid =
              field.state.meta.isTouched && !field.state.meta.isValid;
            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>WIP Limit</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  value={field.state.value}
                  onBlur={field.handleBlur}
                  onChange={(e) =>
                    field.handleChange(parseInt(e.target.value, 10) || 0)
                  }
                  aria-invalid={isInvalid}
                  placeholder="E.g., 5"
                  type="number"
                />
                <FieldDescription>
                  Work In Progress limit. Set to 0 for no limit.
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
            {columnCreateMutation.isPending ? "Creating..." : "Create Column"}
          </Button>
        )}
      </form.Subscribe>
    </form>
  );
}
