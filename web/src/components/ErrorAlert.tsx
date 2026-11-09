import { toApiError } from '../api/problemDetails';

type ErrorAlertProps = {
  error: unknown;
  onRetry?: () => void;
};

// Single place where API errors are rendered: readable message by error code + correlation id for support.
export function ErrorAlert({ error, onRetry }: ErrorAlertProps) {
  const apiError = toApiError(error);

  return (
    <div className="alert" role="alert">
      <p>{apiError.message}</p>
      {apiError.correlationId && <p className="alert__meta">Referencia: {apiError.correlationId}</p>}
      {onRetry && (
        <button type="button" className="button button--secondary" onClick={onRetry}>
          Reintentar
        </button>
      )}
    </div>
  );
}
