import { useKanbanStore } from "@/stores/kanban-store";
import { Plus } from "lucide-react";
import { Button } from "../ui/button";
import {
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "../ui/empty";

interface KanbanAddColumnProps {
  hasColumns: boolean;
}

export function KanbanAddColumn({ hasColumns }: KanbanAddColumnProps) {
  const setIsOpen = useKanbanStore((s) => s.setIsColumnCreateDialogOpen);

  return (
    <Empty className="border border-dashed min-w-80">
      <EmptyHeader>
        <EmptyMedia variant="icon">
          <Plus />
        </EmptyMedia>
        <EmptyTitle>{hasColumns ? "New Column" : "No Columns Yet"}</EmptyTitle>
        <EmptyDescription></EmptyDescription>
      </EmptyHeader>
      <EmptyContent>
        <Button variant="outline" size="sm" onClick={() => setIsOpen(true)}>
          New Column
        </Button>
      </EmptyContent>
    </Empty>
  );
}
