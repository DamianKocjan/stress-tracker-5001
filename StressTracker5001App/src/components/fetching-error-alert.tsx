import { cn } from "@/lib/utils";
import { AlertCircleIcon } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "./ui/alert";
import { Button } from "./ui/button";

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

      <Button
        variant="outline"
        className="absolute top-4 right-4 text-accent-foreground"
        onClick={() => refetch()}
      >
        Retry
      </Button>
    </Alert>
  );
}
