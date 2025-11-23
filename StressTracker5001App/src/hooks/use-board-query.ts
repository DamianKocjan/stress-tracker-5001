import type { BoardDto } from "@/dto/board.dto";
import { fetch } from "@/utils/fetch";
import { useQuery } from "@tanstack/react-query";

export function useBoardQuery(boardId: number) {
  return useQuery({
    queryKey: ["boards", boardId],
    queryFn: async () => {
      const response = await fetch(`/boards/${boardId}`, {
        method: "GET",
      });

      if (!response.ok) {
        throw new Error("Failed to fetch board");
      }

      return response.json() as Promise<BoardDto>;
    },
  });
}
