import { AlertCircleIcon } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "./ui/alert";
import { Button } from "./ui/button";

interface FetchingErrorAlertProps {
  error: unknown;
  refetch: () => void;
}

export function FetchingErrorAlert({
  error,
  refetch,
}: FetchingErrorAlertProps) {
  return (
    <Alert variant="destructive" className="relative">
      <AlertCircleIcon />
      <AlertTitle>Failed to load boards</AlertTitle>
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
