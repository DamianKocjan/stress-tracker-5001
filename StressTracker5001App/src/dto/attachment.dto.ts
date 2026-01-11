import type { UserDto } from "./user.dto";

export interface AttachmentDto {
  id: string;
  cardId: number;
  fileName: string;
  contentType: string;
  fileSize: number;
  uploadedById: number;
  uploadedBy: UserDto;
  uploadedAt: string;
  fileUrl: string;
}
