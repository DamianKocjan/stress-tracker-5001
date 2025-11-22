import { useAuth } from "@/providers/auth";

export function DashboardWelcome() {
  const { user } = useAuth();

  return (
    <div className="mb-8">
      <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
      <p className="text-muted-foreground">
        Welcome back {user?.username}! Here is an overview of your stress
        tracking boards.
      </p>
    </div>
  );
}
