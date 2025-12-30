import type { ActivityLogDto } from "@/dto/activity-log.dto";
import type { AttachmentDto } from "@/dto/attachment.dto";
import type {
  ConfirmEmailChangeDto,
  ConfirmPasswordResetDto,
  DeleteAccountDto,
  RequestEmailChangeDto,
  RequestPasswordResetDto,
  ResendVerificationEmailDto,
  UpdatePasswordDto,
} from "@/dto/auth.dto";
import type {
  BoardInviteCreateDto,
  BoardInviteDto,
} from "@/dto/board-invite.dto";
import type {
  BoardMemberCreateDto,
  BoardMemberDto,
  BoardMemberUpdateDto,
} from "@/dto/board-member.dto";
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
import type { UserUpdateDto } from "@/dto/user.dto";
import { fetch } from "./fetch";

async function handleResponseError(response: Response, genericMessage: string) {
  try {
    const result = (await response.json()) as ResultDto<unknown>;
    const errorMessage = result.errorMessage || genericMessage;
    return new Error(errorMessage);
  } catch {
    return new Error(genericMessage);
  }
}

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
    throw await handleResponseError(response, "Failed to create board");
  }

  const result = (await response.json()) as ResultDto<BoardDto>;
  return unwrapResult(result);
}

export async function getBoards(): Promise<BoardDto[]> {
  const response = await fetch("/boards", {
    method: "GET",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to fetch boards");
  }

  const result = (await response.json()) as ResultDto<BoardDto[]>;
  return unwrapResult(result);
}

export async function getBoard(boardId: number): Promise<BoardDetailsDto> {
  const response = await fetch(`/boards/${boardId}`, {
    method: "GET",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to fetch board");
  }

  const result = (await response.json()) as ResultDto<BoardDetailsDto>;
  return unwrapResult(result);
}

export async function getBoardMembership(
  boardId: number
): Promise<BoardMemberDto> {
  const response = await fetch(`/boards/${boardId}/membership`, {
    method: "GET",
  });

  if (!response.ok) {
    throw await handleResponseError(
      response,
      "Failed to fetch board membership"
    );
  }

  const result = (await response.json()) as ResultDto<BoardMemberDto>;
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
    throw await handleResponseError(response, "Failed to update board");
  }

  const result = (await response.json()) as ResultDto<BoardDto>;
  return unwrapResult(result);
}

export async function deleteBoard(boardId: number): Promise<void> {
  const response = await fetch(`/boards/${boardId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to delete board");
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
    throw await handleResponseError(response, "Failed to create column");
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
    throw await handleResponseError(response, "Failed to update column");
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
    throw await handleResponseError(response, "Failed to move column");
  }

  const result = (await response.json()) as ResultDto<ColumnDto>;
  return unwrapResult(result);
}

export async function deleteColumn(columnId: number): Promise<void> {
  const response = await fetch(`/columns/${columnId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to delete column");
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
    throw await handleResponseError(response, "Failed to create card");
  }

  const result = (await response.json()) as ResultDto<CardDto>;
  return unwrapResult(result);
}

export async function getCardDetails(cardId: number): Promise<CardDetailsDto> {
  const response = await fetch(`/cards/${cardId}`, {
    method: "GET",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to fetch card details");
  }

  const result = (await response.json()) as ResultDto<CardDetailsDto>;
  return unwrapResult(result);
}

export async function getCardComments(cardId: number): Promise<CommentDto[]> {
  const response = await fetch(`/cards/${cardId}/comments`, {
    method: "GET",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to fetch card comments");
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
    throw await handleResponseError(response, "Failed to fetch card comments");
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
    throw await handleResponseError(response, "Failed to update card");
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
    throw await handleResponseError(response, "Failed to move card");
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
    throw await handleResponseError(response, "Failed to assign tags to card");
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
    throw await handleResponseError(response, "Failed to create tag");
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
    throw await handleResponseError(response, "Failed to update tag");
  }

  const result = (await response.json()) as ResultDto<TagDto>;
  return unwrapResult(result);
}

export async function deleteTag(tagId: number): Promise<void> {
  const response = await fetch(`/tags/${tagId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to delete tag");
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
    throw await handleResponseError(response, "Failed to create comment");
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
    throw await handleResponseError(response, "Failed to update comment");
  }

  const result = (await response.json()) as ResultDto<CommentDto>;
  return unwrapResult(result);
}

export async function deleteComment(commentId: number): Promise<void> {
  const response = await fetch(`/comments/${commentId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to delete comment");
  }
}

// Board Member API Functions
export async function getBoardMembers(
  boardId: number
): Promise<BoardMemberDto[]> {
  const response = await fetch(`/boards/${boardId}/members`, {
    method: "GET",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to fetch board members");
  }

  const result = (await response.json()) as ResultDto<BoardMemberDto[]>;
  return unwrapResult(result);
}

export async function addBoardMember(
  boardId: number,
  data: BoardMemberCreateDto
): Promise<BoardMemberDto> {
  const response = await fetch(`/boards/${boardId}/members`, {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to add board member");
  }

  const result = (await response.json()) as ResultDto<BoardMemberDto>;
  return unwrapResult(result);
}

export async function updateMemberRole(
  boardId: number,
  memberId: number,
  data: BoardMemberUpdateDto
): Promise<BoardMemberDto> {
  const response = await fetch(`/boards/${boardId}/members/${memberId}`, {
    method: "PATCH",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to update member role");
  }

  const result = (await response.json()) as ResultDto<BoardMemberDto>;
  return unwrapResult(result);
}

export async function removeBoardMember(
  boardId: number,
  memberId: number
): Promise<void> {
  const response = await fetch(`/boards/${boardId}/members/${memberId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to remove board member");
  }
}

// Board Invite API Functions
export async function getBoardInvites(
  boardId: number
): Promise<BoardInviteDto[]> {
  const response = await fetch(`/boards/${boardId}/invites`, {
    method: "GET",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to fetch board invites");
  }

  const result = (await response.json()) as ResultDto<BoardInviteDto[]>;
  return unwrapResult(result);
}

export async function generateBoardInvite(
  boardId: number,
  data: BoardInviteCreateDto
): Promise<BoardInviteDto> {
  const response = await fetch(`/boards/${boardId}/invites`, {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw await handleResponseError(
      response,
      "Failed to generate board invite"
    );
  }

  const result = (await response.json()) as ResultDto<BoardInviteDto>;
  return unwrapResult(result);
}

export async function revokeInvite(inviteId: number): Promise<void> {
  const response = await fetch(`/invites/${inviteId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to revoke invite");
  }
}

export async function revokeAllBoardInvites(boardId: number): Promise<void> {
  const response = await fetch(`/boards/${boardId}/revoke-invites`, {
    method: "POST",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to revoke all invites");
  }
}

export async function assignUserToCard(
  cardId: number,
  userId: number
): Promise<void> {
  const response = await fetch(`/cards/${cardId}/assign-user`, {
    method: "POST",
    body: JSON.stringify({ userId }),
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to assign user to card");
  }
}

export async function unassignUserFromCard(
  cardId: number,
  userId: number
): Promise<void> {
  const response = await fetch(`/cards/${cardId}/assign-user`, {
    method: "DELETE",
    body: JSON.stringify({ userId }),
  });

  if (!response.ok) {
    throw await handleResponseError(
      response,
      "Failed to unassign user from card"
    );
  }
}

export async function joinBoardWithInvite(
  token: string
): Promise<BoardDetailsDto> {
  const response = await fetch(`/boardinvite/join`, {
    method: "POST",
    body: JSON.stringify({ token }),
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to join board");
  }

  const result = (await response.json()) as ResultDto<BoardDetailsDto>;
  return unwrapResult(result);
}

export async function getBoardActivityLogs(
  boardId: number,
  params?: {
    page?: number;
    pageSize?: number;
    entityType?: number;
    actionType?: number;
  }
): Promise<PagedResultDto<ActivityLogDto>> {
  const searchParams = new URLSearchParams();
  if (params?.page) searchParams.append("page", params.page.toString());
  if (params?.pageSize)
    searchParams.append("pageSize", params.pageSize.toString());
  if (params?.entityType !== undefined)
    searchParams.append("entityType", params.entityType.toString());
  if (params?.actionType !== undefined)
    searchParams.append("actionType", params.actionType.toString());

  const queryString = searchParams.toString();
  const url = `/boards/${boardId}/activity-logs${queryString ? `?${queryString}` : ""}`;

  const response = await fetch(url, {
    method: "GET",
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to fetch activity logs");
  }

  const result = (await response.json()) as ResultDto<
    PagedResultDto<ActivityLogDto>
  >;
  return unwrapResult(result);
}

// Auth API Functions
export async function requestPasswordReset(
  data: RequestPasswordResetDto
): Promise<void> {
  const response = await fetch("/auth/request-password-reset", {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw await handleResponseError(
      response,
      "Failed to request password reset"
    );
  }
}

export async function confirmPasswordReset(
  data: ConfirmPasswordResetDto
): Promise<void> {
  const response = await fetch("/auth/confirm-password-reset", {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to reset password");
  }
}

export async function requestEmailChange(
  data: RequestEmailChangeDto
): Promise<void> {
  const response = await fetch("/auth/request-email-change", {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to request email change");
  }
}

export async function confirmEmailChange(
  data: ConfirmEmailChangeDto
): Promise<void> {
  const response = await fetch("/auth/confirm-email-change", {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to verify email");
  }
}

export async function updateProfile(
  data: UserUpdateDto
): Promise<{ message: string }> {
  const response = await fetch("/auth/profile/update", {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to update profile");
  }

  const result = (await response.json()) as ResultDto<{ message: string }>;
  return unwrapResult(result);
}

export async function updatePassword(
  data: UpdatePasswordDto
): Promise<{ message: string }> {
  const response = await fetch("/auth/profile/update-password", {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to update password");
  }

  const result = (await response.json()) as ResultDto<{ message: string }>;
  return unwrapResult(result);
}

export async function deleteAccount(data: DeleteAccountDto): Promise<void> {
  const response = await fetch("/auth/delete-account", {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to delete account");
  }
}

export async function resendVerificationEmail(
  data: ResendVerificationEmailDto
): Promise<void> {
  const response = await fetch("/auth/resend-verification-email", {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw await handleResponseError(response, "Failed to resend email");
  }
}
