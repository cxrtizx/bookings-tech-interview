import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { bookingsApi, type BookingFilters, type CreateBookingRequest } from '../../api/bookingsApi';
import type { ApiError } from '../../api/problemDetails';

const bookingKeys = {
  all: ['bookings'] as const,
  list: (filters: BookingFilters) => [...bookingKeys.all, 'list', filters] as const,
  detail: (id: string) => [...bookingKeys.all, 'detail', id] as const,
};

export function useBookings(filters: BookingFilters) {
  return useQuery({
    queryKey: bookingKeys.list(filters),
    queryFn: () => bookingsApi.list(filters),
    placeholderData: keepPreviousData,
  });
}

export function useBooking(id: string) {
  return useQuery({
    queryKey: bookingKeys.detail(id),
    queryFn: () => bookingsApi.getById(id),
  });
}

export function useCreateBooking() {
  const queryClient = useQueryClient();
  return useMutation<{ id: string }, ApiError, CreateBookingRequest>({
    mutationFn: bookingsApi.create,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: bookingKeys.all }),
  });
}

// On failure nothing is invalidated, so the details shown keep the last known (unchanged) state (CA8).
export function useConfirmBooking(id: string) {
  const queryClient = useQueryClient();
  return useMutation<void, ApiError, void>({
    mutationFn: () => bookingsApi.confirm(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: bookingKeys.all }),
  });
}

export function useCancelBooking(id: string) {
  const queryClient = useQueryClient();
  return useMutation<void, ApiError, string | undefined>({
    mutationFn: (reason) => bookingsApi.cancel(id, reason),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: bookingKeys.all }),
  });
}
