import { useCardCommentsQuery } from "@/hooks/use-card-comments-query";
import { useCommentDeleteMutation } from "@/hooks/use-comment-delete-mutation";
import { useAuth } from "@/providers/auth";
import { CommentCard, CommentForm, SkeletonCard } from "./ui/comments";

interface CardCommentsProps {
  cardId: number;
}

export function CardComments({ cardId }: CardCommentsProps) {
  const { data: comments = [], status } = useCardCommentsQuery(cardId);
  const { user } = useAuth();
  const deleteCommentMutation = useCommentDeleteMutation(cardId);

  return (
    <div className="flex w-full items-start justify-center">
      <div className="w-full max-w-2xl">
        <div className="mb-6">
          <h2 className="text-lg font-bold mb-1">Comments</h2>
          <p className="text-muted-foreground text-sm">
            {status === "success" && comments.length > 0
              ? `${comments.length} ${comments.length === 1 ? "comment" : "comments"}`
              : "Be the first to comment"}
          </p>
        </div>
        <div className="bg-border h-px w-full mb-4" />
        <div className="space-y-3">
          <CommentForm cardId={cardId} currentUser={user!} />
          {status === "pending" ? (
            <div className="space-y-3">
              {Array.from({ length: 3 }).map((_, index) => (
                <SkeletonCard key={index} />
              ))}
            </div>
          ) : comments.length > 0 ? (
            comments.map((comment) => (
              <CommentCard
                key={`comment-${comment.id}`}
                cardId={cardId}
                onDelete={deleteCommentMutation.mutate}
                currentUser={user!}
                comment={comment}
              />
            ))
          ) : (
            <div className="text-center py-12">
              <p className="text-muted-foreground text-sm">
                No comments yet. Start the conversation!
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
