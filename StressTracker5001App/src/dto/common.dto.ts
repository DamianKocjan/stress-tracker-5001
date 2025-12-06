export interface PagedResultDto<T> {
  items: T[];
  hasMore: boolean;
  previousPage: number;
  page: number;
  nextPage: number;
  pageSize: number;
}
