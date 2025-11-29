import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import type { CardDto } from "@/dto/card.dto";
import { useKanbanStore } from "@/stores/kanban-store";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { GripVertical, Pencil } from "lucide-react";

type ColumnCardProps = {
  card: CardDto;
};

export type CardType = "Card";

export type CardDragData = {
  type: CardType;
  card: CardDto;
};

export function ColumnCard({ card }: ColumnCardProps) {
  const {
    setNodeRef,
    attributes,
    listeners,
    transform,
    transition,
    isDragging,
  } = useSortable({
    id: `card-${card.id}`,
    data: {
      type: "Card",
      card,
    } satisfies CardDragData,
    attributes: {
      roleDescription: "Card",
    },
  });
  const setCardDialogUpdateDialogOpen = useKanbanStore(
    (s) => s.setCardDialogUpdateDialogOpen
  );
  const setCardId = useKanbanStore((s) => s.setCardId);

  const style = {
    transition,
    transform: CSS.Transform.toString(transform),
  };

  if (isDragging) {
    return (
      <div
        ref={setNodeRef}
        style={style}
        className="w-72 h-16 bg-gray-100 opacity-40 border-2 border-dashed border-primary rounded-md"
      />
    );
  }

  return (
    <Card ref={setNodeRef} style={style} className="p-2">
      <CardContent className="p-0 flex items-center gap-2 align-middle text-left whitespace-pre-wrap">
        <Button
          size="icon"
          variant="ghost"
          {...attributes}
          {...listeners}
          className="-ml-2 cursor-grab text-secondary-foreground/80 hover:text-secondary-foreground"
        >
          <span className="sr-only">Move card</span>
          <GripVertical />
        </Button>

        <div className="grow">
          <h2 className="font-medium">{card.title}</h2>
          {card.description && (
            <p className="mt-1 text-sm text-secondary-foreground/80">
              {card.description}
            </p>
          )}
        </div>

        <Button
          variant="ghost"
          size="icon-sm"
          type="button"
          onClick={() => {
            setCardDialogUpdateDialogOpen(true);
            setCardId(card.id);
          }}
        >
          <span className="sr-only">Edit card</span>
          <Pencil />
        </Button>
      </CardContent>
    </Card>
  );
}
