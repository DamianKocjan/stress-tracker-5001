import { useBoardActivityLogsQuery } from "@/hooks/use-board-activity-logs-query";
import { useMediaQuery } from "@/hooks/use-media-query";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { useState } from "react";
import { Button } from "../ui/button";
import { Drawer, DrawerClose, DrawerContent, DrawerFooter } from "../ui/drawer";
import { Field, FieldGroup, FieldLabel } from "../ui/field";
import { ScrollArea } from "../ui/scroll-area";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../ui/select";
import { Separator } from "../ui/separator";
import { Skeleton } from "../ui/skeleton";
import { ActivityLogItem } from "./activity-log-item";

const ENTITY_OPTIONS = [
  { value: "all", label: "All Entities" },
  { value: "0", label: "Comment" },
  { value: "1", label: "User Assignment" },
  { value: "2", label: "Tag" },
  { value: "3", label: "Card" },
  { value: "4", label: "Column" },
  { value: "5", label: "Board Member" },
  { value: "6", label: "Board" },
];

const ACTION_OPTIONS = [
  { value: "all", label: "All Actions" },
  { value: "0", label: "Created" },
  { value: "1", label: "Updated" },
  { value: "2", label: "Deleted" },
  { value: "3", label: "Moved" },
];

interface ActivityLogDrawerProps {
  isOpen: boolean;
  boardId: number | null;
  onClose: () => void;
}

export function ActivityLogDrawer({
  isOpen,
  boardId,
  onClose,
}: ActivityLogDrawerProps) {
  const isDesktop = useMediaQuery("(min-width: 768px)");
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const [entityTypeFilter, setEntityTypeFilter] = useState<number | undefined>(
    undefined
  );
  const [actionTypeFilter, setActionTypeFilter] = useState<number | undefined>(
    undefined
  );

  const { data, status, error } = useBoardActivityLogsQuery({
    boardId: boardId || 0,
    page,
    pageSize,
    entityType: entityTypeFilter,
    actionType: actionTypeFilter,
  });

  const isLoading = status === "pending" || !isOpen;
  const hasFilters =
    entityTypeFilter !== undefined || actionTypeFilter !== undefined;

  const handleEntityTypeChange = (value: string) => {
    setEntityTypeFilter(value === "all" ? undefined : Number(value));
    setPage(1);
  };

  const handleActionTypeChange = (value: string) => {
    setActionTypeFilter(value === "all" ? undefined : Number(value));
    setPage(1);
  };

  const handleResetFilters = () => {
    setEntityTypeFilter(undefined);
    setActionTypeFilter(undefined);
    setPage(1);
  };

  return (
    <Drawer
      direction={isDesktop ? "right" : undefined}
      open={isOpen}
      onOpenChange={(open) => !open && onClose()}
    >
      <DrawerContent className="data-[vaul-drawer-direction=right]:lg:max-w-1/2 data-[vaul-drawer-direction=right]:xl:max-w-2/5">
        <ScrollArea className="flex-1 overflow-y-hidden p-4 pb-0">
          <div className="space-y-4 pr-4">
            {/* Header */}
            <div>
              <h2 className="text-2xl font-semibold tracking-tight">
                Activity Log
              </h2>
            </div>

            <Separator />

            {/* Filters */}
            <FieldGroup className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <Field>
                  <FieldLabel>Entity Type</FieldLabel>
                  <Select
                    value={
                      entityTypeFilter !== undefined
                        ? String(entityTypeFilter)
                        : "all"
                    }
                    onValueChange={handleEntityTypeChange}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="All Entities" />
                    </SelectTrigger>
                    <SelectContent>
                      {ENTITY_OPTIONS.map((opt) => (
                        <SelectItem key={opt.value} value={opt.value}>
                          {opt.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </Field>

                <Field>
                  <FieldLabel>Action Type</FieldLabel>
                  <Select
                    value={
                      actionTypeFilter !== undefined
                        ? String(actionTypeFilter)
                        : "all"
                    }
                    onValueChange={handleActionTypeChange}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="All Actions" />
                    </SelectTrigger>
                    <SelectContent>
                      {ACTION_OPTIONS.map((opt) => (
                        <SelectItem key={opt.value} value={opt.value}>
                          {opt.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </Field>
              </div>

              {hasFilters && (
                <Button
                  onClick={handleResetFilters}
                  variant="outline"
                  size="sm"
                  className="w-full"
                >
                  Clear Filters
                </Button>
              )}
            </FieldGroup>

            <Separator />

            {/* Activity List */}
            {isLoading ? (
              <div className="space-y-3">
                {Array.from({ length: 5 }).map((_, i) => (
                  <div key={i} className="flex gap-3">
                    <Skeleton className="size-8 rounded-full shrink-0" />
                    <div className="flex-1 space-y-2">
                      <Skeleton className="h-4 w-3/4" />
                      <Skeleton className="h-3 w-full" />
                      <Skeleton className="h-3 w-2/5" />
                    </div>
                  </div>
                ))}
              </div>
            ) : status === "error" ? (
              <div className="text-center py-8">
                <p className="text-sm font-medium text-destructive mb-1">
                  Failed to load activity logs
                </p>
                <p className="text-xs text-muted-foreground">
                  {error?.message || "An error occurred"}
                </p>
              </div>
            ) : !data || data.items.length === 0 ? (
              <div className="text-center py-8">
                <p className="text-sm text-muted-foreground">
                  No activities found
                </p>
              </div>
            ) : (
              <div className="space-y-2">
                {data.items.map((activity) => (
                  <ActivityLogItem key={activity.id} activity={activity} />
                ))}
              </div>
            )}

            <Separator />

            {/* Pagination */}
            {data && data.items.length > 0 && (
              <div className="space-y-4">
                <div className="text-xs text-muted-foreground text-center">
                  Page {data.page}
                  {data.items.length > 0 ? ` • ${data.items.length} items` : ""}
                </div>
                <div className="flex gap-2">
                  <Button
                    onClick={() => setPage(page - 1)}
                    disabled={!data.previousPage || page === 1}
                    variant="outline"
                    size="sm"
                    className="flex-1"
                  >
                    <ChevronLeft className="mr-2 size-4" />
                    Previous
                  </Button>
                  <Button
                    onClick={() => setPage(page + 1)}
                    disabled={!data.hasMore}
                    variant="outline"
                    size="sm"
                    className="flex-1"
                  >
                    Next
                    <ChevronRight className="ml-2 size-4" />
                  </Button>
                </div>
              </div>
            )}
          </div>
        </ScrollArea>

        <DrawerFooter className="mt-auto pt-2">
          <DrawerClose asChild>
            <Button variant="outline">Close</Button>
          </DrawerClose>
        </DrawerFooter>
      </DrawerContent>
    </Drawer>
  );
}
