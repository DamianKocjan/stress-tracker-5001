import { ActivityLogDrawer } from "@/components/activity-log/activity-log-drawer";
import {
  BoardKanban,
  BoardKanbanSkeleton,
} from "@/components/board/board-kanban";
import { BoardUpdateDialog } from "@/components/board/board-update-dialog";
import { CardDetailsDialog } from "@/components/card/card-details-dialog";
import { ColumnUpdateDialog } from "@/components/column/column-update-dialog";
import { FetchingErrorAlert } from "@/components/fetching-error-alert";
import { MembersDialog } from "@/components/member/members-dialog";
import { RoleGuard } from "@/components/role-guard";
import { TagManagementDialog } from "@/components/tags/tag-management-dialog";
import { Button } from "@/components/ui/button";
import { Card, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import type { BoardDetailsDto } from "@/dto/board.dto";
import { useBoardQuery } from "@/hooks/use-board-query";
import { createFileRoute } from "@tanstack/react-router";
import { ActivityIcon } from "lucide-react";
import { useState } from "react";

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
      <div className="container mx-auto">
        <FetchingErrorAlert
          title="Failed to load board"
          error={error}
          refetch={refetch}
        />
      </div>
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
  const [isActivityDrawerOpen, setIsActivityDrawerOpen] = useState(false);

  return (
    <div className="space-y-4">
      <Card className="mx-6 mt-6">
        <CardHeader>
          <div className="flex justify-between items-center w-full">
            <h2 className="text-lg font-medium">{board.name}</h2>
            <div className="flex gap-2">
              <RoleGuard minRole="Admin">
                <MembersDialog boardId={board.id} />
                <TagManagementDialog boardId={board.id} tags={board.tags} />
                <Button
                  onClick={() => setIsActivityDrawerOpen(true)}
                  variant="outline"
                  size="sm"
                >
                  <ActivityIcon className="mr-2 size-4" />
                  Activity
                </Button>
                <BoardUpdateDialog
                  defaultValues={{
                    name: board.name,
                    description: board.description,
                  }}
                />
              </RoleGuard>
            </div>
          </div>
        </CardHeader>
      </Card>

      <BoardKanban board={board} />
      <CardDetailsDialog boardId={board.id} />

      <RoleGuard minRole="Admin">
        {board.columns.map((column) => (
          <ColumnUpdateDialog
            key={column.id}
            boardId={board.id}
            column={column}
          />
        ))}
      </RoleGuard>

      <ActivityLogDrawer
        isOpen={isActivityDrawerOpen}
        boardId={board.id}
        onClose={() => setIsActivityDrawerOpen(false)}
      />
    </div>
  );
}
