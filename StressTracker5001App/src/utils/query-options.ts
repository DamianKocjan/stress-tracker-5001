export const boardQueryKey = (boardId: number) => ["boards", boardId];
export const boardsQueryKey = ["boards"];

export const cardDetailsQueryKey = (cardId: number) => ["card-details", cardId];
export const cardCommentsQueryKey = (cardId: number, pageSize: number) => [
  "card-comments",
  cardId,
  pageSize,
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
