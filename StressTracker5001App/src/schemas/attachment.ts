import { z } from "zod";

export const AttachmentSchema = z.object({
  id: z.uuid(),
  cardId: z.number(),
  fileName: z.string(),
  contentType: z.string(),
  fileSize: z.number(),
  uploadedById: z.number(),
  uploadedAt: z.string().datetime(),
  fileUrl: z.url(),
});
