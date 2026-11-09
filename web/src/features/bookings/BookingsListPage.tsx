import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import type { BookingDto, BookingFilters, BookingSort } from '../../api/bookingsApi';
import { ErrorAlert } from '../../components/ErrorAlert';
import { GenericTable } from '../../components/GenericTable';
import { StatusBadge } from '../../components/StatusBadge';
import { formatLocal, isoToLocalInput, localInputToIso } from '../../utils/dates';
import { useBookings } from './hooks';

const sortOptions: { value: BookingSort; label: string }[] = [
  { value: 'start', label: 'Inicio (más próximas)' },
  { value: '-start', label: 'Inicio (más lejanas)' },
  { value: '-createdAt', label: 'Creación (más recientes)' },
  { value: 'createdAt', label: 'Creación (más antiguas)' },
];

const pageSize = 10;

// Filters live in the URL (query params): the source of truth survives reloads and can be shared (CA9).
function readFilters(params: URLSearchParams): BookingFilters {
  return {
    resourceId: params.get('resourceId') ?? undefined,
    from: params.get('from') ?? undefined,
    to: params.get('to') ?? undefined,
    sort: (params.get('sort') as BookingSort | null) ?? 'start',
    page: Number(params.get('page') ?? '1') || 1,
    pageSize,
  };
}

export function BookingsListPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const filters = readFilters(searchParams);
  const navigate = useNavigate();
  const { data, isPending, isError, error, refetch } = useBookings(filters);

  const [resourceId, setResourceId] = useState(filters.resourceId ?? '');
  const [from, setFrom] = useState(isoToLocalInput(filters.from));
  const [to, setTo] = useState(isoToLocalInput(filters.to));

  const updateParams = (changes: Record<string, string | undefined>) => {
    const next = new URLSearchParams(searchParams);
    for (const [key, value] of Object.entries(changes)) {
      if (value) {
        next.set(key, value);
      } else {
        next.delete(key);
      }
    }
    setSearchParams(next);
  };

  const applyFilters = (event: FormEvent) => {
    event.preventDefault();
    updateParams({
      resourceId: resourceId.trim() || undefined,
      from: from ? localInputToIso(from) : undefined,
      to: to ? localInputToIso(to) : undefined,
      page: undefined,
    });
  };

  const clearFilters = () => {
    setResourceId('');
    setFrom('');
    setTo('');
    setSearchParams(new URLSearchParams());
  };

  return (
    <section>
      <div className="page-header">
        <h1>Reservas</h1>
        <Link className="button" to="/bookings/new">
          Nueva reserva
        </Link>
      </div>

      <form className="filters" onSubmit={applyFilters} aria-label="Filtros">
        <label>
          Recurso
          <input value={resourceId} onChange={(event) => setResourceId(event.target.value)} placeholder="room-A" />
        </label>
        <label>
          Desde
          <input type="datetime-local" value={from} onChange={(event) => setFrom(event.target.value)} />
        </label>
        <label>
          Hasta
          <input type="datetime-local" value={to} onChange={(event) => setTo(event.target.value)} />
        </label>
        <label>
          Orden
          <select value={filters.sort} onChange={(event) => updateParams({ sort: event.target.value, page: undefined })}>
            {sortOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </label>
        <div className="filters__actions">
          <button type="submit" className="button">
            Filtrar
          </button>
          <button type="button" className="button button--secondary" onClick={clearFilters}>
            Limpiar
          </button>
        </div>
      </form>

      {isError ? (
        <ErrorAlert error={error} onRetry={() => void refetch()} />
      ) : (
        <>
          <GenericTable<BookingDto>
            items={data?.items ?? []}
            loading={isPending}
            emptyText="No hay reservas que coincidan con los filtros."
            getRowKey={(booking) => booking.id}
            onRowClick={(booking) => navigate(`/bookings/${booking.id}`)}
            header={
              <tr>
                <th>Recurso</th>
                <th>Cliente</th>
                <th>Inicio</th>
                <th>Fin</th>
                <th>Estado</th>
              </tr>
            }
            renderRow={(booking) => (
              <>
                <td>{booking.resourceId}</td>
                <td>{booking.customerName}</td>
                <td>{formatLocal(booking.start)}</td>
                <td>{formatLocal(booking.end)}</td>
                <td>
                  <StatusBadge status={booking.status} />
                </td>
              </>
            )}
          />

          {data && data.totalCount > 0 && (
            <nav className="pagination" aria-label="Paginación">
              <button
                type="button"
                className="button button--secondary"
                disabled={data.page <= 1}
                onClick={() => updateParams({ page: String(data.page - 1) })}
              >
                Anterior
              </button>
              <span>
                Página {data.page} de {data.totalPages} · {data.totalCount} reservas
              </span>
              <button
                type="button"
                className="button button--secondary"
                disabled={data.page >= data.totalPages}
                onClick={() => updateParams({ page: String(data.page + 1) })}
              >
                Siguiente
              </button>
            </nav>
          )}
        </>
      )}
    </section>
  );
}
