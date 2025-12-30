import type { AttachmentDto } from "@/dto/attachment.dto";
import { ROLE } from "@/dto/board-member.dto";
import {
  useAttachmentDeleteMutation,
  useAttachmentUploadMutation,
} from "@/hooks/use-attachment-mutation";
import { useConfirm } from "@/hooks/use-confirm";
import { useUserBoardRole } from "@/hooks/use-user-board-role";
import { useAuth } from "@/providers/auth";
import { formatFileSize, getFileIcon, isImageFile } from "@/utils/file-icons";
import { Download, Loader2, Plus, Trash2 } from "lucide-react";
import { useRef, useState } from "react";
import { RoleGuard } from "../role-guard";
import { Button } from "../ui/button";

interface CardAttachmentsProps {
  cardId: number;
  attachments: AttachmentDto[];
}

export function CardAttachments({ cardId, attachments }: CardAttachmentsProps) {
  const { user } = useAuth();
  const userRole = useUserBoardRole();

  const fileInputRef = useRef<HTMLInputElement>(null);
  const [previewImage, setPreviewImage] = useState<string | null>(null);
  const [previewFileName, setPreviewFileName] = useState<string | null>(null);

  const uploadMutation = useAttachmentUploadMutation(cardId);
  const deleteMutation = useAttachmentDeleteMutation(cardId);
  const [ConfirmDialog, confirm] = useConfirm({
    title: "Delete attachment",
    message: "Are you sure you want to delete this file?",
  });

  const canUpload = userRole !== null && userRole > ROLE.Viewer;

  const handleFileSelect = async (file: File) => {
    if (file.size > 10 * 1024 * 1024) {
      alert("File size must be less than 10 MB");
      fileInputRef.current!.value = "";
      return;
    }

    uploadMutation.mutate(file);
  };

  const handleUploadClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      handleFileSelect(file);
    }
    // Reset input so same file can be selected again
    e.target.value = "";
  };

  const handleDeleteAttachment = async (attachment: AttachmentDto) => {
    const confirmed = await confirm();

    if (confirmed) {
      deleteMutation.mutate(attachment.id);
    }
  };

  const handleDownload = (attachment: AttachmentDto) => {
    const fileUrl = attachment.fileUrl;
    const link = document.createElement("a");
    link.href = fileUrl;
    link.download = attachment.fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const handleImagePreview = (attachment: AttachmentDto) => {
    const fileUrl = attachment.fileUrl;
    setPreviewImage(fileUrl);
    setPreviewFileName(attachment.fileName);
  };

  const canDeleteAttachment = (attachment: AttachmentDto) => {
    return (
      attachment.uploadedById === user?.id ||
      (userRole !== null && userRole >= ROLE.Admin)
    );
  };

  return (
    <div className="space-y-4">
      <ConfirmDialog />
      <div className="flex items-center justify-between">
        <h3 className="font-semibold">Attachments</h3>
        <RoleGuard minRole="Member">
          {canUpload && (
            <Button
              size="sm"
              variant="outline"
              onClick={handleUploadClick}
              disabled={uploadMutation.isPending}
              className="gap-2"
            >
              {uploadMutation.isPending ? (
                <Loader2 className="size-4 animate-spin" />
              ) : (
                <Plus className="size-4" />
              )}
              Upload
            </Button>
          )}
        </RoleGuard>
      </div>
      <input
        ref={fileInputRef}
        type="file"
        onChange={handleFileChange}
        className="hidden"
        accept=".pdf,.doc,.docx,.txt,.zip,.xlsx,.pptx,.jpg,.jpeg,.png,.gif"
      />

      {attachments.length === 0 ? (
        <div className="rounded-lg border border-dashed border-gray-300 p-6 text-center">
          <p className="text-sm text-gray-500">No attachments yet</p>
          {canUpload && (
            <p className="text-xs text-gray-400 mt-1">
              Click the upload button to add files
            </p>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
          {attachments.map((attachment) => {
            const FileIcon = getFileIcon(attachment.fileName);
            const isImage = isImageFile(attachment.fileName);

            return (
              <div
                key={attachment.id}
                className="group relative rounded-lg border border-gray-200 bg-white p-3 hover:border-blue-300 hover:shadow-sm transition-all"
              >
                {isImage ? (
                  <div className="relative h-24 bg-gray-100 rounded-md overflow-hidden mb-2 cursor-pointer group/image">
                    <img
                      src={attachment.fileUrl}
                      alt={attachment.fileName}
                      className="size-full object-cover group-hover/image:opacity-75 transition-opacity"
                      loading="lazy"
                      onClick={() => handleImagePreview(attachment)}
                    />
                    <div className="absolute inset-0 flex items-center justify-center opacity-0 group-hover/image:opacity-100 transition-opacity bg-black/50">
                      <Download className="size-5 text-white" />
                    </div>
                  </div>
                ) : (
                  <div className="h-24 bg-gray-100 rounded-md flex items-center justify-center mb-2 border border-gray-200">
                    <FileIcon className="size-8 text-gray-400" />
                  </div>
                )}

                <div className="space-y-1">
                  <p
                    className="text-sm font-medium truncate"
                    title={attachment.fileName}
                  >
                    {attachment.fileName}
                  </p>
                  <p className="text-xs text-gray-500">
                    {formatFileSize(attachment.fileSize)}
                  </p>
                  <p className="text-xs text-gray-400">
                    by {attachment.uploadedBy.username}
                  </p>
                  <p className="text-xs text-gray-400">
                    {new Date(attachment.uploadedAt).toLocaleDateString()}
                  </p>
                </div>

                <div className="absolute top-2 right-2 flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                  {!isImage && (
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => handleDownload(attachment)}
                      className="size-8 p-0"
                      title="Download"
                    >
                      <Download className="size-4" />
                    </Button>
                  )}

                  {canDeleteAttachment(attachment) && (
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => handleDeleteAttachment(attachment)}
                      disabled={deleteMutation.isPending}
                      className="size-8 p-0 text-red-500 hover:text-red-700 hover:bg-red-50"
                      title="Delete"
                    >
                      {deleteMutation.isPending ? (
                        <Loader2 className="size-4 animate-spin" />
                      ) : (
                        <Trash2 className="size-4" />
                      )}
                    </Button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Image Preview Modal */}
      {previewImage && previewFileName && (
        <div
          className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4"
          onClick={() => {
            setPreviewImage(null);
            setPreviewFileName(null);
          }}
        >
          <div
            className="relative max-w-2xl max-h-96 bg-white rounded-lg overflow-hidden"
            onClick={(e) => e.stopPropagation()}
          >
            <img
              src={previewImage}
              alt={previewFileName}
              className="size-full object-contain"
            />
            <button
              onClick={() => {
                setPreviewImage(null);
                setPreviewFileName(null);
              }}
              className="absolute top-2 right-2 bg-black/50 hover:bg-black/70 text-white rounded-full p-2"
            >
              ✕
            </button>
            <div className="absolute bottom-2 left-2 bg-black/50 text-white text-xs px-2 py-1 rounded">
              {previewFileName}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
