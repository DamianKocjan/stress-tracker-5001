import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter } from "@/components/ui/card";
import type { CardDto } from "@/dto/card.dto";
import { useTagsQuery } from "@/hooks/use-tags-query";
import { useKanbanStore } from "@/stores/kanban-store";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { GripVertical } from "lucide-react";
import { memo } from "react";
import { RoleGuard } from "../role-guard";
import { TagBadge } from "../tags/tag-badge";
import { Avatar, AvatarFallback, AvatarGroup } from "../ui/avatar";
import { Tooltip, TooltipContent, TooltipTrigger } from "../ui/tooltip";

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
    <Card
      ref={setNodeRef}
      style={style}
      className="p-2 cursor-pointer hover:shadow-md"
      onClick={() => setCardId(card.id)}
    >
      <CardContent className="p-0 flex items-center gap-2 align-middle text-left whitespace-pre-wrap">
        <RoleGuard minRole="Member">
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
        </RoleGuard>

        <div className="grow space-y-2">
          <h2 className="font-medium max-w-sm break-all">{card.title}</h2>
          {card.description && (
            <p className="text-sm text-secondary-foreground/80 max-w-sm truncate">
              {card.description}
            </p>
          )}

          <CardTags tags={card.tags} />
        </div>
      </CardContent>

      <CardAssignedUsers assignments={card.assignments} />
    </Card>
  );
}

const CardTags = memo(function CardTags({ tags }: { tags?: CardDto["tags"] }) {
  const { data: tagData } = useTagsQuery();

  if (!tags || tags.length === 0) {
    return null;
  }

  return (
    <div className="flex flex-wrap gap-1">
      {tags.map((tag) => {
        const tagInfo = tagData?.find((t) => t.id === tag);

        if (!tagInfo) {
          return null;
        }
        return (
          <TagBadge
            key={tag}
            tag={tagInfo}
            variant="outline"
            className="text-xs"
          />
        );
      })}
    </div>
  );
});

const CardAssignedUsers = memo(function CardAssignedUsers({
  assignments,
}: {
  assignments: CardDto["assignments"];
}) {
  if (assignments.length === 0) {
    return null;
  }

  return (
    <CardFooter>
      <AvatarGroup>
        {assignments.map((assignment) => (
          <Tooltip key={assignment.id}>
            <TooltipTrigger>
              <Avatar>
                <AvatarFallback>
                  {assignment.user.username.charAt(0).toUpperCase()}
                </AvatarFallback>
              </Avatar>
            </TooltipTrigger>

            <TooltipContent>
              <p>{assignment.user.username}</p>
            </TooltipContent>
          </Tooltip>
        ))}
      </AvatarGroup>
    </CardFooter>
  );
});
