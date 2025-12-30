import { ROLE } from "@/dto/board-member.dto";
import type { CardDetailsDto } from "@/dto/card.dto";
import { useCardAssignTagsMutation } from "@/hooks/use-card-assign-tags-mutation";
import { useCardDeleteMutation } from "@/hooks/use-card-delete-mutation";
import { useCardDetailsQuery } from "@/hooks/use-card-details-query";
import { useCardUpdateMutation } from "@/hooks/use-card-update-mutation";
import { useConfirm } from "@/hooks/use-confirm";
import { useMediaQuery } from "@/hooks/use-media-query";
import { useTagDeleteMutation } from "@/hooks/use-tag-delete-mutation";
import { useTagsQuery } from "@/hooks/use-tags-query";
import { useUserBoardRole } from "@/hooks/use-user-board-role";
import { cn } from "@/lib/utils";
import { CardFormSchema } from "@/schemas/card";
import { useKanbanStore } from "@/stores/kanban-store";
import { showErrorToast } from "@/utils/handle-error";
import { useForm } from "@tanstack/react-form";
import {
  Calendar,
  Clock,
  Pencil,
  Tag,
  Trash2,
  User,
  UsersIcon,
  X,
} from "lucide-react";
import { useState } from "react";
import { FetchingErrorAlert } from "../fetching-error-alert";
import { RoleGuard } from "../role-guard";
import { SelectedTagsDisplay, TagSelector } from "../tags/tag-selector";
import { Button } from "../ui/button";
import { Drawer, DrawerClose, DrawerContent, DrawerFooter } from "../ui/drawer";
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
import { ScrollArea } from "../ui/scroll-area";
import { Separator } from "../ui/separator";
import { Skeleton } from "../ui/skeleton";
import { AssignedUsersDisplay } from "./card-assigned-users-display";
import { CardAttachments } from "./card-attachments";
import { CardComments } from "./card-comments";

interface CardDetailsDialogProps {
  boardId: number;
}

export function CardDetailsDialog({ boardId }: CardDetailsDialogProps) {
  const cardId = useKanbanStore((s) => s.cardId);
  const isDesktop = useMediaQuery("(min-width: 768px)");

  function handleOpenChange(open: boolean) {
    if (!open) {
      useKanbanStore.getState().setCardId(null);
    }
  }

  return (
    <Drawer
      direction={isDesktop ? "right" : undefined}
      open={cardId !== null}
      onOpenChange={handleOpenChange}
    >
      <DrawerContent className="data-[vaul-drawer-direction=right]:lg:max-w-1/2 data-[vaul-drawer-direction=right]:xl:max-w-2/5">
        <ScrollArea className="flex-1 overflow-y-hidden p-4 pb-0">
          {cardId !== null && (
            <CardDetailsContent boardId={boardId} cardId={cardId} />
          )}
        </ScrollArea>
        <DrawerFooter className="mt-auto pt-2">
          <DrawerClose asChild>
            <Button variant="outline">Close</Button>
          </DrawerClose>
        </DrawerFooter>
      </DrawerContent>
    </Drawer>
  );
}

interface CardDetailsContentProps {
  className?: string;
  boardId: number;
  cardId: number;
}

function CardDetailsContent({
  className,
  boardId,
  cardId,
}: CardDetailsContentProps) {
  const { data, status, error, refetch } = useCardDetailsQuery(cardId);
  const [isEditing, setIsEditing] = useState(false);
  const userRole = useUserBoardRole();

  if (status === "pending") {
    return <CardDetailsSkeleton className={className} />;
  }

  if (status === "error") {
    return (
      <FetchingErrorAlert
        title="Failed to load card details"
        error={error}
        refetch={refetch}
        className={className}
      />
    );
  }

  if (isEditing && userRole !== ROLE.Viewer) {
    return (
      <CardEditForm
        className={className}
        boardId={boardId}
        cardId={cardId}
        defaultValues={{
          title: data.title,
          description: data.description,
          dueDate: data.dueDate ?? "",
        }}
        onCancel={() => setIsEditing(false)}
        onSuccess={() => setIsEditing(false)}
      />
    );
  }

  return (
    <CardDetailsView
      className={className}
      boardId={boardId}
      {...data}
      onEdit={() => setIsEditing(true)}
    />
  );
}

