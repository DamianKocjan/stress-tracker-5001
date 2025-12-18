import type { BoardInviteDto } from "@/dto/board-invite.dto";
import { InvitesList } from "./invites-list";
import {
  InvitesEmptyState,
  InvitesErrorState,
  InvitesLoadingState,
} from "./invites-states";

interface BoardInvitesSectionProps {
  invites: BoardInviteDto[];
  boardId: number;
  isLoading: boolean;
  error: Error | null;
  refetch: () => void;
}

export function BoardInvitesSection({
  invites,
  boardId,
  isLoading,
  error,
  refetch,
}: BoardInvitesSectionProps) {
  if (invites.length === 0 && !isLoading) {
    return null;
  }

  return (
    <div className="space-y-4">
      <div>
        <h3 className="font-semibold">Active Invites</h3>
        <p className="text-sm text-muted-foreground">
          Pending invites waiting to be accepted
        </p>
      </div>
      {isLoading ? (
        <InvitesLoadingState count={2} />
      ) : error ? (
        <InvitesErrorState error={error} refetch={refetch} />
      ) : invites.length === 0 ? (
        <InvitesEmptyState />
      ) : (
        <InvitesList invites={invites} boardId={boardId} />
      )}
    </div>
  );
}
