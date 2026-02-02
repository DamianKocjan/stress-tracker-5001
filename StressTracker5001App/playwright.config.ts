import { defineConfig, devices } from "@playwright/test";
import path from "path";
import { fileURLToPath } from "url";

const isCI = !!process.env.CI;
const webUrl = process.env.E2E_WEB_URL ?? "http://localhost:5173";
const apiBaseUrl = process.env.E2E_API_URL ?? "http://localhost:5292";
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

export default defineConfig({
  testDir: "./e2e",
  fullyParallel: false,
  timeout: 60_000,
  expect: {
    timeout: 10_000,
  },
  workers: process.env.PLAYWRIGHT_WORKERS
    ? Number(process.env.PLAYWRIGHT_WORKERS)
    : 1,
  retries: isCI ? 1 : 0,
  use: {
    baseURL: webUrl,
    trace: "on-first-retry",
    screenshot: "only-on-failure",
    video: "retain-on-failure",
  },
  webServer: [
    {
      command: "pnpm dev -- --host 0.0.0.0 --port 5173",
      url: webUrl,
      reuseExistingServer: !isCI,
      cwd: path.resolve(__dirname),
      timeout: 120_000,
    },
    {
      command:
        "dotnet run --project ./StressTracker5001Server/StressTracker5001Server.csproj --urls http://localhost:5292",
      url: `${apiBaseUrl}/api/test/ping`,
      reuseExistingServer: !isCI,
      cwd: path.resolve(__dirname, ".."),
      timeout: 120_000,
      env: {
        ASPNETCORE_ENVIRONMENT: "Development",
        ConnectionStrings__Default: "Data Source=:memory:",
        WebApplicationUrl: webUrl,
      },
    },
  ],
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] },
    },
  ],
});
