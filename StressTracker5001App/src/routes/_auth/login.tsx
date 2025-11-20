import { useAuth } from "@/providers/auth";
import { createFileRoute, Link, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/_auth/login")({
  validateSearch: (search) => ({
    redirect: (search.redirect as string) || "/",
  }),
  beforeLoad: ({ context, search }) => {
    // Redirect if already authenticated
    if (context.auth.isAuthenticated) {
      throw redirect({ to: search.redirect });
    }
  },
  component: RouteComponent,
});

function RouteComponent() {
  const { login } = useAuth();
  const { redirect } = Route.useSearch();
  const navigate = Route.useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const form = e.target as HTMLFormElement;
    const formData = new FormData(form);
    const email = formData.get("email") as string;
    const password = formData.get("password") as string;

    try {
      console.log(
        await login.mutateAsync({ Email: email, Password: password })
      );
      navigate({ to: redirect, search: { redirect: "" } });
    } catch (error) {
      console.error("Login failed:", error);
    }
  };

  return (
    <div>
      Login
      <form onSubmit={handleSubmit}>
        <label>
          Email:
          <input type="email" name="email" />
        </label>
        <br />
        <label>
          Password:
          <input type="password" name="password" />
        </label>
        <br />
        <input type="submit" value="Login" />
      </form>
      <p>
        Don't have an account?{" "}
        <Link search={{ redirect }} to="/register">
          Register
        </Link>
      </p>
    </div>
  );
}
