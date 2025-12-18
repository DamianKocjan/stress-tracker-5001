import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  ROLE,
  ROLE_NAMES,
  ROLES,
  type BoardMemberDto,
  type BoardMemberRoleDto,
} from "@/dto/board-member.dto";
import { useUserBoardRole } from "@/hooks/use-user-board-role";
import {
  EyeIcon,
  Loader2Icon,
  MoreHorizontalIcon,
  ShieldCheckIcon,
  ShieldIcon,
  Trash2Icon,
  UserIcon,
} from "lucide-react";
import { useState } from "react";

interface MemberActionDropdownProps {
  member: BoardMemberDto;
  onRoleChange: (newRole: BoardMemberRoleDto) => Promise<void>;
  onRemove: (memberId: number) => Promise<void>;
  isLoading: boolean;
}

export function MemberActionDropdown({
  member,
  onRoleChange,
  onRemove,
  isLoading = false,
}: MemberActionDropdownProps) {
  const [showRemoveDialog, setShowRemoveDialog] = useState(false);
  const [isRemoving, setIsRemoving] = useState(false);
  const userRole = useUserBoardRole();

  const handleRoleChange = async (newRole: BoardMemberRoleDto) => {
    if (onRoleChange && newRole !== member.role) {
      try {
        await onRoleChange(newRole);
      } catch (error) {
        console.error("Failed to change member role:", error);
      }
    }
  };

  const handleRemove = async () => {
    if (onRemove) {
      setIsRemoving(true);
      try {
        await onRemove(member.id);
        setShowRemoveDialog(false);
      } catch (error) {
        console.error("Failed to remove member:", error);
        setIsRemoving(false);
      }
    }
  };

  return (
    <>
      <DropdownMenu modal={false}>
        <DropdownMenuTrigger asChild>
          <Button
            variant="ghost"
            aria-label="Member actions"
            size="icon-sm"
            disabled={isLoading}
          >
            <MoreHorizontalIcon className="h-4 w-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent className="w-56" align="end">
          <DropdownMenuLabel>{member.user.username}</DropdownMenuLabel>
          <DropdownMenuSeparator />

          <DropdownMenuLabel className="text-xs font-normal text-muted-foreground">
            Change Role
          </DropdownMenuLabel>
          <DropdownMenuGroup>
            {ROLES.map((role) => (
              <DropdownMenuCheckboxItem
                key={role}
                checked={member.role === role}
                onCheckedChange={() => handleRoleChange(role)}
                disabled={
                  isLoading ||
                  // Only Owners can assign the Owner role
                  (role === ROLE.Owner && userRole === ROLE.Admin)
                }
              >
                <div className="flex items-center gap-2">
                  {role === ROLE.Owner && (
                    <ShieldCheckIcon className="size-4" />
                  )}
                  {role === ROLE.Admin && <ShieldIcon className="size-4" />}
                  {role === ROLE.Member && <UserIcon className="size-4" />}
                  {role === ROLE.Viewer && (
                    <EyeIcon className="size-4 opacity-50" />
                  )}
                  <span>{ROLE_NAMES[role]}</span>
                </div>
              </DropdownMenuCheckboxItem>
            ))}
          </DropdownMenuGroup>

          <DropdownMenuSeparator />

          <DropdownMenuItem
            onSelect={() => setShowRemoveDialog(true)}
            disabled={isLoading}
            className="text-destructive focus:bg-destructive/10 focus:text-destructive"
          >
            <Trash2Icon className="mr-2 size-4" />
            Remove Member
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>

      <Dialog open={showRemoveDialog} onOpenChange={setShowRemoveDialog}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Remove Member</DialogTitle>
            <DialogDescription>
              Are you sure you want to remove{" "}
              <span className="font-semibold">{member.user.username}</span> from
              this board? This action cannot be undone.
            </DialogDescription>
          </DialogHeader>

          <DialogFooter>
            <DialogClose asChild>
              <Button variant="outline" disabled={isRemoving}>
                Cancel
              </Button>
            </DialogClose>
            <Button
              variant="destructive"
              onClick={handleRemove}
              disabled={isRemoving}
            >
              {isRemoving ? (
                <>
                  <Loader2Icon className="mr-2 size-4 animate-spin" />
                  Removing...
                </>
              ) : (
                "Remove Member"
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
