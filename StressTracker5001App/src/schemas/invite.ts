import { ROLE_NAMES, ROLES } from "@/dto/board-member.dto";
import z from "zod";

const minRole = Math.min(...ROLES);
const maxRole = Math.max(...ROLES);

export const InviteSchema = z.object({
  role: z
    .number()
    .min(minRole, {
      message: `Role must be at least ${ROLE_NAMES[minRole as keyof typeof ROLE_NAMES]}`,
    })
    .max(maxRole, {
      message: `Role must be at most ${ROLE_NAMES[maxRole as keyof typeof ROLE_NAMES]}`,
    }),
});

export const JoinBoardSchema = z.object({
  token: z
    .string()
    .trim()
    .min(1, { message: "Invite token is required" })
    .min(10, { message: "Invalid invite token format" }),
});
