import { useBoardMembersQuery } from "@/hooks/use-board-members-query";
import { UsersIcon } from "lucide-react";
import React from "react";
import { FetchingErrorAlert } from "../fetching-error-alert";
import { BoardInvitesSection } from "../invite/board-invites-section";
import { CreateInvite } from "../invite/create-invite";
import { RoleGuard } from "../role-guard";
import { Button } from "../ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "../ui/dialog";
import {
  Item,
  ItemActions,
  ItemContent,
  ItemGroup,
  ItemMedia,
  ItemSeparator,
} from "../ui/item";
import { Skeleton } from "../ui/skeleton";
import { Member } from "./member-item";
import { MembersEmptyState } from "./members-empty-state";

interface MembersDialogProps {
  boardId: number;
}

export function MembersDialog({ boardId }: MembersDialogProps) {
  const { data, status, error, refetch } = useBoardMembersQuery(boardId);

  return (
    <RoleGuard minRole="Member">
      <Dialog>
        <DialogTrigger asChild>
          <Button variant="outline" size="sm">
            <UsersIcon className="mr-2 size-4" />
            Members
          </Button>
        </DialogTrigger>
        <DialogContent className="max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Board Members</DialogTitle>
            <DialogDescription>
              Manage members and their roles on this board
            </DialogDescription>
          </DialogHeader>

          {status === "pending" ? (
            <div className="flex w-full flex-col gap-6">
              <ItemGroup>
                {Array.from({ length: 3 }).map((_, i) => (
                  <React.Fragment key={i}>
                    <Item>
                      <ItemMedia>
                        <Skeleton className="size-10 rounded-full" />
                      </ItemMedia>
                      <ItemContent className="gap-1">
                        <Skeleton className="h-4 w-24" />
                        <Skeleton className="h-3 w-32" />
                      </ItemContent>
                      <ItemActions>
                        <Skeleton className="size-8 rounded" />
                      </ItemActions>
                    </Item>
                    {i !== 2 && <ItemSeparator />}
                  </React.Fragment>
                ))}
              </ItemGroup>
            </div>
          ) : status === "error" ? (
            <FetchingErrorAlert
              title="Failed to load members."
              error={error}
              refetch={refetch}
            />
          ) : data.length === 0 ? (
            <MembersEmptyState />
          ) : (
            <div className="flex w-full flex-col gap-6">
              <ItemGroup>
                {data.map((member, index) => (
                  <React.Fragment key={`${member.id}-${member.userId}`}>
                    <Member member={member} boardId={boardId} />
                    {index !== data.length - 1 && <ItemSeparator />}
                  </React.Fragment>
                ))}
              </ItemGroup>
            </div>
          )}

          <RoleGuard minRole="Admin">
            <BoardInvitesSection boardId={boardId} />

            <DialogFooter>
              <CreateInvite boardId={boardId} />
            </DialogFooter>
          </RoleGuard>
        </DialogContent>
      </Dialog>
    </RoleGuard>
  );
}
