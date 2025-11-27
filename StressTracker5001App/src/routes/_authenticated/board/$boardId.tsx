import { BoardUpdateDialog } from "@/components/board-update-dialog";
import { FetchingErrorAlert } from "@/components/fetching-error-alert";
import { Card, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import type { BoardDetailsDto } from "@/dto/board.dto";
import { useBoardQuery } from "@/hooks/use-board-query";
import { createFileRoute } from "@tanstack/react-router";

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
    return <FetchingErrorAlert error={error} refetch={refetch} />;
  }

  return <BoardDetails board={data} />;
}

function BoardSkeleton() {
  return (
    <div className="p-6 grid gap-4">
      <Skeleton className="h-6 w-full" />

      <Skeleton className="h-4 w-full" />

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
    </div>
  );
}

interface BoardDetailsProps {
  board: BoardDetailsDto;
}

function BoardDetails({ board }: BoardDetailsProps) {
  return (
    <div className="p-6 space-y-4">
      <Card>
        <CardHeader>
          <div className="flex justify-between items-center w-full">
            <h2 className="text-lg font-medium">{board.name}</h2>
            <BoardUpdateDialog
              defaultValues={{
                name: board.name,
                description: board.description,
              }}
            />
          </div>
        </CardHeader>
      </Card>
    </div>
  );
}
