import type { BoardDto } from "@/dto/board.dto";
import { fetch } from "@/utils/fetch";
import { useQuery } from "@tanstack/react-query";

export function useBoardsQuery() {
  return useQuery({
    queryKey: ["boards"],
    queryFn: async () => {
      const response = await fetch("/boards", {
        method: "GET",
      });

      if (!response.ok) {
        throw new Error("Failed to fetch boards");
      }

      return response.json() as Promise<BoardDto[]>;
    },
  });
}
