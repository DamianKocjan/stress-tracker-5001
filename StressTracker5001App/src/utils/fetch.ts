import { API_BASE_URL } from "@/constants";

function _fetch(input: RequestInfo, init?: RequestInit) {
  const headers = new Headers(init?.headers || {});
  headers.set("Content-Type", "application/json");
  headers.set("Accept", "application/json");

  return fetch(`${API_BASE_URL}${input}`, {
    credentials: "include",
    ...init,
    headers,
  });
}

export { _fetch as fetch };
