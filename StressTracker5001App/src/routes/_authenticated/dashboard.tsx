import { BoardList } from "@/components/board-list";
import { DashboardWelcome } from "@/components/dashboard-welcome";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated/dashboard")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <div className="container mx-auto p-6">
      <DashboardWelcome />

      <BoardList />
    </div>
  );
}
