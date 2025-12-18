import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { ROLE } from "@/dto/board-member.dto";
import type { CommentDto } from "@/dto/comment.dto";
import type { UserDto } from "@/dto/user.dto";
import { useCommentCreateMutation } from "@/hooks/use-comment-create-mutation";
import { useCommentQuery } from "@/hooks/use-comment-query";
import { useCommentUpdateMutation } from "@/hooks/use-comment-update-mutation";
import { useUserBoardRole } from "@/hooks/use-user-board-role";
import { cn } from "@/lib/utils";
import { CommentSchema } from "@/schemas/comment";
import { useForm } from "@tanstack/react-form";
import { CheckIcon, Edit2Icon, TrashIcon, XIcon } from "lucide-react";
import { useEffect, useState } from "react";
import { Textarea } from "../ui/textarea";
import { CommentInput } from "./comment-input";

type CommentLayoutProps = React.ComponentProps<"div"> & {
  comment: CommentDto;
  onDelete: (id: number) => void;
  onEdit?: () => void;
  currentUser: UserDto;
  isEditing?: boolean;
};

function CommentLayout({
  onDelete,
  onEdit,
  comment,
  currentUser,
  isEditing,
  className,
  children,
  ...props
}: CommentLayoutProps) {
  const isOwnComment = currentUser.id === comment.user.id;

  return (
    <div
      className={cn(
        "group flex gap-3 rounded-lg border bg-card px-4 py-3 transition-all hover:shadow-sm",
        isEditing && "ring-2 ring-primary/20",
        className
      )}
      {...props}
    >
      <Avatar className="size-9">
        <AvatarFallback className="text-xs">
          {getInitials(comment.user.username)}
        </AvatarFallback>
      </Avatar>
      <div className="flex-1 min-w-0">
        <div className="flex items-start justify-between gap-2">
          <div className="flex flex-wrap items-center gap-x-2 gap-y-1 text-sm">
            <p className="font-semibold text-foreground">
              {comment.user.username}
            </p>
            <div className="bg-muted size-1 rounded-full" />
            <p
              className="text-muted-foreground text-xs"
              title={new Date(comment.createdAt).toLocaleString()}
            >
              {getTimeAgo(new Date(comment.createdAt))}
            </p>
            {comment.updatedAt && comment.updatedAt !== comment.createdAt && (
              <span className="text-muted-foreground text-xs italic">
                (edited)
              </span>
            )}
          </div>
          {isOwnComment && !isEditing && (
            <div className="flex gap-1 opacity-0 transition-opacity group-hover:opacity-100">
              {onEdit && (
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-7 w-7 text-muted-foreground hover:text-foreground"
                  type="button"
                  onClick={onEdit}
                >
                  <span className="sr-only">Edit comment</span>
                  <Edit2Icon className="size-3.5" />
                </Button>
              )}
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7 text-muted-foreground hover:text-destructive"
                type="button"
                onClick={() => onDelete(comment.id)}
              >
                <span className="sr-only">Delete comment</span>
                <TrashIcon className="size-3.5" />
              </Button>
            </div>
          )}
        </div>
        {children}
      </div>
    </div>
  );
}

type CommentCardProps = CommentLayoutProps & {
  comment: CommentDto;
  cardId: number;
};

export function CommentCard({ comment, cardId, ...props }: CommentCardProps) {
  const [isEditing, setIsEditing] = useState(false);
  const updateCommentMutation = useCommentUpdateMutation(cardId);
  const userRole = useUserBoardRole();
  const canEdit = userRole !== ROLE.Viewer;

  const form = useForm({
    defaultValues: {
      content: comment.content,
    },
    validators: {
      onSubmit: CommentSchema,
    },
    onSubmit: async ({ value }) => {
      await updateCommentMutation.mutateAsync({
        id: comment.id,
        content: value.content,
      });
      setIsEditing(false);
    },
  });

  const handleCancel = () => {
    form.reset();
    setIsEditing(false);
  };

  return (
    <CommentLayout
      comment={comment}
      isEditing={isEditing && canEdit}
      onEdit={() => setIsEditing(true)}
      {...props}
    >
      {isEditing && canEdit ? (
        <form
          className="mt-2 space-y-2"
          onSubmit={(e) => {
            e.preventDefault();
            e.stopPropagation();
            form.handleSubmit();
          }}
        >
          <form.Field name="content">
            {({ state, handleChange }) => (
              <Textarea
                className="resize-none"
                placeholder="Comment..."
                value={state.value}
                onChange={(e) => handleChange(e.target.value)}
                autoFocus
                rows={5}
              />
            )}
          </form.Field>
          <div className="flex gap-2 justify-end">
            <Button
              type="button"
              variant="ghost"
              size="sm"
              onClick={handleCancel}
              disabled={updateCommentMutation.isPending}
            >
              <XIcon className="size-4 mr-1" />
              Cancel
            </Button>
            <Button
              type="submit"
              size="sm"
              disabled={
                updateCommentMutation.isPending ||
                !form.state.values.content.trim()
              }
            >
              <CheckIcon className="size-4 mr-1" />
              {updateCommentMutation.isPending ? "Saving..." : "Save"}
            </Button>
          </div>
        </form>
      ) : (
        <div className="mt-2 text-sm">
          <p className="text-foreground whitespace-pre-wrap wrap-break-word leading-relaxed">
            {comment.content}
          </p>
        </div>
      )}
    </CommentLayout>
  );
}

