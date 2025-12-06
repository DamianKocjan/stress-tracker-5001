import { Skeleton } from "@/components/ui/skeleton";
import { useCommentDeleteMutation } from "@/hooks/use-comment-delete-mutation";
import { useCardCommentsInfiniteQuery } from "@/hooks/use-comment-infinite-query";
import { useAuth } from "@/providers/auth";
import { Fragment } from "react/jsx-runtime";
import { FetchingErrorAlert } from "./fetching-error-alert";
import { CommentCard, CommentForm, SkeletonCard } from "./ui/comments";

interface CardCommentsProps {
  cardId: number;
}

export function CardComments({ cardId }: CardCommentsProps) {
  const { data, status, error, refetch, hasNextPage, fetchNextPage } =
    useCardCommentsInfiniteQuery(cardId);
  const { user } = useAuth();
  const deleteCommentMutation = useCommentDeleteMutation(cardId);

  if (status === "pending") {
    return <CardCommentsSkeleton />;
  }

  if (status === "error") {
    return (
      <FetchingErrorAlert
        title="Failed to load comments"
        error={error}
        refetch={refetch}
      />
    );
  }
  const commentCount =
    data?.pages.reduce((total, page) => total + page.items.length, 0) || 0;
  const hasAnyComments = commentCount > 0;

  return (
    <div className="flex w-full items-start justify-center">
      <div className="w-full">
        <div className="mb-6">
          <h2 className="text-lg font-bold mb-1">Comments</h2>
          <p className="text-muted-foreground text-sm">
            {hasAnyComments
              ? `${commentCount} ${commentCount === 1 ? "comment" : "comments"}`
              : "Be the first to comment"}
          </p>
        </div>
        <div className="bg-border h-px w-full mb-4" />
        <div className="space-y-3">
          <CommentForm cardId={cardId} currentUser={user!} />
          {/* {hasAnyComments ? (
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
          )} */}

          {data.pages.map((page, pageIndex) => (
            <Fragment key={`card-${cardId}-comments-page-${pageIndex}`}>
              {page.items.map((comment) => (
                <CommentCard
                  key={`comment-${comment.id}`}
                  cardId={cardId}
                  onDelete={deleteCommentMutation.mutate}
                  currentUser={user!}
                  comment={comment}
                />
              ))}
            </Fragment>
          ))}

          {hasNextPage && (
            <div className="flex justify-center">
              <button
                className="btn btn-outline"
                onClick={() => fetchNextPage()}
              >
                Load more comments
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function CardCommentsSkeleton() {
  return (
    <div className="flex w-full items-start justify-center">
      <div className="w-full">
        <div className="mb-6">
          <Skeleton className="h-7 w-32 mb-2" />
          <Skeleton className="h-4 w-40" />
        </div>
        <div className="bg-border h-px w-full mb-4" />
        <div className="space-y-3">
          {/* Comment form skeleton */}
          <div className="my-5 flex w-full gap-x-3">
            <Skeleton className="h-9 w-9 rounded-full flex-shrink-0" />
            <div className="flex-1">
              <Skeleton className="h-20 w-full rounded-lg" />
            </div>
          </div>
          {/* Comment cards skeleton */}
          {Array.from({ length: 3 }).map((_, index) => (
            <SkeletonCard key={index} />
          ))}
        </div>
      </div>
    </div>
  );
}
