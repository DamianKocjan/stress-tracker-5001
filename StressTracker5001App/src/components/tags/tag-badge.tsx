import { Badge } from "@/components/ui/badge";
import type { TagDto } from "@/dto/tag.dto";
import { cn } from "@/lib/utils";
import { X } from "lucide-react";

interface TagBadgeProps {
  tag: TagDto;
  onRemove?: () => void;
  variant?: "default" | "outline";
  className?: string;
}

function isLightColor(hexColor: string): boolean {
  // Convert hex to RGB and calculate luminance
  const r = parseInt(hexColor.slice(1, 3), 16);
  const g = parseInt(hexColor.slice(3, 5), 16);
  const b = parseInt(hexColor.slice(5, 7), 16);
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
  return luminance > 0.6;
}

export function TagBadge({
  tag,
  onRemove,
  variant = "default",
  className,
}: TagBadgeProps) {
  const textColor =
    variant === "default" && isLightColor(tag.color)
      ? "text-gray-900"
      : "text-white";

  return (
    <Badge
      variant={variant}
      style={variant === "default" ? { backgroundColor: tag.color } : undefined}
      className={cn(
        "flex items-center gap-1 px-2 py-1",
        variant === "default" && textColor,
        variant === "outline" && "border-2",
        className
      )}
    >
      {variant === "outline" && (
        <span
          className="inline-block w-2 h-2 rounded-full"
          style={{ backgroundColor: tag.color }}
        />
      )}
      <span className="text-xs">{tag.name}</span>
      {onRemove && (
        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            onRemove();
          }}
          className="hover:opacity-70"
        >
          <X className="h-3 w-3" />
        </button>
      )}
    </Badge>
  );
}
