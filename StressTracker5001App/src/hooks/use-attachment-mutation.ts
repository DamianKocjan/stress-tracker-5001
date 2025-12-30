import type { CardDetailsDto } from "@/dto/card.dto";
import { deleteAttachment, uploadAttachment } from "@/utils/api";
import { cardDetailsQueryKey } from "@/utils/query-options";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useAttachmentUploadMutation(cardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (file: File) => uploadAttachment(cardId, file),
    onSuccess(data) {
      toast.success("File uploaded successfully!", {
        description: `${data.fileName} has been uploaded.`,
      });

      // Update card details with new attachment
      queryClient.setQueryData<CardDetailsDto>(
        cardDetailsQueryKey(cardId),
        (oldData) => {
          if (!oldData) {
            return;
          }

          return {
            ...oldData,
            attachments: [...(oldData.attachments || []), data],
          };
        }
      );
    },
    onError(error) {
      toast.error("Failed to upload file", {
        description: error.message,
      });
    },
  });
}

export function useAttachmentDeleteMutation(cardId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (attachmentId: string) => deleteAttachment(attachmentId),
    onSuccess(_, attachmentId) {
      toast.success("File deleted successfully!");

      // Update card details by removing the attachment
      queryClient.setQueryData<CardDetailsDto>(
        cardDetailsQueryKey(cardId),
        (oldData) => {
          if (!oldData) {
            return;
          }

          return {
            ...oldData,
            attachments: oldData.attachments.filter(
              (a) => a.id !== attachmentId
            ),
          };
        }
      );
    },
    onError(error) {
      toast.error("Failed to delete file", {
        description: error.message,
      });
    },
  });
}
