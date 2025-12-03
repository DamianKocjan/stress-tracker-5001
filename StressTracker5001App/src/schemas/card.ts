import z from "zod";

export const CardFormSchema = z.object({
  title: z
    .string()
    .min(1, "Title is required")
    .max(100, "Title must be 100 characters or less"),
  description: z
    .string()
    .max(1000, "Description must be 1000 characters or less"),
  dueDate: z.iso.datetime().or(z.literal("")),
});
