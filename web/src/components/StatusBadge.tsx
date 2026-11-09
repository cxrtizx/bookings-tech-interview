import type { BookingStatus } from '../api/bookingsApi';

const labels: Record<BookingStatus, string> = {
  Pending: 'Pendiente',
  Confirmed: 'Confirmada',
  Cancelled: 'Cancelada',
};

export function StatusBadge({ status }: { status: BookingStatus }) {
  return <span className={`badge badge--${status.toLowerCase()}`}>{labels[status]}</span>;
}
