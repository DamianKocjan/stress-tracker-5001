import { Button } from "@/components/ui/button";
import { useAuth } from "@/providers/auth";
import { createFileRoute, Link } from "@tanstack/react-router";

export const Route = createFileRoute("/")({
  component: IndexComponent,
});

function IndexComponent() {
  const { isAuthenticated, user, logout } = useAuth();

  return (
    <div>
      {isAuthenticated ? "Welcome back!" : "Welcome to Stress Tracker 5001!"}

      {isAuthenticated ? (
        <>
          <pre>{JSON.stringify(user, null, 2)}</pre>
          <Button onClick={() => logout.mutateAsync()}>Logout</Button>
        </>
      ) : (
        <>
          <Button asChild>
            <Link search={{ redirect: "/" }} to="/login">
              Login
            </Link>
          </Button>
          <Button asChild>
            <Link search={{ redirect: "/" }} to="/register">
              Register
            </Link>
          </Button>
        </>
      )}
    </div>
  );
}
