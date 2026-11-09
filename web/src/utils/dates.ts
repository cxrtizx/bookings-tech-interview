const localFormatter = new Intl.DateTimeFormat('es-ES', { dateStyle: 'short', timeStyle: 'short' });

// The API returns ISO 8601 with offset (UTC); the UI shows it in the user's local time.
export function formatLocal(iso: string | null | undefined): string {
  return iso ? localFormatter.format(new Date(iso)) : '—';
}

// <input type="datetime-local"> value (no offset, local time) -> ISO 8601 UTC for the API.
export function localInputToIso(value: string): string {
  return new Date(value).toISOString();
}

// ISO 8601 -> <input type="datetime-local"> value in local time (yyyy-MM-ddTHH:mm).
export function isoToLocalInput(iso: string | null | undefined): string {
  if (!iso) {
    return '';
  }
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) {
    return '';
  }
  const pad = (value: number) => String(value).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}
