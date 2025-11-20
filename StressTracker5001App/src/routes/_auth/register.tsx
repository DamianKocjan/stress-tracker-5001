import { useAuth } from "@/providers/auth";
import { createFileRoute, Link, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/_auth/register")({
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
  const { register } = useAuth();
  const { redirect } = Route.useSearch();
  const navigate = Route.useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const form = e.target as HTMLFormElement;
    const formData = new FormData(form);
    const email = formData.get("email") as string;
    const password = formData.get("password") as string;
    const confirmPassword = formData.get("confirmPassword") as string;

    if (password !== confirmPassword) {
      console.error("Passwords do not match");
      return;
    }

    const username = formData.get("username") as string;

    try {
      console.log(
        await register.mutateAsync({
          Email: email,
          Password: password,
          Username: username,
        })
      );
      navigate({ to: "/login", search: { redirect } });
    } catch (error) {
      console.error("Register failed:", error);
    }
  };

  return (
    <div>
      Register
      <form onSubmit={handleSubmit}>
        <label>
          Username:
          <input type="text" name="username" />
        </label>
        <label>
          Email:
          <input type="email" name="email" />
        </label>
        <br />
        <label>
          Password:
          <input type="password" name="password" />
        </label>
        <label>
          Confirm Password:
          <input type="password" name="confirmPassword" />
        </label>
        <br />
        <input type="submit" value="Register" />
      </form>
      <p>
        Already have an account?{" "}
        <Link search={{ redirect }} to="/login">
          Login
        </Link>
      </p>
    </div>
  );
}
