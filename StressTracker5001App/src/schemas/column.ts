import z from "zod";

export const ColumnFormSchema = z.object({
  name: z
    .string()
    .min(1, "Name is required")
    .max(100, "Name must be 100 characters or less"),
  wipLimit: z.number(),
});
