import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { cn } from "@/lib/utils";
import type { ColorLike } from "color";
import Color from "color";
import { XIcon } from "lucide-react";
import {
  type ComponentProps,
  type MouseEventHandler,
  type ReactNode,
} from "react";

export type TagsProps = {
  open?: boolean;
  onOpenChange?: (open: boolean) => void;
  children?: ReactNode;
  className?: string;
};

export const Tags = ({
  open,
  onOpenChange,
  children,
  className,
}: TagsProps) => (
  <Popover onOpenChange={onOpenChange} open={open}>
    <div className={cn("relative w-full", className)}>{children}</div>
  </Popover>
);

export type TagsTriggerProps = ComponentProps<typeof Button>;

export const TagsTrigger = ({
  className,
  children,
  ...props
}: TagsTriggerProps) => (
  <PopoverTrigger asChild>
    <Button
      className={cn("h-auto w-full justify-between p-2", className)}
      role="combobox"
      variant="outline"
      {...props}
    >
      <div className="flex flex-wrap items-center gap-1">
        {children}
        <span className="px-2 py-px text-muted-foreground">
          Select a tag...
        </span>
      </div>
    </Button>
  </PopoverTrigger>
);

export type TagsValueProps = ComponentProps<typeof Badge> & {
  color: ColorLike;
};

export const TagsValue = ({
  className,
  children,
  onRemove,
  color,
  ...props
}: TagsValueProps & { onRemove?: () => void }) => {
  const handleRemove: MouseEventHandler<HTMLDivElement> = (event) => {
    event.preventDefault();
    event.stopPropagation();
    onRemove?.();
  };

  const colorInstance = Color(color);
  const luminance = colorInstance.luminosity();
  const hoverColor =
    luminance < 0.5
      ? colorInstance.darken(0.1).hex()
      : colorInstance.lighten(0.1).hex();
  const textColor = luminance > 0.5 ? "#000000" : "#FFFFFF";

  return (
    <Badge
      className={cn(
        "flex items-center gap-2 bg-(--bg-color) text-(--text-color) hover:bg-(--hover-bg-color) border-(--text-color)",
        className
      )}
      style={
        {
          "--bg-color": color,
          "--hover-bg-color": hoverColor,
          "--text-color": textColor,
        } as React.CSSProperties
      }
      {...props}
    >
      {children}
      {onRemove && (
        <div
          className="size-auto cursor-pointer hover:text-muted-foreground"
          onClick={handleRemove}
        >
          <XIcon size={12} />
        </div>
      )}
    </Badge>
  );
};

export type TagsContentProps = ComponentProps<typeof PopoverContent>;

export const TagsContent = ({
  className,
  children,
  ...props
}: TagsContentProps) => {
  return (
    <PopoverContent className={cn("p-0", className)} {...props}>
      <Command>{children}</Command>
    </PopoverContent>
  );
};

export type TagsInputProps = ComponentProps<typeof CommandInput>;

export const TagsInput = ({ className, ...props }: TagsInputProps) => (
  <CommandInput className={cn("h-9", className)} {...props} />
);

export type TagsListProps = ComponentProps<typeof CommandList>;

export const TagsList = ({ className, ...props }: TagsListProps) => (
  <CommandList className={cn("max-h-[200px]", className)} {...props} />
);

export type TagsEmptyProps = ComponentProps<typeof CommandEmpty>;

export const TagsEmpty = ({ children, ...props }: TagsEmptyProps) => (
  <CommandEmpty {...props}>{children ?? "No tags found."}</CommandEmpty>
);

export type TagsGroupProps = ComponentProps<typeof CommandGroup>;

export const TagsGroup = CommandGroup;

export type TagsItemProps = ComponentProps<typeof CommandItem>;

export const TagsItem = ({ className, ...props }: TagsItemProps) => (
  <CommandItem
    className={cn("cursor-pointer items-center justify-between", className)}
    {...props}
  />
);
