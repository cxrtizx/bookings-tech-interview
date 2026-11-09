import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ErrorAlert } from '../../components/ErrorAlert';
import { StatusBadge } from '../../components/StatusBadge';
import { formatLocal } from '../../utils/dates';
import { useBooking, useCancelBooking, useConfirmBooking } from './hooks';

export function BookingDetailsPage() {
  const { id = '' } = useParams<{ id: string }>();
  const { data: booking, isPending, isError, error, refetch } = useBooking(id);
  const confirm = useConfirmBooking(id);
  const cancel = useCancelBooking(id);
  const [reason, setReason] = useState('');

  if (isPending) {
    return <p className="state">Cargando reserva…</p>;
  }

  if (isError) {
    return (
      <section>
        <ErrorAlert error={error} onRetry={() => void refetch()} />
        <Link to="/bookings">Volver al listado</Link>
      </section>
    );
  }

  const canConfirm = booking.status === 'Pending';
  const canCancel = booking.status === 'Pending' || booking.status === 'Confirmed';
  const busy = confirm.isPending || cancel.isPending;
  const actionError = confirm.error ?? cancel.error;

  return (
    <section>
      <div className="page-header">
        <h1>Reserva</h1>
        <StatusBadge status={booking.status} />
      </div>

      <dl className="details">
        <dt>Recurso</dt>
        <dd>{booking.resourceId}</dd>
        <dt>Cliente</dt>
        <dd>
          {booking.customerName} ({booking.customerEmail})
        </dd>
        <dt>Horario</dt>
        <dd>
          {formatLocal(booking.start)} – {formatLocal(booking.end)}
        </dd>
        <dt>Creada</dt>
        <dd>{formatLocal(booking.createdAt)}</dd>
        <dt>Confirmada</dt>
        <dd>{formatLocal(booking.confirmedAt)}</dd>
        <dt>Cancelada</dt>
        <dd>{formatLocal(booking.cancelledAt)}</dd>
        {booking.cancellationReason && (
          <>
            <dt>Motivo</dt>
            <dd>{booking.cancellationReason}</dd>
          </>
        )}
      </dl>

      {/* On failure the query is not invalidated, so the data above keeps showing the unchanged state (CA8). */}
      {actionError && <ErrorAlert error={actionError} />}

      {(canConfirm || canCancel) && (
        <div className="actions">
          {canConfirm && (
            <button
              type="button"
              className="button"
              disabled={busy}
              onClick={() => {
                cancel.reset();
                confirm.mutate();
              }}
            >
              Confirmar
            </button>
          )}
          {canCancel && (
            <div className="actions__cancel">
              <label>
                Motivo (opcional)
                <input value={reason} maxLength={100} onChange={(event) => setReason(event.target.value)} />
              </label>
              <button
                type="button"
                className="button button--danger"
                disabled={busy}
                onClick={() => {
                  confirm.reset();
                  cancel.mutate(reason);
                }}
              >
                Cancelar reserva
              </button>
            </div>
          )}
        </div>
      )}

      <Link to="/bookings">Volver al listado</Link>
    </section>
  );
}