function CardDetailsSkeleton({ className }: { className?: string }) {
  return (
    <div className={cn("space-y-4", className)}>
      <Skeleton className="h-6 w-3/4" />
      <Skeleton className="h-20 w-full" />
      <Separator />
      <div className="space-y-2">
        <Skeleton className="h-4 w-1/2" />
        <Skeleton className="h-4 w-1/3" />
        <Skeleton className="h-4 w-2/5" />
      </div>
    </div>
  );
}

interface CardDetailsViewProps extends CardDetailsDto {
  className?: string;
  boardId: number;
  onEdit: () => void;
}

function CardDetailsView({
  className,
  boardId,
  id,
  title,
  description,
  dueDate,
  createdBy,
  tags: cardTags,
  assignments,
  attachments,
  createdAt,
  updatedAt,
  onEdit,
}: CardDetailsViewProps) {
  const [ConfirmDialog, confirm] = useConfirm({
    title: "Delete Card",
    message: `Are you sure you want to delete "${title}"? This action cannot be undone.`,
  });
  const cardDeleteMutation = useCardDeleteMutation(boardId, title);
  const assignTagMutation = useCardAssignTagsMutation(boardId);
  const removeTagMutation = useTagDeleteMutation(boardId);
  const { data: tags = [] } = useTagsQuery();

  async function handleDelete() {
    const confirmed = await confirm();
    if (confirmed) {
      try {
        await cardDeleteMutation.mutateAsync(id);
        useKanbanStore.getState().setCardId(null);
      } catch (error) {
        showErrorToast(error);
      }
    }
  }

  return (
    <div className={cn("space-y-4", className)}>
      <ConfirmDialog />

      <div className="flex items-start justify-between gap-2">
        <h3 className="text-lg font-semibold leading-tight break-all">
          {title}
        </h3>

        <RoleGuard minRole="Member">
          <div className="flex gap-1">
            <Button variant="ghost" size="icon" onClick={onEdit}>
              <Pencil className="size-4" />
              <span className="sr-only">Edit</span>
            </Button>
            <Button
              variant="ghost"
              size="icon"
              onClick={handleDelete}
              disabled={cardDeleteMutation.isPending}
            >
              <Trash2 className="size-4 text-destructive" />
              <span className="sr-only">Delete</span>
            </Button>
          </div>
        </RoleGuard>
      </div>

      {description ? (
        <p className="text-muted-foreground whitespace-pre-wrap break-all">
          {description}
        </p>
      ) : (
        <p className="text-muted-foreground italic">No description provided.</p>
      )}

      <Separator />

      <div className="space-y-2">
        <div className="flex items-center gap-2">
          <Tag className="text-muted-foreground size-4" />
          <span className="text-sm text-muted-foreground">Tags:</span>
        </div>

        <RoleGuard
          minRole="Member"
          fallback={
            <SelectedTagsDisplay
              selectedTags={tags.filter((tag) => cardTags.includes(tag.id))}
            />
          }
        >
          <TagSelector
            availableTags={tags}
            selectedTags={tags.filter((tag) => cardTags.includes(tag.id))}
            onClose={(tags) =>
              assignTagMutation.mutateAsync({ tags, cardId: id })
            }
            maxTags={5}
            disabled={
              assignTagMutation.isPending || removeTagMutation.isPending
            }
          />
        </RoleGuard>
      </div>

      <Separator />

      <div className="space-y-2">
        <div className="flex items-center gap-2">
          <UsersIcon className="text-muted-foreground size-4" />
          <span className="text-sm text-muted-foreground">Assignees:</span>
        </div>

        <AssignedUsersDisplay
          assignments={assignments}
          boardId={boardId}
          cardId={id}
        />
      </div>

      <Separator />

      <div className="space-y-2 text-sm">
        <div className="flex items-center gap-2">
          <Calendar className="text-muted-foreground size-4" />
          <span className="text-muted-foreground">Due date:</span>
          {dueDate ? (
            <span>
              {formatDate(dueDate)} {formatRelativeDate(dueDate)}
            </span>
          ) : (
            <span className="text-muted-foreground italic">Not set</span>
          )}
        </div>

        <div className="flex items-center gap-2">
          <User className="text-muted-foreground size-4" />
          <span className="text-muted-foreground">Created by:</span>
          <span>{createdBy.username}</span>
        </div>

        <div className="flex items-center gap-2">
          <Clock className="text-muted-foreground size-4" />
          <span className="text-muted-foreground">Created:</span>
          <span>{formatDateTime(createdAt)}</span>
        </div>

        <div className="flex items-center gap-2">
          <Clock className="text-muted-foreground size-4" />
          <span className="text-muted-foreground">Updated:</span>
          <span>{formatDateTime(updatedAt)}</span>
        </div>
      </div>

      <Separator />

      <CardAttachments cardId={id} attachments={attachments} />

      <Separator />

      <CardComments cardId={id} />
    </div>
  );
}

