import { ResetPasswordForm } from "@/components/auth/reset-password-form";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_auth/reset-password/$token")({
  component: RouteComponent,
});

function RouteComponent() {
  const { token } = Route.useParams();

  return (
    <div className="bg-muted flex min-h-svh flex-col items-center justify-center p-6 md:p-10">
      <div className="w-full max-w-sm md:max-w-md">
        <ResetPasswordForm token={token as string} />
      </div>
    </div>
  );
}
