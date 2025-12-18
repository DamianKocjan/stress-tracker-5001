import { create } from "zustand";

interface JoinBoardDialogStore {
  isOpen: boolean;
  setIsOpen: (isOpen: boolean) => void;
}

export const useJoinBoardDialogStore = create<JoinBoardDialogStore>((set) => ({
  isOpen: false,
  setIsOpen: (isOpen: boolean) => set({ isOpen }),
}));
