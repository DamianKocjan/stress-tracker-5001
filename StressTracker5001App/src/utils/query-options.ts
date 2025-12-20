export const boardQueryKey = (boardId: number) => ["boards", boardId];
export const boardsQueryKey = ["boards"];

export const cardDetailsQueryKey = (cardId: number) => ["card-details", cardId];
export const cardCommentsQueryKey = (cardId: number) => [
  "card-comments",
  cardId,
];

export const boardMembershipQueryKey = (boardId: number) => [
  "board-membership",
  boardId,
];

export const boardMembersQueryKey = (boardId: number) => [
  "board-members",
  boardId,
];
export const boardInvitesQueryKey = (boardId: number) => [
  "board-invites",
  boardId,
];

export const boardActivityLogsQueryKey = (
  boardId: number,
  page: number = 1,
  pageSize: number = 10,
  entityType?: number,
  actionType?: number
) => ["board-activity-logs", boardId, page, pageSize, entityType, actionType];
