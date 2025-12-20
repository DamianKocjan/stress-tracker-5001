import { ConfirmEmailForm } from "@/components/auth/confirm-email-form";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_auth/confirm-email")({
  validateSearch: (search) => ({
    token: (search.token as string) || "",
  }),
  component: RouteComponent,
});

function RouteComponent() {
  const { token } = Route.useSearch();

  return (
    <div className="bg-muted flex min-h-svh flex-col items-center justify-center p-6 md:p-10">
      <div className="w-full max-w-sm md:max-w-md">
        <ConfirmEmailForm token={token} />
      </div>
    </div>
  );
}
