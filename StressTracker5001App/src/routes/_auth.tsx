/* eslint-disable react-hooks/rules-of-hooks */
import { useAuth } from "@/providers/auth";
import {
  createFileRoute,
  Outlet,
  useNavigate,
  useSearch,
} from "@tanstack/react-router";
import { useEffect } from "react";

export const Route = createFileRoute("/_auth")({
  component: () => {
    const { isAuthenticated } = useAuth();
    const redirect = useSearch({
      from: "/_auth",
      // @ts-expect-error search type inference issue
      select: (s) => s.redirect,
    });
    const navigate = useNavigate();

    useEffect(() => {
      if (isAuthenticated) {
        navigate({ to: redirect, search: {} });
      }
    }, [isAuthenticated, redirect]);

    return <Outlet />;
  },
});
