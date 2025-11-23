import type { UserDto } from "./user.dto";

export interface BoardCreateDto {
  name: string;
  description: string;
}

export interface BoardDto {
  id: number;
  name: string;
  description: string;
  ownerId: number;
  owner: UserDto;
  createdAt: string;
  updatedAt: string;
}
