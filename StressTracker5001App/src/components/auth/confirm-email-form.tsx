import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Field, FieldError, FieldGroup } from "@/components/ui/field";
import { useConfirmEmailChangeMutation } from "@/hooks/use-confirm-email-change-mutation";
import { cn } from "@/lib/utils";
import { useNavigate } from "@tanstack/react-router";
import { AlertCircle, CheckCircle, Loader2Icon } from "lucide-react";
import { useEffect } from "react";

const REDIRECT_DELAY_MS = 2000;

export function ConfirmEmailForm({
  token,
  className,
  ...props
}: {
  token: string;
} & React.ComponentProps<"div">) {
  const navigate = useNavigate();
  const {
    mutate,
    isPending,
    isSuccess,
    isError,
    error: mutationError,
  } = useConfirmEmailChangeMutation();

  useEffect(() => {
    if (!token) {
      return;
    }

    mutate({ token });
  }, [token, mutate]);

  useEffect(() => {
    if (isSuccess) {
      setTimeout(() => {
        navigate({ to: "/dashboard" });
      }, REDIRECT_DELAY_MS);
    }
  }, [isSuccess, navigate]);

  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <Card className="overflow-hidden p-0">
        <CardContent className="grid p-0">
          <div className="p-6 md:p-8">
            <FieldGroup>
              <div className="flex flex-col items-center gap-4 text-center">
                {isPending && (
                  <>
                    <Loader2Icon className="size-12 animate-spin" />
                    <h1 className="text-2xl font-bold">Verifying Email...</h1>
                  </>
                )}

                {isSuccess && (
                  <>
                    <CheckCircle className="size-12 text-green-500" />
                    <h1 className="text-2xl font-bold">Email Verified!</h1>
                    <p className="text-muted-foreground">
                      Your email has been verified successfully. Redirecting...
                    </p>
                  </>
                )}

                {isError && mutationError && !isPending && (
                  <>
                    <AlertCircle className="size-12 text-red-500" />
                    <h1 className="text-2xl font-bold">Verification Failed</h1>
                    <Field data-invalid>
                      <FieldError>{mutationError.message}</FieldError>
                    </Field>
                    <Button
                      onClick={() =>
                        navigate({ to: "/login", search: { redirect: "/" } })
                      }
                      className="w-full"
                    >
                      Back to Login
                    </Button>
                  </>
                )}
              </div>
            </FieldGroup>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
