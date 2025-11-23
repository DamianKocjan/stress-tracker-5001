import { create } from "zustand";

interface BoardCreateDialogStore {
  isOpen: boolean;
  setIsOpen: (isOpen: boolean) => void;
}

export const useBoardCreateDialogStore = create<BoardCreateDialogStore>(
  (set) => ({
    isOpen: false,
    setIsOpen: (isOpen: boolean) => set({ isOpen }),
  })
);
