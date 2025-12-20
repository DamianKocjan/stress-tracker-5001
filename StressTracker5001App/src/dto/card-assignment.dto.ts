import type { UserDto } from "./user.dto";

export interface CardAssignmentDto {
  id: number;
  userId: number;
  user: UserDto;
  assignedAt: string;
}
