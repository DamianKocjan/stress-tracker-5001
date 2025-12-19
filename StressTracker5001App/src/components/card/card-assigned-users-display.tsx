import type { BoardMemberDto } from "@/dto/board-member.dto";
import type { CardAssignmentDto } from "@/dto/card-assignment.dto";
import { useBoardMembersQuery } from "@/hooks/use-board-members-query";
import { useCardAssignUserMutation } from "@/hooks/use-card-assign-user-mutation";
import { useCardUnassignUserMutation } from "@/hooks/use-card-unassign-user-mutation";
import { CheckIcon, Loader2Icon } from "lucide-react";
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
  cardId: number;
}

export function AssignedUsersDisplay({
  assignments,
  boardId,
  cardId,
}: AssignedUsersDisplayProps) {
  return (
    <RoleGuard
      minRole="Member"
      fallback={<AssignedUsers assignments={assignments} />}
    >
      <AssigneeSelector
        assignments={assignments}
        boardId={boardId}
        cardId={cardId}
      />
    </RoleGuard>
  );
}

function AssigneeSelector({
  assignments,
  boardId,
  cardId,
}: {
  assignments: CardAssignmentDto[];
  boardId: number;
  cardId: number;
}) {
  const [isOpen, setIsOpen] = useState(false);

  const { data: boardMembers } = useBoardMembersQuery(boardId);
  const assignUserMutation = useCardAssignUserMutation(boardId);
  const unassignUserMutation = useCardUnassignUserMutation(boardId);

  const assignedUserIds = useMemo(
    () => new Set(assignments.map((a) => a.user.id)),
    [assignments]
  );

  const handleAssignUser = (userId: number) => {
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
              {boardMembers?.map((member: BoardMemberDto) => (
                <CommandItem
                  key={member.user.id}
                  value={member.user.id.toString()}
                  onSelect={() =>
                    assignedUserIds.has(member.user.id)
                      ? handleUnassignUser(member.user.id)
                      : handleAssignUser(member.user.id)
                  }
                >
                  <div className="flex items-center gap-2">
                    <CheckIcon
                      className="size-4 text-secondary-foreground data-assigned:opacity-100 opacity-0"
                      data-assigned={
                        assignedUserIds.has(member.user.id) ? true : undefined
                      }
                    />

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