interface CommentFormProps {
  currentUser: UserDto;
  commentId?: number;
  cardId: number;
}

export function CommentForm({
  currentUser,
  commentId,
  cardId,
}: CommentFormProps) {
  const { data } = useCommentQuery(cardId, commentId);
  const createCommentMutation = useCommentCreateMutation(cardId);
  const updateCommentMutation = useCommentUpdateMutation(cardId);

  const form = useForm({
    defaultValues: {
      content: "",
    },
    validators: {
      onSubmit: CommentSchema,
    },
    onSubmit: ({ value }) => {
      if (commentId) {
        updateCommentMutation.mutate({
          id: commentId,
          content: value.content,
        });
      } else {
        createCommentMutation.mutate({
          content: value.content,
        });
      }
    },
  });

  useEffect(() => {
    if (data && commentId) {
      form.reset({
        content: data.content,
      });
    }
  }, [data, commentId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    e.stopPropagation();
    await form.handleSubmit();
    // Reset form after successful submission
    if (!commentId) {
      form.reset();
    }
  };

  return (
    <form className="my-5 w-full space-y-2" onSubmit={handleSubmit}>
      <div className="flex w-full gap-x-3">
        <Avatar className="size-9">
          <AvatarFallback className="text-xs">
            {getInitials(currentUser.username)}
          </AvatarFallback>
        </Avatar>
        <form.Field name="content">
          {({ state, handleChange }) => (
            <CommentInput
              value={state.value}
              onChange={handleChange}
              disabled={
                createCommentMutation.isPending ||
                updateCommentMutation.isPending
              }
            />
          )}
        </form.Field>
      </div>
    </form>
  );
}

export function SkeletonCard({
  className,
  ...props
}: React.ComponentProps<"div">) {
  return (
    <div
      className={cn(
        "flex gap-3 rounded-lg border bg-card px-4 py-3",
        className
      )}
      {...props}
    >
      <div>
        <Skeleton className="size-9 rounded-full" />
      </div>
      <div className="flex-1 min-w-0 space-y-2">
        <div className="flex items-center gap-2">
          <Skeleton className="h-4 w-24" />
          <div className="bg-muted h-1 w-1 rounded-full" />
          <Skeleton className="h-3 w-16" />
        </div>
        <div className="space-y-1.5">
          <Skeleton className="size-full" />
          <Skeleton className="size-4/5" />
        </div>
      </div>
    </div>
  );
}

function getInitials(value: string) {
  return value
    .split(" ")
    .map((word) => word.charAt(0).toUpperCase())
    .slice(0, 2)
    .join("");
}

function getTimeAgo(timestamp: number | Date) {
  const date = new Date(timestamp);
  const now = new Date();
  const timeDifference = now.getTime() - date.getTime();

  const seconds = Math.floor(timeDifference / 1000);
  const minutes = Math.floor(seconds / 60);
  const hours = Math.floor(minutes / 60);
  const days = Math.floor(hours / 24);
  const months = Math.floor(days / 30);
  const years = Math.floor(months / 12);

  if (seconds < 60) {
    return `Just Now`;
  } else if (minutes < 60) {
    return `${minutes} min. ago`;
  } else if (hours < 24) {
    return `${hours} hr. ago`;
  } else if (days < 30) {
    return `${days} day${days === 1 ? "" : "s"} ago`;
  } else if (months < 12) {
    return `${months} mo ago`;
  } else {
    return `${years} yr. ago`;
  }
}
