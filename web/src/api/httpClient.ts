import { ApiError, type ProblemDetails } from './problemDetails';

// The only place in the app that calls fetch. Components and hooks go through bookingsApi.
const baseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') ?? '';

type QueryValue = string | number | undefined | null;

export function buildQuery(params: Record<string, QueryValue>): string {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== '') {
      search.set(key, String(value));
    }
  }
  const query = search.toString();
  return query ? `?${query}` : '';
}

export async function request<TResponse>(path: string, init: RequestInit = {}): Promise<TResponse> {
  let response: Response;
  try {
    response = await fetch(`${baseUrl}${path}`, {
      ...init,
      headers: {
        Accept: 'application/json',
        ...(init.body ? { 'Content-Type': 'application/json' } : {}),
        ...init.headers,
      },
    });
  } catch {
    throw ApiError.network();
  }

  if (!response.ok) {
    throw new ApiError(response.status, await readProblem(response));
  }

  const text = await response.text();
  return (text ? JSON.parse(text) : undefined) as TResponse;
}

async function readProblem(response: Response): Promise<ProblemDetails> {
  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    // e.g. 404 from an unmatched route has no body
    return { status: response.status, code: response.status === 404 ? 'booking.not_found' : undefined };
  }
}
