/* eslint-disable react-hooks/rules-of-hooks */
import { useAuth } from "@/providers/auth";
import {
  createFileRoute,
  Outlet,
  useNavigate,
  useSearch,
} from "@tanstack/react-router";

export const Route = createFileRoute("/_auth")({
  component: () => {
    const { isAuthenticated } = useAuth();
    const redirect = useSearch({
      from: "/_auth/login",
      select: (s) => s.redirect,
    });
    const navigate = useNavigate();

    if (isAuthenticated) {
      navigate({ to: redirect, search: {} });
    }

    return <Outlet />;
  },
});
