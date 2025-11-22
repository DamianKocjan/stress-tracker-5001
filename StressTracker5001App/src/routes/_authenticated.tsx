import { AppFooter } from "@/components/app-footer";
import { AppNav } from "@/components/app-nav";
import { createFileRoute, Outlet, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated")({
  beforeLoad: ({ context, location }) => {
    if (!context.auth.isAuthenticated) {
      throw redirect({
        to: "/login",
        search: {
          // Save current location for redirect after login
          redirect: location.href,
        },
      });
    }
  },
  component: () => (
    <div className="min-h-screen scroll-smooth">
      <AppNav />
      <main>
        <Outlet />
      </main>
      <AppFooter />
    </div>
  ),
});
