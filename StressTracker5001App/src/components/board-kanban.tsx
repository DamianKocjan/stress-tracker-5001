import type { BoardDetailsDto } from "@/dto/board.dto";
import type { CardDto } from "@/dto/card.dto";
import type { ColumnDto } from "@/dto/column.dto";
import { useCardMoveMutation } from "@/hooks/use-card-move-mutation";
import { useColumnMoveMutation } from "@/hooks/use-column-move-mutation";
import { boardQueryKey } from "@/utils/query-options";
import {
  closestCenter,
  closestCorners,
  DndContext,
  DragOverlay,
  KeyboardSensor,
  PointerSensor,
  pointerWithin,
  useSensor,
  useSensors,
  type CollisionDetection,
  type DragEndEvent,
  type DragOverEvent,
  type DragStartEvent,
} from "@dnd-kit/core";
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
} from "@dnd-kit/sortable";
import { useQueryClient } from "@tanstack/react-query";
import { useCallback, useEffect, useMemo, useState } from "react";
import { createPortal } from "react-dom";
import { CardCreateDialog } from "./card-create-dialog";
import { ColumnCard } from "./column-card";
import { ColumnCreateDialog } from "./column-create-dialog";
import { KanbanAddColumn } from "./kanban-add-column";
import { KanbanColumn } from "./kanban-column";
import { ScrollArea, ScrollBar } from "./ui/scroll-area";
import { Skeleton } from "./ui/skeleton";

interface BoardKanbanProps {
  board: BoardDetailsDto;
}

