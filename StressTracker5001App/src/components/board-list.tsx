import { Plus } from "lucide-react";
import { BoardCard } from "./board-card";
import { BoardsEmptyState } from "./boards-empty-state";
import { Button } from "./ui/button";

// Mock data for boards - replace with actual data fetching later
const boards = [
  // Uncomment to test populated state
  {
    Id: 1,
    Name: "Stress Tracking Board 1",
    Description: "Track your stress levels over time.",
    OwnerId: 1,
    Owner: {
      Id: 1,
      Email: "owner@example.com",
      Username: "owneruser",
      CreatedAt: "2024-01-01T00:00:00Z",
      UpdatedAt: "2024-01-02T00:00:00Z",
    },
    CreatedAt: "2024-01-01T00:00:00Z",
    UpdatedAt: "2024-01-02T00:00:00Z",
  },
];

export function BoardList() {
  const hasBoards = boards.length > 0;

  if (!hasBoards) {
    return <BoardsEmptyState />;
  }

  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
      {boards.map((board) => (
        <BoardCard {...board} key={board.Id} />
      ))}
      <Button
        variant="outline"
        className="h-auto min-h-[180px] flex-col gap-2 border-dashed hover:border-primary hover:bg-muted/50"
      >
        <Plus className="h-8 w-8" />
        <span>Create New Board</span>
      </Button>
    </div>
  );
}
