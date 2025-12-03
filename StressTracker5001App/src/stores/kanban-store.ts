import { create } from "zustand";

interface KanbanStore {
  isColumnCreateDialogOpen: boolean;
  setIsColumnCreateDialogOpen: (isOpen: boolean) => void;
  isColumnUpdateDialogOpen: boolean;
  setIsColumnUpdateDialogOpen: (isOpen: boolean) => void;
  isCardCreateDialogOpen: boolean;
  setIsCardCreateDialogOpen: (isOpen: boolean) => void;
  /**
   * The ID of the column for which the card creation dialog is open.
   */
  columnId: number | null;
  setColumnId: (columnId: number | null) => void;
  cardId: number | null;
  setCardId: (cardId: number | null) => void;
}

export const useKanbanStore = create<KanbanStore>((set) => ({
  isColumnCreateDialogOpen: false,
  setIsColumnCreateDialogOpen: (isOpen: boolean) =>
    set({ isColumnCreateDialogOpen: isOpen }),
  isColumnUpdateDialogOpen: false,
  setIsColumnUpdateDialogOpen: (isOpen: boolean) =>
    set({ isColumnUpdateDialogOpen: isOpen }),
  isCardCreateDialogOpen: false,
  setIsCardCreateDialogOpen: (isOpen: boolean) =>
    set({ isCardCreateDialogOpen: isOpen }),
  columnId: null,
  setColumnId: (columnId: number | null) => set({ columnId }),
  cardId: null,
  setCardId: (cardId: number | null) => set({ cardId }),
}));
