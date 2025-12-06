export const boardQueryKey = (boardId: number) => ["boards", boardId];
export const boardsQueryKey = ["boards"];

export const cardDetailsQueryKey = (cardId: number) => ["card-details", cardId];
export const cardCommentsQueryKey = (cardId: number, pageSize: number) => [
  "card-comments",
  cardId,
  pageSize,
];
