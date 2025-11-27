import type {
  BoardCreateDto,
  BoardDetailsDto,
  BoardDto,
  BoardUpdateDto,
} from "@/dto/board.dto";
import type {
  CardCreateDto,
  CardDetailsDto,
  CardDto,
  CardMoveDto,
  CardUpdateDto,
} from "@/dto/card.dto";
import type {
  ColumnCreateDto,
  ColumnDto,
  ColumnMoveDto,
  ColumnUpdateDto,
} from "@/dto/column.dto";
import { fetch } from "./fetch";

export async function createBoard(data: BoardCreateDto): Promise<BoardDto> {
  const response = await fetch("/boards", {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw new Error("Failed to create board");
  }

  return response.json() as Promise<BoardDto>;
}

export async function getBoards(): Promise<BoardDto[]> {
  const response = await fetch("/boards", {
    method: "GET",
  });

  if (!response.ok) {
    throw new Error("Failed to fetch boards");
  }

  return response.json() as Promise<BoardDto[]>;
}

export async function getBoard(boardId: number): Promise<BoardDetailsDto> {
  const response = await fetch(`/boards/${boardId}`, {
    method: "GET",
  });

  if (!response.ok) {
    throw new Error("Failed to fetch board");
  }

  return response.json() as Promise<BoardDetailsDto>;
}

export async function updateBoard(
  boardId: number,
  data: BoardUpdateDto
): Promise<BoardDto> {
  const response = await fetch(`/boards/${boardId}`, {
    method: "PUT",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw new Error("Failed to update board");
  }

  return response.json() as Promise<BoardDto>;
}

export async function deleteBoard(boardId: number): Promise<void> {
  const response = await fetch(`/boards/${boardId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    throw new Error("Failed to delete board");
  }
}

export async function createColumn(
  boardId: number,
  data: ColumnCreateDto
): Promise<ColumnDto> {
  const response = await fetch(`/boards/${boardId}/columns`, {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw new Error("Failed to create column");
  }

  return response.json() as Promise<ColumnDto>;
}

export async function updateColumn(
  columnId: number,
  data: ColumnUpdateDto
): Promise<ColumnDto> {
  const response = await fetch(`/columns/${columnId}`, {
    method: "PUT",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw new Error("Failed to update column");
  }

  return response.json() as Promise<ColumnDto>;
}

export async function moveColumn(
  columnId: number,
  data: ColumnMoveDto
): Promise<ColumnDto> {
  const response = await fetch(`/columns/${columnId}/move`, {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw new Error("Failed to move column");
  }

  return response.json() as Promise<ColumnDto>;
}

export async function deleteColumn(columnId: number): Promise<void> {
  const response = await fetch(`/columns/${columnId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    throw new Error("Failed to delete column");
  }
}

export async function createCard(
  columnId: number,
  data: CardCreateDto
): Promise<CardDto> {
  const response = await fetch(`/columns/${columnId}/cards`, {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw new Error("Failed to create card");
  }

  return response.json() as Promise<CardDto>;
}

export async function getCardDetails(cardId: number): Promise<CardDetailsDto> {
  const response = await fetch(`/cards/${cardId}`, {
    method: "GET",
  });

  if (!response.ok) {
    throw new Error("Failed to fetch card details");
  }

  return response.json() as Promise<CardDetailsDto>;
}

export async function updateCard(
  cardId: number,
  data: CardUpdateDto
): Promise<CardDetailsDto> {
  const response = await fetch(`/cards/${cardId}`, {
    method: "PUT",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw new Error("Failed to update card");
  }

  return response.json() as Promise<CardDetailsDto>;
}

export async function moveCard(
  cardId: number,
  data: CardMoveDto
): Promise<CardDto> {
  const response = await fetch(`/cards/${cardId}/move`, {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw new Error("Failed to move card");
  }

  return response.json() as Promise<CardDto>;
}

export async function deleteCard(cardId: number): Promise<void> {
  const response = await fetch(`/cards/${cardId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    throw new Error("Failed to delete card");
  }
}
