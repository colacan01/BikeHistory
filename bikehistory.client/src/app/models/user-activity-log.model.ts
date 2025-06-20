export interface UserActivityLog {
  userId?: string;
  userName?: string;
  pageUrl?: string;
  previousPageUrl?: string;
  actionType?: string;
  additionalData?: Record<string, string>;
}

export interface UserActivityLogResponse {
  id: number;
  userId: string;
  userName: string;
  ipAddress: string;
  pageUrl: string;
  previousPageUrl: string;
  userAgent: string;
  timestamp: string;
  actionType: string;
  additionalDataJson: string;
}

// 로그 검색 필터 인터페이스
export interface ActivityLogFilter {
  startDate?: Date;
  endDate?: Date;
  userId?: string;
  page?: number;
  pageSize?: number;
}

// 페이징을 위한 응답 인터페이스 추가
export interface PaginatedResponse<T> {
  data: T[];
  totalItems: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
  hasNext: boolean;
  hasPrevious: boolean;
}
