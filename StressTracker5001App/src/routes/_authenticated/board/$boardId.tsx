import { BoardKanban, BoardKanbanSkeleton } from "@/components/board-kanban";
import { BoardUpdateDialog } from "@/components/board-update-dialog";
import { CardDetailsDialog } from "@/components/card-details-dialog";
import { ColumnUpdateDialog } from "@/components/column-update-dialog";
import { FetchingErrorAlert } from "@/components/fetching-error-alert";
import { TagManagementDialog } from "@/components/tag-management-dialog";
import { Card, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import type { BoardDetailsDto } from "@/dto/board.dto";
import { useBoardQuery } from "@/hooks/use-board-query";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated/board/$boardId")({
  component: RouteComponent,
});

function RouteComponent() {
  const { boardId } = Route.useParams();
  const { data, status, error, refetch } = useBoardQuery(Number(boardId));

  if (status === "pending") {
    return <BoardSkeleton />;
  }
  if (status === "error") {
    return (
      <FetchingErrorAlert
        title="Failed to load board"
        error={error}
        refetch={refetch}
      />
    );
  }

  return <BoardDetails board={data} />;
}

function BoardSkeleton() {
  return (
    <div className="p-6 grid gap-4">
      <Skeleton className="h-6 w-full" />

      <Skeleton className="h-4 w-full" />

      <BoardKanbanSkeleton />
    </div>
  );
}

interface BoardDetailsProps {
  board: BoardDetailsDto;
}

function BoardDetails({ board }: BoardDetailsProps) {
  return (
    <div className="space-y-4">
      <Card className="mx-6 mt-6">
        <CardHeader>
          <div className="flex justify-between items-center w-full">
            <h2 className="text-lg font-medium">{board.name}</h2>
            <div className="flex gap-2">
              <TagManagementDialog boardId={board.id} tags={board.tags} />
              <BoardUpdateDialog
                defaultValues={{
                  name: board.name,
                  description: board.description,
                }}
              />
            </div>
          </div>
        </CardHeader>
      </Card>

      <BoardKanban board={board} />
      <CardDetailsDialog boardId={board.id} />
      {board.columns.map((column) => (
        <ColumnUpdateDialog
          key={column.id}
          boardId={board.id}
          column={column}
        />
      ))}
    </div>
  );
}
