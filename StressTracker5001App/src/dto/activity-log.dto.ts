import type { UserDto } from "./user.dto";

/**
 * Define activity log entity types
 * (0 - Comment, 1 - User Assignment, 2 - Tag, 3 - Card, 4 - Column, 5 - Board Member, 6 - Board)
 */
export type ActivityLogEntityTypeDto = 0 | 1 | 2 | 3 | 4 | 5 | 6;

/**
 * Define activity log action types
 * (0 - Created, 1 - Updated, 2 - Deleted, 3 - Moved)
 */
export type ActivityLogActionTypeDto = 0 | 1 | 2 | 3;

export const ActivityLogEntityType: Record<string, ActivityLogEntityTypeDto> = {
  Comment: 0,
  UserAssignment: 1,
  Tag: 2,
  Card: 3,
  Column: 4,
  BoardMember: 5,
  Board: 6,
};

export const ActivityLogActionType: Record<string, ActivityLogActionTypeDto> = {
  Created: 0,
  Updated: 1,
  Deleted: 2,
  Moved: 3,
};

export const ActivityLogActionNames: Record<ActivityLogActionTypeDto, string> =
  {
    0: "Created",
    1: "Updated",
    2: "Deleted",
    3: "Moved",
  };

export interface ActivityLogDto {
  id: number;
  boardId: number;
  entityType: ActivityLogEntityTypeDto;
  actionType: ActivityLogActionTypeDto;
  entityId: number;
  description: string;
  createdBy: UserDto;
  createdAt: string; // ISO date string
}
