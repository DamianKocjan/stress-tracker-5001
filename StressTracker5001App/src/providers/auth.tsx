import { fetch } from "@/utils/fetch";
import {
  useMutation,
  useQuery,
  type UseMutationResult,
} from "@tanstack/react-query";
import React, {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";

interface User {
  id: string;
  username: string;
  email: string;
  createdAt: string;
  updatedAt: string;
}

interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  login: UseMutationResult<
    void,
    Error,
    {
      Email: string;
      Password: string;
    },
    unknown
  >;
  register: UseMutationResult<
    void,
    Error,
    {
      Username: string;
      Email: string;
      Password: string;
    },
    unknown
  >;
  logout: UseMutationResult<void, Error, void, unknown>;
}

const AuthContext = createContext<AuthState | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  const loginMutation = useMutation({
    mutationFn: async ({
      Email,
      Password,
    }: {
      Email: string;
      Password: string;
    }) => {
      const response = await fetch("/auth/login", {
        method: "POST",
        body: JSON.stringify({ Email, Password }),
      });

      if (!response.ok) {
        throw new Error("Login failed");
      }

      setIsAuthenticated(true);
    },
  });

  const registerMutation = useMutation({
    mutationFn: async ({
      Username,
      Email,
      Password,
    }: {
      Username: string;
      Email: string;
      Password: string;
    }) => {
      const response = await fetch("/auth/register", {
        method: "POST",
        body: JSON.stringify({ Username, Email, Password }),
      });

      if (!response.ok) {
        throw new Error("Registration failed");
      }
    },
  });

  const validateTokenMutation = useMutation({
    mutationFn: async () => {
      const response = await fetch("/auth/validate-token", {
        method: "POST",
      });

      setIsAuthenticated(response.ok);
    },
  });

  const logoutMutation = useMutation({
    mutationFn: async () => {
      await fetch("/auth/logout", {
        method: "POST",
      });

      setIsAuthenticated(false);
    },
  });

  const refreshTokenMutation = useMutation({
    mutationFn: async () => {
      const response = await fetch("/auth/refresh-token", {
        method: "POST",
      });
      if (!response.ok) {
        throw new Error("Failed to refresh token");
      }
    },
    onError() {
      setIsAuthenticated(false);
    },
    onSuccess() {
      setIsAuthenticated(true);
      profileQuery.refetch();
    },
  });

  const profileQuery = useQuery({
    queryKey: ["profile"],
    queryFn: async () => {
      const response = await fetch("/auth/profile");

      if (!response.ok) {
        throw new Error("Failed to fetch profile");
      }

      return response.json() as Promise<User>;
    },
    enabled: isAuthenticated,
  });

  useEffect(() => {
    // Check token validity every time page gains focus
    function handleVisibilityChange() {
      if (document.visibilityState === "visible") {
        validateTokenMutation.mutate();
      }
    }

    // Initial token validation on mount
    validateTokenMutation.mutate();

    document.addEventListener("visibilitychange", handleVisibilityChange);
    return () => {
      document.removeEventListener("visibilitychange", handleVisibilityChange);
    };
  }, []);

  useEffect(() => {
    let interval: number | undefined;
    if (isAuthenticated) {
      interval = setInterval(
        () => {
          refreshTokenMutation.mutate();
        },
        14 * 60 * 1000
      ); // Refresh every 14 minutes
    } else {
      // If not authenticated, try to refresh token once
      refreshTokenMutation.mutate();
    }
    return () => {
      if (interval) clearInterval(interval);
    };
  }, [isAuthenticated]);

  const value = useMemo(() => {
    return {
      isAuthenticated,
      user: profileQuery.data || null,
      login: loginMutation,
      register: registerMutation,
      logout: logoutMutation,
    };
  }, [
    isAuthenticated,
    loginMutation,
    logoutMutation,
    profileQuery.data,
    registerMutation,
  ]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
