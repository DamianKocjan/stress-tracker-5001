import type { ColumnDto } from "@/dto/column.dto";
import { useColumnDeleteMutation } from "@/hooks/use-column-delete-mutation";
import { useColumnUpdateMutation } from "@/hooks/use-column-update-mutation";
import { useMediaQuery } from "@/hooks/use-media-query";
import { cn } from "@/lib/utils";
import { ColumnFormSchema } from "@/schemas/column";
import { useKanbanStore } from "@/stores/kanban-store";
import { showErrorToast } from "@/utils/handle-error";
import { useForm } from "@tanstack/react-form";
import { Loader2, Pencil, X } from "lucide-react";
import { Button } from "../ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "../ui/dialog";
import {
  Drawer,
  DrawerClose,
  DrawerContent,
  DrawerDescription,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "../ui/drawer";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "../ui/field";
import { Input } from "../ui/input";
import { Tooltip, TooltipContent, TooltipTrigger } from "../ui/tooltip";

interface ColumnUpdateDialogProps {
  boardId: number;
  column: ColumnDto;
}

export function ColumnUpdateDialog({
  boardId,
  column,
}: ColumnUpdateDialogProps) {
  const isOpen = useKanbanStore((s) => s.isColumnUpdateDialogOpen);
  const setIsOpen = useKanbanStore((s) => s.setIsColumnUpdateDialogOpen);
  const columnId = useKanbanStore((s) => s.columnId);
  const setColumnId = useKanbanStore((s) => s.setColumnId);
  const isDesktop = useMediaQuery("(min-width: 768px)");

  // Only render if this column is selected
  if (columnId !== column.id) {
    return null;
  }

  function handleOpenChange(open: boolean) {
    setIsOpen(open);
    if (!open) {
      setColumnId(null);
    }
  }

  if (isDesktop) {
    return (
      <Dialog open={isOpen} onOpenChange={handleOpenChange}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Update Column</DialogTitle>
            <DialogDescription>
              Update the column by changing the details below.
            </DialogDescription>
          </DialogHeader>
          <ColumnForm
            boardId={boardId}
            column={column}
            close={() => handleOpenChange(false)}
          />
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Drawer open={isOpen} onOpenChange={handleOpenChange}>
      <DrawerContent>
        <DrawerHeader className="text-left">
          <DrawerTitle>Update Column</DrawerTitle>
          <DrawerDescription>
            Update the column by changing the details below.
          </DrawerDescription>
        </DrawerHeader>
        <ColumnForm
          className="px-4"
          boardId={boardId}
          column={column}
          close={() => handleOpenChange(false)}
        />
        <DrawerFooter className="pt-2">
          <DrawerClose asChild>
            <Button variant="outline">Cancel</Button>
          </DrawerClose>
        </DrawerFooter>
      </DrawerContent>
    </Drawer>
  );
}

interface ColumnUpdateDialogTriggerProps {
  columnId: number;
}

export function ColumnUpdateDialogTrigger({
  columnId,
}: ColumnUpdateDialogTriggerProps) {
  const setIsOpen = useKanbanStore((s) => s.setIsColumnUpdateDialogOpen);
  const setColumnId = useKanbanStore((s) => s.setColumnId);
  const isDesktop = useMediaQuery("(min-width: 768px)");

  function handleClick() {
    setColumnId(columnId);
    setIsOpen(true);
  }

  if (isDesktop) {
    return (
      <Tooltip>
        <TooltipTrigger asChild>
          <DialogTrigger asChild>
            <Button variant="ghost" size="icon" onClick={handleClick}>
              <Pencil className="size-4" />
            </Button>
          </DialogTrigger>
        </TooltipTrigger>
        <TooltipContent>
          <p>Edit column</p>
        </TooltipContent>
      </Tooltip>
    );
  }

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <DrawerTrigger asChild>
          <Button variant="ghost" size="icon" onClick={handleClick}>
            <Pencil className="size-4" />
          </Button>
        </DrawerTrigger>
      </TooltipTrigger>
      <TooltipContent>
        <p>Edit column</p>
      </TooltipContent>
    </Tooltip>
  );
}

interface ColumnFormProps {
  className?: string;
  boardId: number;
  column: ColumnDto;
  close: () => void;
}

function ColumnForm({ className, boardId, column, close }: ColumnFormProps) {
  const columnUpdateMutation = useColumnUpdateMutation(boardId, column.id);
  const columnDeleteMutation = useColumnDeleteMutation(
    boardId,
    column.id,
    column.name
  );

  const form = useForm({
    defaultValues: {
      name: column.name,
      wipLimit: column.wipLimit ?? 0,
    },
    validators: {
      onSubmit: ColumnFormSchema,
    },
    async onSubmit({ value }) {
      try {
        await columnUpdateMutation.mutateAsync({
          name: value.name,
          wipLimit: value.wipLimit === 0 ? null : value.wipLimit,
        });

        form.reset();
        close();
      } catch (error) {
        console.error(error);
        showErrorToast(error);
      }
    },
  });

  async function handleDelete() {
    try {
      await columnDeleteMutation.mutateAsync();
      close();
    } catch (error) {
      console.error(error);
      showErrorToast(error);
    }
  }

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
                  placeholder="E.g., To Do, In Progress, Done"
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
                  type="number"
                  min={0}
                  value={field.state.value}
                  onBlur={field.handleBlur}
                  onChange={(e) =>
                    field.handleChange(parseInt(e.target.value, 10) || 0)
                  }
                  aria-invalid={isInvalid}
                  placeholder="0"
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

      <div className="flex gap-2">
        <form.Subscribe selector={(s) => !s.canSubmit && s.isSubmitting}>
          {(isSubmitting) => (
            <Button type="submit" className="flex-1" disabled={isSubmitting}>
              {columnUpdateMutation.isPending ? "Updating..." : "Update Column"}
            </Button>
          )}
        </form.Subscribe>

        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              type="button"
              size="icon"
              variant="destructive"
              disabled={columnDeleteMutation.isPending}
              onClick={handleDelete}
            >
              {columnDeleteMutation.isPending ? (
                <Loader2 className="animate-spin size-4" />
              ) : (
                <X className="size-4" />
              )}
            </Button>
          </TooltipTrigger>
          <TooltipContent>
            <p>Delete this column</p>
          </TooltipContent>
        </Tooltip>
      </div>
    </form>
  );
}