export function BoardKanban({ board }: BoardKanbanProps) {
  const queryClient = useQueryClient();
  const { mutateAsync: moveCard } = useCardMoveMutation(board.id);
  const { mutateAsync: moveColumn } = useColumnMoveMutation(board.id);

  // Local state for columns and cards - synced with props
  const [columns, setColumns] = useState<ColumnDto[]>(board.columns);
  const [cards, setCards] = useState<CardDto[]>(board.cards);

  // Keep local state in sync with props when they change (e.g., after API response)
  useEffect(() => {
    setColumns(board.columns);
  }, [board.columns]);

  useEffect(() => {
    setCards(board.cards);
  }, [board.cards]);

  const [activeColumn, setActiveColumn] = useState<ColumnDto | null>(null);
  const [activeCard, setActiveCard] = useState<CardDto | null>(null);

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: { distance: 10 },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  // Custom collision detection: use pointerWithin for cards, closestCenter for columns
  const collisionDetectionStrategy: CollisionDetection = useCallback(
    (args) => {
      // If dragging a column, use closestCenter for better horizontal detection
      if (activeColumn) {
        return closestCenter(args);
      }

      // For cards: first check pointerWithin, then fall back to closestCorners
      const pointerCollisions = pointerWithin(args);
      if (pointerCollisions.length > 0) {
        return pointerCollisions;
      }

      return closestCorners(args);
    },
    [activeColumn]
  );

  const columnIds = useMemo(
    () =>
      columns
        .toSorted((a, b) => a.position - b.position)
        .map((column) => `column-${column.id}`),
    [columns]
  );

  const cardsToColumnsMap = useMemo(() => {
    const map: Map<number, CardDto[]> = new Map();

    cards.forEach((card) => {
      if (!map.has(card.columnId)) {
        map.set(card.columnId, []);
      }
      map.get(card.columnId)!.push(card);
    });

    return map;
  }, [cards]);

  // Helper function to recalculate positions for cards in a specific column
  function recalculateCardPositions(
    cardsList: CardDto[],
    columnId: number
  ): CardDto[] {
    let position = 0;
    return cardsList.map((card) => {
      if (card.columnId === columnId) {
        return { ...card, position: position++ };
      }
      return card;
    });
  }

  function onDragStart(event: DragStartEvent) {
    if (event.active.data.current?.type === "Column") {
      setActiveColumn(event.active.data.current.column);
      return;
    }
    if (event.active.data.current?.type === "Card") {
      setActiveCard(event.active.data.current.card);
      return;
    }
  }

  function onDragOver(event: DragOverEvent) {
    const { active, over } = event;
    if (!over) return;

    const activeId = active.id;
    const overId = over.id;

    if (activeId === overId) return;

    const isActiveCard = active.data.current?.type === "Card";
    const isOverCard = over.data.current?.type === "Card";
    const isOverColumn = over.data.current?.type === "Column";

    if (!isActiveCard) return;

    // Extract numeric IDs from prefixed string IDs
    const activeCardId = active.data.current?.card?.id as number;

    // Update local state optimistically
    setCards((prevCards) => {
      const newCards = [...prevCards];
      const activeIndex = newCards.findIndex((t) => t.id === activeCardId);

      if (activeIndex === -1) return prevCards;

      if (isActiveCard && isOverCard) {
        const overCardId = over.data.current?.card?.id as number;
        const overIndex = newCards.findIndex((t) => t.id === overCardId);
        if (overIndex === -1) return prevCards;

        const activeColumnId = newCards[activeIndex].columnId;
        const overColumnId = newCards[overIndex].columnId;

        if (activeColumnId !== overColumnId) {
          newCards[activeIndex] = {
            ...newCards[activeIndex],
            columnId: overColumnId,
          };
          const movedCards = arrayMove(newCards, activeIndex, overIndex - 1);
          // Recalculate positions for both source and target columns
          let updatedCards = recalculateCardPositions(movedCards, overColumnId);
          updatedCards = recalculateCardPositions(updatedCards, activeColumnId);
          return updatedCards;
        }

        const movedCards = arrayMove(newCards, activeIndex, overIndex);
        return recalculateCardPositions(movedCards, activeColumnId);
      }

      if (isActiveCard && isOverColumn) {
        const targetColumnId = over.data.current?.column?.id as number;
        const previousColumnId = newCards[activeIndex].columnId;

        // Don't update if already in the same column
        if (previousColumnId === targetColumnId) return prevCards;

        newCards[activeIndex] = {
          ...newCards[activeIndex],
          columnId: targetColumnId,
        };
        let updatedCards = recalculateCardPositions(newCards, targetColumnId);
        updatedCards = recalculateCardPositions(updatedCards, previousColumnId);
        return updatedCards;
      }

      return prevCards;
    });
  }

  function onDragEnd(event: DragEndEvent) {
    const { active, over } = event;

    // Store references before clearing state
    const draggedColumn = activeColumn;
    const draggedCard = activeCard;

    setActiveColumn(null);
    setActiveCard(null);

    if (!over) {
      // Reset to original state if no drop target
      setColumns(board.columns);
      setCards(board.cards);
      return;
    }

    const isActiveColumn = active.data.current?.type === "Column";

    if (isActiveColumn && draggedColumn) {
      // Get the actual column IDs from the data
      const activeColumnId = draggedColumn.id;
      const overColumn = over.data.current?.column;
      const overColumnId = overColumn?.id as number | undefined;

      // If dropping over a card, get the column from the card
      const overCard = over.data.current?.card as CardDto | undefined;
      const targetColumnId = overColumnId ?? overCard?.columnId;

      // If we still don't have a target, reset and return
      if (!targetColumnId) {
        setColumns(board.columns);
        return;
      }

      // Check if column has moved actually
      const activeIndex = columns.findIndex((c) => c.id === activeColumnId);
      const overIndex = columns.findIndex((c) => c.id === targetColumnId);

      if (activeIndex === overIndex) {
        return;
      }

      // Update local state
      const newColumns = arrayMove(columns, activeIndex, overIndex);
      setColumns(newColumns);

      // Update query cache without fetching
      queryClient.setQueryData(
        boardQueryKey(board.id),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) return oldData;
          return {
            ...oldData,
            columns: newColumns,
          };
        }
      );

      // Column movement - call API
      moveColumn({
        columnId: activeColumnId,
        newPosition: overIndex,
      });
      return;
    }

    // Card movement - call API
    if (draggedCard) {
      // Get current state of the card from local state (after drag updates)
      const currentCard = cards.find((c) => c.id === draggedCard.id);

      if (!currentCard) {
        setCards(board.cards);
        return;
      }

      // Get the new position from local state (already recalculated during onDragOver)
      const newColumnId = currentCard.columnId;
      const newPosition = currentCard.position;

      // Only call API if something actually changed from original position
      // Compare against draggedCard which has the ORIGINAL position from drag start
      if (
        newColumnId === draggedCard.columnId &&
        newPosition === draggedCard.position
      ) {
        return;
      }

      // Update query cache without fetching
      queryClient.setQueryData(
        boardQueryKey(board.id),
        (oldData: BoardDetailsDto | undefined) => {
          if (!oldData) return oldData;
          return {
            ...oldData,
            cards: cards,
          };
        }
      );

      moveCard({
        cardId: draggedCard.id,
        newColumnId: newColumnId,
        newPosition: newPosition,
      });
    }
  }

  return (
    <DndContext
      sensors={sensors}
      collisionDetection={collisionDetectionStrategy}
      onDragStart={onDragStart}
      onDragOver={onDragOver}
      onDragEnd={onDragEnd}
    >
      <div className="grid size-full min-h-120 w-full max-h-[calc(100vh-8rem)]">
        <ColumnCreateDialog boardId={board.id} />
        <CardCreateDialog boardId={board.id} />

        <ScrollArea className="size-full max-w-dvw overflow-hidden">
          <div className="flex gap-4 py-4 px-6 min-h-[75dvh]">
            <KanbanAddColumn hasColumns={columns.length > 0} />

            <SortableContext items={columnIds}>
              {columns.map((column) => (
                <KanbanColumn
                  key={column.id}
                  column={column}
                  cards={cardsToColumnsMap.get(column.id) || []}
                />
              ))}
            </SortableContext>
          </div>
          <ScrollBar orientation="horizontal" />
        </ScrollArea>
      </div>

      {createPortal(
        <DragOverlay>
          {activeColumn && (
            <KanbanColumn
              column={activeColumn}
              cards={cardsToColumnsMap.get(activeColumn.id) || []}
            />
          )}
          {activeCard && <ColumnCard card={activeCard} />}
        </DragOverlay>,
        document.body
      )}
    </DndContext>
  );
}

export function BoardKanbanSkeleton() {
  return (
    <div className="overflow-hidden">
      <div className="grid grid-cols-4 min-w-7xl min-h-96 w-full gap-4">
        {Array.from({ length: 4 }).map((_, index) => (
          <div key={index}>
            <Skeleton className="w-full h-8 mb-2" />

            <Skeleton className="w-full h-[calc(100%-2rem)]" />
          </div>
        ))}
      </div>
    </div>
  );
}
