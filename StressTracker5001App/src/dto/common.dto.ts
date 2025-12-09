export interface PagedResultDto<T> {
  items: T[];
  hasMore: boolean;
  previousPage: number;
  page: number;
  nextPage: number;
  pageSize: number;
}

export interface ResultDto<T> {
  success: boolean;
  data?: T;
  errorMessage?: string;
  errors?: string[];
  statusCode?: number;
}
