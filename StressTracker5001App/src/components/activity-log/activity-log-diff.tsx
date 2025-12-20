import { cn } from "@/lib/utils";

interface ActivityLogDiffProps {
  changes?: Record<string, { Old: unknown; New: unknown }>;
  className?: string;
}

function formatValue(value: unknown): string {
  if (value === null || value === undefined) {
    return "(empty)";
  }
  if (typeof value === "object") {
    return JSON.stringify(value);
  }
  return String(value);
}

export function ActivityLogDiff({ changes, className }: ActivityLogDiffProps) {
  if (!changes || Object.keys(changes).length === 0) {
    return null;
  }

  return (
    <div className={cn("space-y-2", className)}>
      {Object.entries(changes).map(([key, { Old, New }]) => {
        const oldStr = formatValue(Old);
        const newStr = formatValue(New);

        return (
          <div key={key} className="text-sm space-y-1">
            <p className="font-medium text-foreground capitalize">
              {key
                .replace(/([A-Z])/g, " $1")
                .trim()
                .toLowerCase()}
            </p>
            <div className="flex items-center gap-2 text-xs text-muted-foreground flex-wrap">
              <span className="line-through text-red-600 dark:text-red-400">
                {oldStr}
              </span>
              <span className="text-muted-foreground">→</span>
              <span className="font-medium text-green-600 dark:text-green-400">
                {newStr}
              </span>
            </div>
          </div>
        );
      })}
    </div>
  );
}
