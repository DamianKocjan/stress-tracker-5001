import { cn } from "@/lib/utils";
import { useAuth } from "@/providers/auth";
import { Link } from "@tanstack/react-router";
import { LayoutDashboard } from "lucide-react";
import { Button } from "./ui/button";

export function AppNav() {
  const { isAuthenticated, logout } = useAuth();

  return (
    <nav className="border-b border-border bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60 sticky top-0 z-50">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          <div className="flex items-center gap-2">
            <LayoutDashboard className="h-6 w-6 text-foreground" />
            <span className="text-xl font-semibold text-foreground">
              StressTracker
            </span>
          </div>
          <div
            className={cn("hidden items-center gap-6", {
              "md:flex": !isAuthenticated,
            })}
          >
            <Link
              to="/"
              hash="features"
              hashScrollIntoView={{ behavior: "smooth", block: "start" }}
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              Features
            </Link>
            <Link
              to="/"
              hash="benefits"
              hashScrollIntoView={{ behavior: "smooth", block: "start" }}
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              Benefits
            </Link>
            <Link
              to="/"
              hash="how-it-works"
              hashScrollIntoView={{ behavior: "smooth", block: "start" }}
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              How It Works
            </Link>
          </div>

          {isAuthenticated ? (
            <div className="flex items-center gap-3">
              <Button variant="ghost" size="sm" asChild>
                <Link to="/">Dashboard</Link>
              </Button>
              <Button
                size="sm"
                disabled={logout.isPending}
                onClick={() => logout.mutateAsync()}
              >
                Logout
              </Button>
            </div>
          ) : (
            <div className="flex items-center gap-3">
              <Button variant="ghost" size="sm" asChild>
                <Link to="/login" search={{ redirect: "/" }}>
                  Sign In
                </Link>
              </Button>
              <Button size="sm">Get Started</Button>
            </div>
          )}
        </div>
      </div>
    </nav>
  );
}
