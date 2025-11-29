import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerHeader,
  DrawerTitle,
} from "@/components/ui/drawer";
import { type ReactElement, useCallback, useState } from "react";
import { useMediaQuery } from "./use-media-query";

interface UseConfirmProps {
  title: string;
  message: string;
}

export function useConfirm({
  title,
  message,
}: UseConfirmProps): [() => ReactElement, () => Promise<unknown>] {
  const [promise, setPromise] = useState<{
    resolve: (value: boolean) => void;
  } | null>(null);

  function confirm() {
    return new Promise((resolve) => {
      setPromise({ resolve });
    });
  }

  function handleClose() {
    setPromise(null);
  }

  function handleConfirm() {
    promise?.resolve(true);
    handleClose();
  }

  function handleCancel() {
    promise?.resolve(false);
    handleClose();
  }

  const dialog = useCallback(
    () => (
      <ConfirmationDialog
        title={title}
        message={message}
        open={promise !== null}
        onOpenChange={handleClose}
        handleConfirm={handleConfirm}
        handleCancel={handleCancel}
      />
    ),
    [handleCancel, handleConfirm, message, promise, title]
  );

  return [dialog, confirm];
}

interface ConfirmationDialogProps {
  title: string;
  message: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  handleConfirm: () => void;
  handleCancel: () => void;
}

// eslint-disable-next-line react-refresh/only-export-components
function ConfirmationDialog({
  title,
  message,
  open,
  onOpenChange,
  handleConfirm,
  handleCancel,
}: ConfirmationDialogProps) {
  const isDesktop = useMediaQuery("(min-width: 768px)");

  if (isDesktop) {
    return (
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{message}</DialogDescription>
        </DialogHeader>
        <DialogContent className="border-none hide-scrollbar max-h-[85vh] w-full overflow-y-auto p-0 sm:max-w-lg">
          <div className="flex w-full flex-col items-center justify-end gap-x-2 gap-y-2 pt-4 lg:flex-row">
            <Button
              onClick={handleCancel}
              variant="outline"
              className="w-full lg:w-auto"
            >
              Cancel
            </Button>
            <Button onClick={handleConfirm} className="w-full lg:w-auto">
              Confirm
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Drawer open={open} onOpenChange={onOpenChange}>
      <DrawerContent>
        <DrawerHeader>
          <DrawerTitle>{title}</DrawerTitle>
          <DrawerDescription>{message}</DrawerDescription>
        </DrawerHeader>
        <div className="hide-scrollbar max-h-[85vh] overflow-y-auto">
          <div className="flex w-full flex-col items-center justify-end gap-x-2 gap-y-2 pt-4 lg:flex-row">
            <Button
              onClick={handleCancel}
              variant="outline"
              className="w-full lg:w-auto"
            >
              Cancel
            </Button>
            <Button onClick={handleConfirm} className="w-full lg:w-auto">
              Confirm
            </Button>
          </div>
        </div>
      </DrawerContent>
    </Drawer>
  );
}
