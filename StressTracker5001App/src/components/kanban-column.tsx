import type { CardDto } from "@/dto/card.dto";
import type { ColumnDto } from "@/dto/column.dto";
import { cn } from "@/lib/utils";
import { useKanbanStore } from "@/stores/kanban-store";
import { SortableContext, useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { GripVertical } from "lucide-react";
import { useMemo } from "react";
import { ColumnCard } from "./column-card";
import { Badge } from "./ui/badge";
import { Button } from "./ui/button";
import { Card, CardContent, CardFooter, CardHeader } from "./ui/card";
import { ScrollArea } from "./ui/scroll-area";

export type ColumnType = "Column";

export type ColumnDragData = {
  type: ColumnType;
  column: ColumnDto;
};

interface KanbanColumnProps {
  column: ColumnDto;
  cards: CardDto[];
}

export function KanbanColumn({ column, cards }: KanbanColumnProps) {
  const setCardCreateDialogOpen = useKanbanStore(
    (s) => s.setIsCardDialogCreateDialogOpen
  );
  const setColumnId = useKanbanStore((s) => s.setColumnId);

  const sortedCards = useMemo(() => {
    return cards.toSorted((a, b) => a.position - b.position);
  }, [cards]);

  const cardIds = useMemo(() => {
    return sortedCards.map((card) => `card-${card.id}`);
  }, [sortedCards]);

  const isColumnOverLimit =
    column.wipLimit !== null ? sortedCards.length > column.wipLimit : false;

  const {
    setNodeRef,
    attributes,
    listeners,
    transform,
    transition,
    isDragging,
  } = useSortable({
    id: `column-${column.id}`,
    data: {
      type: "Column",
      column,
    } satisfies ColumnDragData,
    attributes: {
      roleDescription: `Column: ${column.name}`,
    },
    animateLayoutChanges: () => true,
  });

  const style = {
    transition,
    transform: CSS.Translate.toString(transform),
  };

  if (isDragging) {
    return (
      <div
        ref={setNodeRef}
        style={style}
        className="min-h-[200px] min-w-80 bg-gray-100/30 border-2 border-dashed border-primary rounded-md"
      />
    );
  }

  return (
    <div ref={setNodeRef} style={style}>
      <Card
        className={cn(
          "min-h-[200px] min-w-80 bg-primary-foreground flex flex-col shrink-0 snap-center overflow-y-auto pb-0 transition-all duration-200 hover:shadow-md gap-2",
          {
            // If column is over WIP limit, apply a red background
            "shadow-md shadow-destructive": isColumnOverLimit,
          }
        )}
      >
        <CardHeader className="font-semibold border-b flex flex-row items-center justify-between">
          <Button
            size="icon"
            variant="ghost"
            {...attributes}
            {...listeners}
            className="-ml-2 cursor-grab text-secondary-foreground/80 hover:text-secondary-foreground"
          >
            <span className="sr-only">Move column</span>
            <GripVertical />
          </Button>

          <h1>{column.name}</h1>

          {column.wipLimit && sortedCards.length ? (
            <Badge variant={isColumnOverLimit ? "destructive" : "outline"}>
              {sortedCards.length}/{column.wipLimit}
            </Badge>
          ) : sortedCards.length ? (
            <Badge variant="outline">{sortedCards.length}</Badge>
          ) : (
            <div className="w-6" />
          )}
        </CardHeader>
        <ScrollArea>
          <CardContent className="flex grow flex-col gap-2 p-2">
            <SortableContext items={cardIds}>
              {sortedCards.length === 0 ? (
                <div className="flex grow items-center justify-center">
                  <p className="text-gray-400">No cards here.</p>
                </div>
              ) : (
                sortedCards.map((c) => <ColumnCard key={c.id} card={c} />)
              )}
            </SortableContext>
          </CardContent>
        </ScrollArea>
        <CardFooter className="px-0 mt-auto">
          <Button
            type="button"
            className="w-full"
            variant="ghost"
            onClick={() => {
              setColumnId(column.id);
              setCardCreateDialogOpen(true);
            }}
          >
            Create Card
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}
