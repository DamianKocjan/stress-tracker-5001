import { FetchingErrorAlert } from "../fetching-error-alert";
import {
  Item,
  ItemActions,
  ItemContent,
  ItemGroup,
  ItemMedia,
  ItemSeparator,
} from "../ui/item";
import { Skeleton } from "../ui/skeleton";

interface InvitesLoadingStateProps {
  count?: number;
}

export function InvitesLoadingState({ count = 2 }: InvitesLoadingStateProps) {
  return (
    <div className="flex w-full flex-col gap-6">
      <ItemGroup>
        {Array.from({ length: count }).map((_, i) => (
          <div key={i}>
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
            {i !== count - 1 && <ItemSeparator />}
          </div>
        ))}
      </ItemGroup>
    </div>
  );
}

interface InvitesErrorStateProps {
  error: Error | null;
  refetch: () => void;
}

export function InvitesErrorState({ error, refetch }: InvitesErrorStateProps) {
  return (
    <FetchingErrorAlert
      title="Failed to load invites."
      error={error}
      refetch={refetch}
    />
  );
}

interface InvitesEmptyStateProps {
  children?: React.ReactNode;
}

export function InvitesEmptyState({ children }: InvitesEmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center gap-4 rounded-lg border border-dashed border-muted-foreground/25 px-4 py-8">
      <p className="text-center text-sm text-muted-foreground">
        No active invites
      </p>
      {children}
    </div>
  );
}
