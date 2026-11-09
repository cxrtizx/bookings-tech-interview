// RFC 7807 ProblemDetails as returned by Bookings.Api (code + errors extensions).
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  code?: string;
  correlationId?: string;
  errors?: Record<string, string[]>;
}

// Readable messages per stable error code. The code is the contract; the API title is only a fallback.
const messagesByCode: Record<string, string> = {
  'booking.overlap': 'El recurso ya tiene una reserva confirmada que se solapa con este horario.',
  'booking.invalid_transition': 'La acción no está permitida en el estado actual de la reserva.',
  'booking.not_found': 'La reserva no existe.',
  'booking.start_in_past': 'El inicio debe ser una fecha futura.',
  'booking.invalid_date_range': 'El inicio debe ser anterior al fin.',
  validation_failed: 'Revisa los campos marcados.',
  bad_request: 'La petición no es válida.',
  internal_error: 'Se ha producido un error inesperado. Inténtalo de nuevo más tarde.',
  network_error: 'No se ha podido contactar con el servidor.',
};

export class ApiError extends Error {
  readonly status: number;
  readonly code: string;
  readonly fieldErrors: Record<string, string[]>;
  readonly correlationId?: string;

  constructor(status: number, problem: ProblemDetails) {
    const code = problem.code ?? (status >= 500 ? 'internal_error' : 'bad_request');
    super(messagesByCode[code] ?? problem.title ?? 'Error desconocido.');
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
    this.fieldErrors = problem.errors ?? {};
    this.correlationId = problem.correlationId;
  }

  static network(): ApiError {
    return new ApiError(0, { code: 'network_error' });
  }
}

export function toApiError(error: unknown): ApiError {
  return error instanceof ApiError ? error : new ApiError(500, { code: 'internal_error' });
}
