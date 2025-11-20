export const API_BASE_URL = import.meta.env.DEV
  ? "http://localhost:5292/api"
  : (import.meta.env.API_BASE_URL_PROD as string);
