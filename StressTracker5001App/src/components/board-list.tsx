import { useBoardsQuery } from "@/hooks/use-boards-query";
import { useBoardCreateDialogStore } from "@/stores/board-create-dialog-store";
import { Plus } from "lucide-react";
import { BoardCard } from "./board-card";
import { BoardsEmptyState } from "./boards-empty-state";
import { FetchingErrorAlert } from "./fetching-error-alert";
import { Button } from "./ui/button";
import { Skeleton } from "./ui/skeleton";

export function BoardList() {
  const setBoardCreateDialogOpen = useBoardCreateDialogStore(
    (state) => state.setIsOpen
  );
  const { data: boards, status, error, refetch } = useBoardsQuery();

  if (status === "pending") {
    return (
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 6 }).map((_, index) => (
          <Skeleton key={index} className="h-12" />
        ))}
      </div>
    );
  }

  if (status === "error") {
    return <FetchingErrorAlert error={error} refetch={refetch} />;
  }

  const hasBoards = boards.length > 0;

  if (!hasBoards) {
    return <BoardsEmptyState />;
  }

  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
      {boards.map((board) => (
        <BoardCard {...board} key={board.id} />
      ))}
      <Button
        variant="outline"
        className="h-auto min-h-[180px] flex-col gap-2 border-dashed hover:border-primary hover:bg-muted/50"
        onClick={() => setBoardCreateDialogOpen(true)}
      >
        <Plus className="h-8 w-8" />
        <span>Create New Board</span>
      </Button>
    </div>
  );
}
