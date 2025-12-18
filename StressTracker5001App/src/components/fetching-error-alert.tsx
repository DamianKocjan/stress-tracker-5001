import { cn } from "@/lib/utils";
import { AlertCircleIcon, Loader2Icon } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "./ui/alert";
import { Button } from "./ui/button";
import { Tooltip, TooltipContent, TooltipTrigger } from "./ui/tooltip";

interface FetchingErrorAlertProps {
  title: string;
  error: unknown;
  refetch: () => void;
  className?: string;
}

export function FetchingErrorAlert({
  title,
  error,
  refetch,
  className,
}: FetchingErrorAlertProps) {
  return (
    <Alert variant="destructive" className={cn("relative", className)}>
      <AlertCircleIcon />
      <AlertTitle>{title}</AlertTitle>
      <AlertDescription>
        {error instanceof Error ? error.message : "An unknown error occurred."}
      </AlertDescription>

      <Tooltip>
        <TooltipTrigger asChild>
          <Button
            variant="outline"
            size="icon-sm"
            className="absolute top-3 right-3 text-accent-foreground group"
            onClick={() => refetch()}
          >
            <span className="sr-only">Retry</span>
            <Loader2Icon className="group-hover:animate-spin size-3" />
          </Button>
        </TooltipTrigger>
        <TooltipContent>Retry</TooltipContent>
      </Tooltip>
    </Alert>
  );
}
