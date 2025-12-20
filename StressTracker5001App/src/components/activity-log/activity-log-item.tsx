import type { ActivityLogDto } from "@/dto/activity-log.dto";
import { ActivityLogActionNames } from "@/dto/activity-log.dto";
import { cn } from "@/lib/utils";
import { getInitials } from "@/utils/get-initials";
import { getTimeAgo } from "@/utils/get-time-ago";
import {
  ArrowRightIcon,
  CheckCircle2Icon,
  EditIcon,
  LayoutIcon,
  MessageSquareIcon,
  TagIcon,
  TrashIcon,
  UserCheckIcon,
  UsersIcon,
} from "lucide-react";
import { Avatar, AvatarFallback } from "../ui/avatar";

const ENTITY_ICONS: Record<string, React.ReactNode> = {
  Comment: <MessageSquareIcon className="size-4" />,
  UserAssignment: <UserCheckIcon className="size-4" />,
  Tag: <TagIcon className="size-4" />,
  Card: <CheckCircle2Icon className="size-4" />,
  Column: <LayoutIcon className="size-4" />,
  BoardMember: <UsersIcon className="size-4" />,
  Board: <LayoutIcon className="size-4" />,
};

const ENTITY_NAMES: Record<number, string> = {
  0: "Comment",
  1: "User Assignment",
  2: "Tag",
  3: "Card",
  4: "Column",
  5: "Board Member",
  6: "Board",
};

const ACTION_COLORS: Record<
  number,
  { bg: string; text: string; border: string }
> = {
  0: { bg: "bg-green-50", text: "text-green-700", border: "border-green-200" }, // Created
  1: { bg: "bg-blue-50", text: "text-blue-700", border: "border-blue-200" }, // Updated
  2: { bg: "bg-red-50", text: "text-red-700", border: "border-red-200" }, // Deleted
  3: {
    bg: "bg-purple-50",
    text: "text-purple-700",
    border: "border-purple-200",
  }, // Moved
};

const ACTION_ICONS: Record<number, React.ReactNode> = {
  0: <CheckCircle2Icon className="size-3" />, // Created
  1: <EditIcon className="size-3" />, // Updated
  2: <TrashIcon className="size-3" />, // Deleted
  3: <ArrowRightIcon className="size-3" />, // Moved
};

interface ActivityLogItemProps {
  activity: ActivityLogDto;
}

export function ActivityLogItem({ activity }: ActivityLogItemProps) {
  const entityName = ENTITY_NAMES[activity.entityType] || "Unknown";
  const actionName = ActivityLogActionNames[activity.actionType] || "Unknown";
  const colors = ACTION_COLORS[activity.actionType] || ACTION_COLORS[0];
  const createdAtDate = new Date(activity.createdAt);

  return (
    <div
      className={cn(
        "flex gap-3 p-3 rounded-lg border transition-colors",
        "hover:bg-accent/50 border-border",
        colors.bg
      )}
    >
      <Avatar className="size-9 shrink-0">
        <AvatarFallback className="text-xs font-semibold">
          {getInitials(activity.createdBy.username)}
        </AvatarFallback>
      </Avatar>

      <div className="flex-1 min-w-0 space-y-1">
        <div className="flex items-baseline justify-between gap-2">
          <p className="text-sm font-semibold text-foreground">
            {activity.createdBy.username}
          </p>
          <div
            className={cn(
              "shrink-0 px-2 py-1 rounded-full border text-xs font-medium whitespace-nowrap",
              colors.text,
              colors.border,
              colors.bg
            )}
          >
            <div className="flex items-center gap-1">
              {ACTION_ICONS[activity.actionType]}
              <span>{actionName}</span>
            </div>
          </div>
        </div>

        <p className="text-sm text-foreground/80 line-clamp-2">
          {activity.description}
        </p>

        <div className="flex items-center gap-2 pt-1">
          {ENTITY_ICONS[entityName] && (
            <span className={cn("text-muted-foreground", colors.text)}>
              {ENTITY_ICONS[entityName]}
            </span>
          )}
          <span className="text-xs text-muted-foreground">{entityName}</span>
          <span className="text-xs text-muted-foreground/60 ml-auto">
            {getTimeAgo(createdAtDate)}
          </span>
        </div>
      </div>
    </div>
  );
}