interface CardEditFormProps {
  className?: string;
  boardId: number;
  cardId: number;
  defaultValues: {
    title: string;
    description: string;
    dueDate: string;
  };
  onCancel: () => void;
  onSuccess: () => void;
}

function CardEditForm({
  className,
  boardId,
  cardId,
  defaultValues,
  onCancel,
  onSuccess,
}: CardEditFormProps) {
  const [ConfirmDialog, confirm] = useConfirm({
    title: "Discard Changes",
    message: "You have unsaved changes. Are you sure you want to discard them?",
  });
  const cardUpdateMutation = useCardUpdateMutation(boardId, cardId);

  const form = useForm({
    defaultValues,
    validators: {
      onSubmit: CardFormSchema,
    },
    async onSubmit({ value }) {
      try {
        await cardUpdateMutation.mutateAsync({
          ...value,
          dueDate: value.dueDate === "" ? null : value.dueDate,
        });
        onSuccess();
      } catch (error) {
        console.error(error);
        showErrorToast(error);
      }
    },
  });

  async function handleCancel() {
    if (form.state.isDirty) {
      const confirmed = await confirm();
      if (!confirmed) {
        return;
      }
    }
    form.reset();
    onCancel();
  }

  return (
    <>
      <ConfirmDialog />
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
          <form.Field
            name="dueDate"
            children={(field) => {
              const isInvalid =
                field.state.meta.isTouched && !field.state.meta.isValid;
              return (
                <Field data-invalid={isInvalid}>
                  <FieldLabel htmlFor={field.name}>Due Date</FieldLabel>
                  <Input
                    id={field.name}
                    name={field.name}
                    type="datetime-local"
                    value={
                      field.state.value
                        ? formatDateTimeLocalValue(field.state.value)
                        : ""
                    }
                    onBlur={field.handleBlur}
                    onChange={(e) => {
                      const value = e.target.value;
                      if (value) {
                        field.handleChange(new Date(value).toISOString());
                      } else {
                        field.handleChange("");
                      }
                    }}
                    aria-invalid={isInvalid}
                  />
                  <FieldDescription>
                    Optional due date for this card.
                  </FieldDescription>
                  {isInvalid && <FieldError errors={field.state.meta.errors} />}
                </Field>
              );
            }}
          />
        </FieldGroup>

        <div className="flex gap-2">
          <Button
            type="button"
            variant="outline"
            onClick={handleCancel}
            className="flex-1"
          >
            <X className="mr-2 size-4" />
            Cancel
          </Button>
          <form.Subscribe selector={(s) => !s.canSubmit || s.isSubmitting}>
            {(isDisabled) => (
              <Button type="submit" disabled={isDisabled} className="flex-1">
                {cardUpdateMutation.isPending ? "Saving..." : "Save Changes"}
              </Button>
            )}
          </form.Subscribe>
        </div>
      </form>
    </>
  );
}

// Date formatting utilities using native Intl
function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  }).format(date);
}

function formatDateTime(dateString: string): string {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(date);
}

function formatRelativeDate(dateString: string): string {
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = date.getTime() - now.getTime();
  const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24));

  if (diffDays < 0) {
    const absDays = Math.abs(diffDays);
    if (absDays === 1) return "(overdue by 1 day)";
    return `(overdue by ${absDays} days)`;
  }
  if (diffDays === 0) return "(due today)";
  if (diffDays === 1) return "(due tomorrow)";
  if (diffDays <= 7) return `(in ${diffDays} days)`;
  return "";
}

function formatDateTimeLocalValue(isoString: string): string {
  const date = new Date(isoString);
  // Format as YYYY-MM-DDTHH:mm for datetime-local input
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  return `${year}-${month}-${day}T${hours}:${minutes}`;
}
