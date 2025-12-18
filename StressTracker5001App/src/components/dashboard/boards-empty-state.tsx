import { useBoardCreateDialogStore } from "@/stores/board-create-dialog-store";
import { PlusIcon } from "lucide-react";
import { Button } from "../ui/button";
import {
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "../ui/empty";

export function BoardsEmptyState() {
  const setBoardCreateDialogOpen = useBoardCreateDialogStore(
    (state) => state.setIsOpen
  );

  return (
    <Empty>
      <EmptyHeader>
        <EmptyMedia variant="icon">
          <PlusIcon />
        </EmptyMedia>
        <EmptyTitle>No Boards Created Yet</EmptyTitle>
        <EmptyDescription>
          Get started by creating your first stress tracking board.
        </EmptyDescription>
      </EmptyHeader>
      <EmptyContent>
        <Button onClick={() => setBoardCreateDialogOpen(true)}>
          Create Board
        </Button>
      </EmptyContent>
    </Empty>
  );
}
