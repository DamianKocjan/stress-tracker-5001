import type { BoardMemberDto } from "@/dto/board-member.dto";
import type { CardAssignmentDto } from "@/dto/card-assignment.dto";
import { useBoardMembersQuery } from "@/hooks/use-board-members-query";
import { useCardAssignUserMutation } from "@/hooks/use-card-assign-user-mutation";
import { useCardUnassignUserMutation } from "@/hooks/use-card-unassign-user-mutation";
import { useKanbanStore } from "@/stores/kanban-store";
import { Loader2Icon } from "lucide-react";
import { useMemo, useState } from "react";
import { RoleGuard } from "../role-guard";
import { Avatar, AvatarFallback, AvatarGroup } from "../ui/avatar";
import { Button } from "../ui/button";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "../ui/command";
import { Popover, PopoverContent, PopoverTrigger } from "../ui/popover";
import { Tooltip, TooltipContent, TooltipTrigger } from "../ui/tooltip";

interface AssignedUsersDisplayProps {
  assignments: CardAssignmentDto[];
  boardId: number;
}

export function AssignedUsersDisplay({
  assignments,
  boardId,
}: AssignedUsersDisplayProps) {
  return (
    <RoleGuard
      minRole="Member"
      fallback={<AssignedUsers assignments={assignments} />}
    >
      <AssigneeSelector assignments={assignments} boardId={boardId} />
    </RoleGuard>
  );
}

function AssigneeSelector({
  assignments,
  boardId,
}: {
  assignments: CardAssignmentDto[];
  boardId: number;
}) {
  const [isOpen, setIsOpen] = useState(false);
  const cardId = useKanbanStore((state) => state.cardId);

  const { data: boardMembers } = useBoardMembersQuery(boardId);
  const assignUserMutation = useCardAssignUserMutation(boardId);
  const unassignUserMutation = useCardUnassignUserMutation(boardId);

  const assignedUserIds = useMemo(
    () => new Set(assignments.map((a) => a.user.id)),
    [assignments]
  );

  const availableUsers = useMemo(
    () => boardMembers?.filter((m) => !assignedUserIds.has(m.user.id)) ?? [],
    [boardMembers, assignedUserIds]
  );

  const handleAssignUser = (userId: number) => {
    if (!cardId) return;
    assignUserMutation.mutate(
      { cardId, userId },
      {
        onSuccess: () => {
          setIsOpen(false);
        },
      }
    );
  };

  const handleUnassignUser = (userId: number) => {
    if (!cardId) return;
    unassignUserMutation.mutate({ cardId, userId });
  };

  const isLoading =
    assignUserMutation.isPending || unassignUserMutation.isPending;

  return (
    <Popover open={isOpen} onOpenChange={setIsOpen}>
      <PopoverTrigger asChild>
        <Button
          className="h-auto w-full justify-between p-2"
          variant="outline"
          disabled={isLoading}
        >
          <div className="flex items-center gap-2">
            {assignments.length > 0 ? (
              <AssignedUsers assignments={assignments} />
            ) : (
              <span className="text-muted-foreground text-sm">
                No assignees...
              </span>
            )}
          </div>
          {isLoading && <Loader2Icon className="size-4 animate-spin" />}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-48 p-0">
        <Command>
          <CommandInput placeholder="Search users..." />
          <CommandList>
            <CommandEmpty>No users available to assign.</CommandEmpty>
            <CommandGroup>
              {availableUsers.map((member: BoardMemberDto) => (
                <CommandItem
                  key={member.user.id}
                  value={member.user.id.toString()}
                  onSelect={() => handleAssignUser(member.user.id)}
                >
                  <div className="flex items-center gap-2">
                    <Avatar className="size-6">
                      <AvatarFallback className="text-xs">
                        {member.user.username.charAt(0).toUpperCase()}
                      </AvatarFallback>
                    </Avatar>
                    {member.user.username}
                  </div>
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>

        {assignments.length > 0 && (
          <>
            <div className="border-t border-border" />
            <CommandGroup className="p-2">
              <p className="mb-2 text-xs font-semibold text-muted-foreground">
                Assigned Users
              </p>
              {assignments.map((assignment) => (
                <div
                  key={assignment.id}
                  className="flex items-center justify-between gap-2 rounded px-2 py-1.5 text-sm hover:bg-accent"
                >
                  <div className="flex items-center gap-2">
                    <Avatar className="size-5">
                      <AvatarFallback className="text-xs">
                        {assignment.user.username.charAt(0).toUpperCase()}
                      </AvatarFallback>
                    </Avatar>
                    {assignment.user.username}
                  </div>
                  <button
                    type="button"
                    onClick={() => handleUnassignUser(assignment.user.id)}
                    className="text-muted-foreground hover:text-foreground"
                    disabled={isLoading}
                  >
                    ✕
                  </button>
                </div>
              ))}
            </CommandGroup>
          </>
        )}
      </PopoverContent>
    </Popover>
  );
}

function AssignedUsers({ assignments }: { assignments: CardAssignmentDto[] }) {
  return (
    <AvatarGroup>
      {assignments.map((assignment) => (
        <Tooltip key={assignment.id}>
          <TooltipTrigger asChild>
            <Avatar className="size-6">
              <AvatarFallback className="text-xs">
                {assignment.user.username.charAt(0).toUpperCase()}
              </AvatarFallback>
            </Avatar>
          </TooltipTrigger>

          <TooltipContent>
            <p>{assignment.user.username}</p>
          </TooltipContent>
        </Tooltip>
      ))}
    </AvatarGroup>
  );
}
