import type { BoardInviteDto } from "@/dto/board-invite.dto";
import { ROLE_NAMES, type BoardMemberRoleDto } from "@/dto/board-member.dto";
import { useRevokeAllInvitesMutation } from "@/hooks/use-revoke-all-invites-mutation";
import { useRevokeInviteMutation } from "@/hooks/use-revoke-invite-mutation";
import { CheckIcon, CopyIcon, TrashIcon } from "lucide-react";
import React, { useState } from "react";
import { RoleGuard } from "../role-guard";
import { Avatar, AvatarFallback } from "../ui/avatar";
import { Button } from "../ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "../ui/dialog";
import {
  Item,
  ItemActions,
  ItemContent,
  ItemDescription,
  ItemMedia,
  ItemSeparator,
  ItemTitle,
} from "../ui/item";
import { Tooltip, TooltipContent, TooltipTrigger } from "../ui/tooltip";

interface InvitesListProps {
  invites: BoardInviteDto[];
  boardId: number;
}

export function InvitesList({ invites, boardId }: InvitesListProps) {
  const revokeInviteMutation = useRevokeInviteMutation(boardId);
  const revokeAllMutation = useRevokeAllInvitesMutation(boardId);
  const [copiedId, setCopiedId] = useState<number | null>(null);
  const [revokeAllOpen, setRevokeAllOpen] = useState(false);

  const handleCopy = (token: string, id: number) => {
    navigator.clipboard.writeText(token);
    setCopiedId(id);
    setTimeout(() => setCopiedId(null), 2000);
  };

  const handleRevokeInvite = (inviteId: number) => {
    revokeInviteMutation.mutate(inviteId);
  };

  const handleRevokeAll = () => {
    revokeAllMutation.mutate();
    setRevokeAllOpen(false);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  const getRoleColor = (role: BoardMemberRoleDto) => {
    switch (role) {
      case 3:
        return "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-100";
      case 2:
        return "bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-100";
      case 1:
        return "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-100";
      case 0:
        return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-100";
      default:
        return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-100";
    }
  };

  if (invites.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center gap-4 rounded-lg border border-dashed border-muted-foreground/25 px-4 py-8">
        <p className="text-center text-sm text-muted-foreground">
          No active invites
        </p>
      </div>
    );
  }

  return (
    <RoleGuard minRole="Admin">
      <div className="flex w-full flex-col gap-6">
        <div className="flex items-center justify-between gap-2">
          <p className="text-sm font-medium text-muted-foreground">
            {invites.length} active{" "}
            {invites.length === 1 ? "invite" : "invites"}
          </p>
          <Dialog open={revokeAllOpen} onOpenChange={setRevokeAllOpen}>
            <DialogTrigger asChild>
              <Button variant="outline" size="sm" className="text-destructive">
                <TrashIcon className="mr-2 size-4" />
                Revoke All
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Revoke all invites?</DialogTitle>
                <DialogDescription>
                  This will revoke all active invites for this board. Users will
                  not be able to join using existing invite links.
                </DialogDescription>
              </DialogHeader>
              <div className="flex justify-end gap-2">
                <Button
                  variant="outline"
                  onClick={() => setRevokeAllOpen(false)}
                >
                  Cancel
                </Button>
                <Button
                  variant="destructive"
                  onClick={handleRevokeAll}
                  disabled={revokeAllMutation.isPending}
                >
                  {revokeAllMutation.isPending ? "Revoking..." : "Revoke All"}
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        </div>

        <div className="flex w-full flex-col gap-0">
          {invites.map((invite, index) => (
            <React.Fragment key={invite.id}>
              <Item className="border-none">
                <ItemMedia>
                  <Avatar>
                    <AvatarFallback>
                      {invite.generatedByUser.username.charAt(0).toUpperCase()}
                    </AvatarFallback>
                  </Avatar>
                </ItemMedia>
                <ItemContent>
                  <ItemTitle className="text-sm">
                    {invite.generatedByUser.username}
                  </ItemTitle>
                  <ItemDescription className="flex flex-col gap-2 text-xs">
                    <span className="font-mono text-muted-foreground">
                      {invite.token.substring(0, 8)}...
                    </span>
                    <span className="text-muted-foreground">
                      Created {formatDate(invite.createdAt)}
                    </span>
                  </ItemDescription>
                </ItemContent>
                <ItemActions className="flex gap-1">
                  <span
                    className={`inline-flex items-center rounded-md px-2 py-1 text-xs font-medium ${getRoleColor(invite.role)}`}
                  >
                    {ROLE_NAMES[invite.role]}
                  </span>
                  <Tooltip>
                    <TooltipTrigger asChild>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleCopy(invite.token, invite.id)}
                        className="size-8"
                      >
                        {copiedId === invite.id ? (
                          <CheckIcon className="size-4" />
                        ) : (
                          <CopyIcon className="size-4" />
                        )}
                      </Button>
                    </TooltipTrigger>
                    <TooltipContent>
                      {copiedId === invite.id ? "Copied!" : "Copy invite link"}
                    </TooltipContent>
                  </Tooltip>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleRevokeInvite(invite.id)}
                    disabled={revokeInviteMutation.isPending}
                    className="size-8 text-destructive hover:bg-destructive/10 hover:text-destructive"
                  >
                    <TrashIcon className="size-4" />
                  </Button>
                </ItemActions>
              </Item>
              {index !== invites.length - 1 && <ItemSeparator />}
            </React.Fragment>
          ))}
        </div>
      </div>
    </RoleGuard>
  );
}
