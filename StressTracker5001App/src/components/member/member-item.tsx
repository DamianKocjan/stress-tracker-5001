import {
  ROLE,
  ROLE_NAMES,
  type BoardMemberDto,
  type BoardMemberRoleDto,
} from "@/dto/board-member.dto";
import { useRemoveBoardMemberMutation } from "@/hooks/use-remove-board-member-mutation";
import { useUpdateMemberRoleMutation } from "@/hooks/use-update-member-role-mutation";
import { EyeIcon, ShieldCheckIcon, ShieldIcon, UserIcon } from "lucide-react";
import { RoleGuard } from "../role-guard";
import { Avatar, AvatarFallback, AvatarImage } from "../ui/avatar";
import { Badge } from "../ui/badge";
import {
  Item,
  ItemActions,
  ItemContent,
  ItemDescription,
  ItemMedia,
  ItemTitle,
} from "../ui/item";
import { Tooltip, TooltipContent, TooltipTrigger } from "../ui/tooltip";
import { MemberActionDropdown } from "./member-action-dropdown";

interface MemberProps {
  member: BoardMemberDto;
  boardId: number;
}

export function Member({ member, boardId }: MemberProps) {
  const updateRoleMutation = useUpdateMemberRoleMutation(boardId, member.id);
  const removeMemberMutation = useRemoveBoardMemberMutation(boardId, member.id);

  async function handleRoleChange(newRole: BoardMemberRoleDto) {
    await updateRoleMutation.mutateAsync({ role: newRole });
  }

  async function handleRemove() {
    await removeMemberMutation.mutateAsync();
  }

  return (
    <Item className="border-none">
      <ItemMedia>
        <Avatar>
          <AvatarImage src="" className="grayscale" />
          <AvatarFallback>
            {member.user.username.charAt(0).toUpperCase()}
          </AvatarFallback>
        </Avatar>
      </ItemMedia>
      <ItemContent className="gap-1">
        <ItemTitle className="flex items-center gap-2">
          {member.user.username}
          {member.role === ROLE.Owner && (
            <Badge className="bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-200">
              Owner
            </Badge>
          )}
        </ItemTitle>
        <ItemDescription>
          <Tooltip>
            <TooltipTrigger>{getRoleIcon(member.role)}</TooltipTrigger>
            <TooltipContent>
              <p>{ROLE_NAMES[member.role]}</p>
            </TooltipContent>
          </Tooltip>
        </ItemDescription>
      </ItemContent>

      <RoleGuard minRole="Admin">
        <ItemActions>
          <MemberActionDropdown
            member={member as BoardMemberDto}
            onRoleChange={handleRoleChange}
            onRemove={handleRemove}
            isLoading={
              updateRoleMutation.isPending || removeMemberMutation.isPending
            }
          />
        </ItemActions>
      </RoleGuard>
    </Item>
  );
}

function getRoleIcon(role: BoardMemberRoleDto) {
  switch (role) {
    case ROLE.Owner:
      return <ShieldCheckIcon className="size-4" />;
    case ROLE.Admin:
      return <ShieldIcon className="size-4" />;
    case ROLE.Member:
      return <UserIcon className="size-4" />;
    case ROLE.Viewer:
      return <EyeIcon className="size-4 opacity-50" />;
    default:
      return null;
  }
}
