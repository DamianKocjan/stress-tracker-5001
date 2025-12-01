import { create } from "zustand";

interface KanbanStore {
  isColumnDialogCreateDialogOpen: boolean;
  setIsColumnDialogCreateDialogOpen: (isOpen: boolean) => void;
  isColumnDialogUpdateDialogOpen: boolean;
  setIsColumnDialogUpdateDialogOpen: (isOpen: boolean) => void;
  isCardDialogCreateDialogOpen: boolean;
  setIsCardDialogCreateDialogOpen: (isOpen: boolean) => void;
  /**
   * The ID of the column for which the card creation dialog is open.
   */
  columnId: number | null;
  setColumnId: (columnId: number | null) => void;
  cardId: number | null;
  setCardId: (cardId: number | null) => void;
}

export const useKanbanStore = create<KanbanStore>((set) => ({
  isColumnDialogCreateDialogOpen: false,
  setIsColumnDialogCreateDialogOpen: (isOpen: boolean) =>
    set({ isColumnDialogCreateDialogOpen: isOpen }),
  isColumnDialogUpdateDialogOpen: false,
  setIsColumnDialogUpdateDialogOpen: (isOpen: boolean) =>
    set({ isColumnDialogUpdateDialogOpen: isOpen }),
  isCardDialogCreateDialogOpen: false,
  setIsCardDialogCreateDialogOpen: (isOpen: boolean) =>
    set({ isCardDialogCreateDialogOpen: isOpen }),
  columnId: null,
  setColumnId: (columnId: number | null) => set({ columnId }),
  cardId: null,
  setCardId: (cardId: number | null) => set({ cardId }),
}));
