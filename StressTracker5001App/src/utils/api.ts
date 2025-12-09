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
import type { CommentDto } from "@/dto/comment.dto";
import type { PagedResultDto, ResultDto } from "@/dto/common.dto";
import type { TagCreateDto, TagDto, TagUpdateDto } from "@/dto/tag.dto";
import { fetch } from "./fetch";

function unwrapResult<T>(result: ResultDto<T>): T {
  if (!result.success) {
    throw new Error(result.errorMessage || "An error occurred");
  }
  return result.data as T;
}

export async function createBoard(data: BoardCreateDto): Promise<BoardDto> {
  const response = await fetch("/boards", {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<BoardDto>;
    throw new Error(result.errorMessage || "Failed to create board");
  }

  const result = (await response.json()) as ResultDto<BoardDto>;
  return unwrapResult(result);
}

export async function getBoards(): Promise<BoardDto[]> {
  const response = await fetch("/boards", {
    method: "GET",
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<BoardDto[]>;
    throw new Error(result.errorMessage || "Failed to fetch boards");
  }

  const result = (await response.json()) as ResultDto<BoardDto[]>;
  return unwrapResult(result);
}

export async function getBoard(boardId: number): Promise<BoardDetailsDto> {
  const response = await fetch(`/boards/${boardId}`, {
    method: "GET",
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<BoardDetailsDto>;
    throw new Error(result.errorMessage || "Failed to fetch board");
  }

  const result = (await response.json()) as ResultDto<BoardDetailsDto>;
  return unwrapResult(result);
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
    const result = (await response.json()) as ResultDto<BoardDto>;
    throw new Error(result.errorMessage || "Failed to update board");
  }

  const result = (await response.json()) as ResultDto<BoardDto>;
  return unwrapResult(result);
}

export async function deleteBoard(boardId: number): Promise<void> {
  const response = await fetch(`/boards/${boardId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<void>;
    throw new Error(result.errorMessage || "Failed to delete board");
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
    const result = (await response.json()) as ResultDto<ColumnDto>;
    throw new Error(result.errorMessage || "Failed to create column");
  }

  const result = (await response.json()) as ResultDto<ColumnDto>;
  return unwrapResult(result);
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
    const result = (await response.json()) as ResultDto<ColumnDto>;
    throw new Error(result.errorMessage || "Failed to update column");
  }

  const result = (await response.json()) as ResultDto<ColumnDto>;
  return unwrapResult(result);
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
    const result = (await response.json()) as ResultDto<ColumnDto>;
    throw new Error(result.errorMessage || "Failed to move column");
  }

  const result = (await response.json()) as ResultDto<ColumnDto>;
  return unwrapResult(result);
}

export async function deleteColumn(columnId: number): Promise<void> {
  const response = await fetch(`/columns/${columnId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<void>;
    throw new Error(result.errorMessage || "Failed to delete column");
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
    const result = (await response.json()) as ResultDto<CardDto>;
    throw new Error(result.errorMessage || "Failed to create card");
  }

  const result = (await response.json()) as ResultDto<CardDto>;
  return unwrapResult(result);
}

export async function getCardDetails(cardId: number): Promise<CardDetailsDto> {
  const response = await fetch(`/cards/${cardId}`, {
    method: "GET",
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<CardDetailsDto>;
    throw new Error(result.errorMessage || "Failed to fetch card details");
  }

  const result = (await response.json()) as ResultDto<CardDetailsDto>;
  return unwrapResult(result);
}

export async function getCardComments(cardId: number): Promise<CommentDto[]> {
  const response = await fetch(`/cards/${cardId}/comments`, {
    method: "GET",
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<CommentDto[]>;
    throw new Error(result.errorMessage || "Failed to fetch card comments");
  }

  const result = (await response.json()) as ResultDto<CommentDto[]>;
  return unwrapResult(result);
}

export async function getCardCommentsPaged(
  cardId: number,
  page: number,
  pageSize: number = 10
): Promise<PagedResultDto<CommentDto>> {
  const response = await fetch(
    `/cards/${cardId}/comments?page=${page}&pageSize=${pageSize}`
  );

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<
      PagedResultDto<CommentDto>
    >;
    throw new Error(result.errorMessage || "Failed to fetch card comments");
  }

  const result = (await response.json()) as ResultDto<
    PagedResultDto<CommentDto>
  >;
  return unwrapResult(result);
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
    const result = (await response.json()) as ResultDto<CardDetailsDto>;
    throw new Error(result.errorMessage || "Failed to update card");
  }

  const result = (await response.json()) as ResultDto<CardDetailsDto>;
  return unwrapResult(result);
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
    const result = (await response.json()) as ResultDto<CardDto>;
    throw new Error(result.errorMessage || "Failed to move card");
  }

  const result = (await response.json()) as ResultDto<CardDto>;
  return unwrapResult(result);
}

export async function assignTagsToCard(
  cardId: number,
  tagIds: number[]
): Promise<void> {
  const response = await fetch(`/cards/${cardId}/tags`, {
    method: "POST",
    body: JSON.stringify({ tags: tagIds }),
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<void>;
    throw new Error(result.errorMessage || "Failed to assign tags to card");
  }
}

export async function deleteCard(cardId: number): Promise<void> {
  const response = await fetch(`/cards/${cardId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<void>;
    throw new Error(result.errorMessage || "Failed to delete card");
  }
}

export async function createTag(
  boardId: number,
  data: TagCreateDto
): Promise<TagDto> {
  const response = await fetch("/tags", {
    method: "POST",
    body: JSON.stringify({ ...data, boardId }),
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<TagDto>;
    throw new Error(result.errorMessage || "Failed to create tag");
  }

  const result = (await response.json()) as ResultDto<TagDto>;
  return unwrapResult(result);
}

export async function updateTag(
  tagId: number,
  data: TagUpdateDto
): Promise<TagDto> {
  const response = await fetch(`/tags/${tagId}`, {
    method: "PUT",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<TagDto>;
    throw new Error(result.errorMessage || "Failed to update tag");
  }

  const result = (await response.json()) as ResultDto<TagDto>;
  return unwrapResult(result);
}

export async function deleteTag(tagId: number): Promise<void> {
  const response = await fetch(`/tags/${tagId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<void>;
    throw new Error(result.errorMessage || "Failed to delete tag");
  }
}

export async function createComment(
  cardId: number,
  content: string
): Promise<CommentDto> {
  const response = await fetch(`/cards/${cardId}/comments`, {
    method: "POST",
    body: JSON.stringify({ content }),
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<CommentDto>;
    throw new Error(result.errorMessage || "Failed to create comment");
  }

  const result = (await response.json()) as ResultDto<CommentDto>;
  return unwrapResult(result);
}

export async function updateComment(
  commentId: number,
  content: string
): Promise<CommentDto> {
  const response = await fetch(`/comments/${commentId}`, {
    method: "PUT",
    body: JSON.stringify({ content }),
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<CommentDto>;
    throw new Error(result.errorMessage || "Failed to update comment");
  }

  const result = (await response.json()) as ResultDto<CommentDto>;
  return unwrapResult(result);
}

export async function deleteComment(commentId: number): Promise<void> {
  const response = await fetch(`/comments/${commentId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const result = (await response.json()) as ResultDto<void>;
    throw new Error(result.errorMessage || "Failed to delete comment");
  }
}
