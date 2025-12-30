export const boardQueryKey = (boardId: number) => ["boards", boardId] as const;
export const boardsQueryKey = ["boards"] as const;

export const cardDetailsQueryKey = (cardId: number) =>
  ["card-details", cardId] as const;
export const cardCommentsQueryKey = (cardId: number) =>
  ["card-comments", cardId] as const;

export const boardMembershipQueryKey = (boardId: number) =>
  ["board-membership", boardId] as const;

export const boardMembersQueryKey = (boardId: number) =>
  ["board-members", boardId] as const;
export const boardInvitesQueryKey = (boardId: number) =>
  ["board-invites", boardId] as const;

export const boardActivityLogsQueryKey = (
  boardId: number,
  page: number = 1,
  pageSize: number = 10,
  entityType?: number,
  actionType?: number
) =>
  [
    "board-activity-logs",
    boardId,
    page,
    pageSize,
    entityType,
    actionType,
  ] as const;
