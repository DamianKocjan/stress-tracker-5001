import type { BoardDto } from "@/dto/board.dto";
import { useBoardDeleteMutation } from "@/hooks/use-board-delete-mutation";
import { useBoardUpdateMutation } from "@/hooks/use-board-update-mutation";
import { useMediaQuery } from "@/hooks/use-media-query";
import { cn } from "@/lib/utils";
import { BoardFormSchema } from "@/schemas/board";
import { showErrorToast } from "@/utils/handle-error";
import { useForm } from "@tanstack/react-form";
import { useNavigate, useParams } from "@tanstack/react-router";
import { Loader2, Pencil, X } from "lucide-react";
import { useState } from "react";
import { Button } from "./ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "./ui/dialog";
import {
  Drawer,
  DrawerClose,
  DrawerContent,
  DrawerDescription,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
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
import { Tooltip, TooltipContent, TooltipTrigger } from "./ui/tooltip";

type BoardUpdateDialogProps = {
  defaultValues: Pick<BoardDto, "name" | "description">;
};

export function BoardUpdateDialog({ defaultValues }: BoardUpdateDialogProps) {
  const [isOpen, setIsOpen] = useState(false);
  const isDesktop = useMediaQuery("(min-width: 768px)");

  if (isDesktop) {
    return (
      <Dialog open={isOpen} onOpenChange={setIsOpen}>
        <Tooltip>
          <TooltipTrigger asChild>
            <DialogTrigger asChild>
              <Button variant="outline" size="icon">
                <Pencil className="size-4" />
              </Button>
            </DialogTrigger>
          </TooltipTrigger>
          <TooltipContent>
            <p>Edit board details</p>
          </TooltipContent>
        </Tooltip>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Update Board</DialogTitle>
            <DialogDescription>
              Update the board by changing the details below.
            </DialogDescription>
          </DialogHeader>
          <BoardForm
            defaultValues={defaultValues}
            close={() => setIsOpen(false)}
          />
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Drawer open={isOpen} onOpenChange={setIsOpen}>
      <Tooltip>
        <TooltipTrigger asChild>
          <DrawerTrigger asChild>
            <Button variant="outline" size="icon">
              <Pencil className="size-4" />
            </Button>
          </DrawerTrigger>
        </TooltipTrigger>
        <TooltipContent>
          <p>Edit board details</p>
        </TooltipContent>
      </Tooltip>
      <DrawerContent>
        <DrawerHeader className="text-left">
          <DrawerTitle>Update Board</DrawerTitle>
          <DrawerDescription>
            Update the board by changing the details below.
          </DrawerDescription>
        </DrawerHeader>
        <BoardForm
          className="px-4"
          defaultValues={defaultValues}
          close={() => setIsOpen(false)}
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

interface BoardFormProps {
  className?: string;
  defaultValues: Pick<BoardDto, "name" | "description">;
  close: () => void;
}

function BoardForm({ className, defaultValues, close }: BoardFormProps) {
  const { boardId } = useParams({ from: "/_authenticated/board/$boardId" });
  const navigate = useNavigate();
  const boardUpdateMutation = useBoardUpdateMutation(Number(boardId));
  const boardDeleteMutation = useBoardDeleteMutation(
    Number(boardId),
    defaultValues.name
  );

  const form = useForm({
    defaultValues: {
      name: defaultValues.name,
      description: defaultValues.description || "",
    },
    validators: {
      onSubmit: BoardFormSchema,
    },
    async onSubmit({ value }) {
      try {
        await boardUpdateMutation.mutateAsync(value);

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
      await boardDeleteMutation.mutateAsync();
      close();
      navigate({ to: "/dashboard" });
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

      <div className="flex gap-2">
        <form.Subscribe selector={(s) => !s.canSubmit && s.isSubmitting}>
          {(isSubmitting) => (
            <Button type="submit" className="flex-1" disabled={isSubmitting}>
              {boardUpdateMutation.isPending ? "Updating..." : "Update Board"}
            </Button>
          )}
        </form.Subscribe>

        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              type="button"
              size="icon"
              variant="destructive"
              disabled={boardDeleteMutation.isPending}
              onClick={handleDelete}
            >
              {boardDeleteMutation.isPending ? (
                <Loader2 className="animate-spin size-4" />
              ) : (
                <X className="size-4" />
              )}
            </Button>
          </TooltipTrigger>
          <TooltipContent>
            <p>Delete this board</p>
          </TooltipContent>
        </Tooltip>
      </div>
    </form>
  );
}
