import type { CardDto } from "./card.dto";
import type { ColumnDto } from "./column.dto";
import type { TagDto } from "./tag.dto";
import type { UserDto } from "./user.dto";

export interface BoardCreateDto {
  name: string;
  description: string;
}

export interface BoardUpdateDto {
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

export interface BoardDetailsDto extends BoardDto {
  columns: ColumnDto[];
  cards: CardDto[];
  tags: TagDto[];
}
