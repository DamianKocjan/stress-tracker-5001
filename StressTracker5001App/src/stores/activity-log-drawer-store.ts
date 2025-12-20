import { create } from "zustand";

interface ActivityLogDrawerStore {
  isOpen: boolean;
  boardId: number | null;
  page: number;
  pageSize: number;
  entityTypeFilter: number | undefined;
  actionTypeFilter: number | undefined;
  openDrawer: (boardId: number) => void;
  closeDrawer: () => void;
  setPage: (page: number) => void;
  setEntityTypeFilter: (entityType: number | undefined) => void;
  setActionTypeFilter: (actionType: number | undefined) => void;
  resetFilters: () => void;
}

export const useActivityLogDrawerStore = create<ActivityLogDrawerStore>(
  (set) => ({
    isOpen: false,
    boardId: null,
    page: 1,
    pageSize: 10,
    entityTypeFilter: undefined,
    actionTypeFilter: undefined,
    openDrawer: (boardId: number) =>
      set({
        isOpen: true,
        boardId,
        page: 1,
        entityTypeFilter: undefined,
        actionTypeFilter: undefined,
      }),
    closeDrawer: () =>
      set({
        isOpen: false,
        boardId: null,
      }),
    setPage: (page: number) => set({ page }),
    setEntityTypeFilter: (entityType: number | undefined) =>
      set({ entityTypeFilter: entityType, page: 1 }),
    setActionTypeFilter: (actionType: number | undefined) =>
      set({ actionTypeFilter: actionType, page: 1 }),
    resetFilters: () =>
      set({
        page: 1,
        entityTypeFilter: undefined,
        actionTypeFilter: undefined,
      }),
  })
);
