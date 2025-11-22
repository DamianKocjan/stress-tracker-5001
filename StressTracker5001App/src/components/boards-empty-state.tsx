import { PlusIcon } from "lucide-react";
import { Button } from "./ui/button";
import {
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "./ui/empty";

export function BoardsEmptyState() {
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
        <Button>Create Board</Button>
      </EmptyContent>
    </Empty>
  );
}
