import type { LoginDto, RegisterDto } from "@/dto/auth.dto";
import type { ResultDto } from "@/dto/common.dto";
import type { UserDto } from "@/dto/user.dto";
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

export interface AuthState {
  isAuthenticated: boolean;
  user: UserDto | null;
  login: UseMutationResult<void, Error, LoginDto, unknown>;
  register: UseMutationResult<void, Error, RegisterDto, unknown>;
  logout: UseMutationResult<void, Error, void, unknown>;
}

const AuthContext = createContext<AuthState | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  const loginMutation = useMutation({
    mutationFn: async ({ email, password }: LoginDto) => {
      const response = await fetch("/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });

      if (!response.ok) {
        const result = (await response.json()) as ResultDto<void>;
        throw new Error(result.errorMessage || "Login failed");
      }

      setIsAuthenticated(true);
    },
  });

  const registerMutation = useMutation({
    mutationFn: async ({ username, email, password }: RegisterDto) => {
      const response = await fetch("/auth/register", {
        method: "POST",
        body: JSON.stringify({ username, email, password }),
      });

      if (!response.ok) {
        const result = (await response.json()) as ResultDto<void>;
        throw new Error(result.errorMessage || "Registration failed");
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
        const result = (await response.json()) as ResultDto<void>;
        throw new Error(result.errorMessage || "Failed to refresh token");
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
        const result = (await response.json()) as ResultDto<UserDto>;
        throw new Error(result.errorMessage || "Failed to fetch profile");
      }

      const result = (await response.json()) as ResultDto<UserDto>;
      if (!result.success || !result.data) {
        throw new Error(result.errorMessage || "Failed to fetch profile");
      }
      return result.data;
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
export function useAuth(): AuthState {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
