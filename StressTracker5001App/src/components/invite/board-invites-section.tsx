import { useBoardInvitesQuery } from "@/hooks/use-board-invites-query";
import { Separator } from "../ui/separator";
import { InvitesList } from "./invites-list";
import {
  InvitesEmptyState,
  InvitesErrorState,
  InvitesLoadingState,
} from "./invites-states";

interface BoardInvitesSectionProps {
  boardId: number;
}

export function BoardInvitesSection({ boardId }: BoardInvitesSectionProps) {
  const {
    data: invites = [],
    status,
    error,
    refetch,
  } = useBoardInvitesQuery(boardId);

  if (invites.length === 0 && status !== "pending") {
    return null;
  }

  return (
    <>
      <Separator />

      <div className="space-y-4">
        <div>
          <h3 className="font-semibold">Active Invites</h3>
          <p className="text-sm text-muted-foreground">
            Pending invites waiting to be accepted
          </p>
        </div>
        {status === "pending" ? (
          <InvitesLoadingState count={2} />
        ) : status === "error" ? (
          <InvitesErrorState error={error} refetch={refetch} />
        ) : invites.length === 0 ? (
          <InvitesEmptyState />
        ) : (
          <InvitesList invites={invites} boardId={boardId} />
        )}
      </div>
    </>
  );
}
