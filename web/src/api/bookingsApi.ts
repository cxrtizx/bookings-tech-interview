import { buildQuery, request } from './httpClient';

export type BookingStatus = 'Pending' | 'Confirmed' | 'Cancelled';

// Mirrors Bookings.Application BookingDto (JSON in camelCase, dates ISO 8601 with offset).
export interface BookingDto {
  id: string;
  customerName: string;
  customerEmail: string;
  start: string;
  end: string;
  createdAt: string;
  confirmedAt: string | null;
  cancelledAt: string | null;
  cancellationReason: string | null;
  resourceId: string;
  status: BookingStatus;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export type BookingSort = 'start' | '-start' | 'createdAt' | '-createdAt';

export interface BookingFilters {
  resourceId?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
  sort?: BookingSort;
}

export interface CreateBookingRequest {
  resourceId: string;
  customer: { name: string; email: string };
  start: string;
  end: string;
}

export interface CreateBookingResponse {
  id: string;
}

export const bookingsApi = {
  list: (filters: BookingFilters) =>
    request<PagedResult<BookingDto>>(`/api/bookings${buildQuery({ ...filters })}`),

  getById: (id: string) => request<BookingDto>(`/api/bookings/${encodeURIComponent(id)}`),

  create: (body: CreateBookingRequest) =>
    request<CreateBookingResponse>('/api/bookings', { method: 'POST', body: JSON.stringify(body) }),

  confirm: (id: string) =>
    request<void>(`/api/bookings/${encodeURIComponent(id)}/confirm`, { method: 'POST' }),

  cancel: (id: string, reason?: string) =>
    request<void>(`/api/bookings/${encodeURIComponent(id)}/cancel`, {
      method: 'POST',
      body: JSON.stringify({ reason: reason?.trim() || null }),
    }),
};
