const API_BASE_URL = import.meta.env.VITE_API_URL ?? '/api';

export class ApiError extends Error {
  status: number;
  details?: unknown;

  constructor(status: number, message: string, details?: unknown) {
    super(message);
    this.status = status;
    this.details = details;
  }
}

export type ApiQuery = Record<string, string | number | boolean | undefined | null>;

type RequestOptions = Omit<RequestInit, 'body'> & {
  body?: unknown;
  query?: ApiQuery;
};

export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const url = new URL(`${API_BASE_URL}${path}`, window.location.origin);

  if (options.query) {
    Object.entries(options.query).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        url.searchParams.set(key, String(value));
      }
    });
  }

  const headers = new Headers(options.headers);
  const body = options.body === undefined ? undefined : JSON.stringify(options.body);

  if (body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  const response = await fetch(url, {
    ...options,
    body,
    headers,
    credentials: 'include',
  });

  if (response.status === 204) {
    return undefined as T;
  }

  const isJson = response.headers.get('content-type')?.includes('json');
  const payload = isJson ? await response.json() : await response.text();

  if (!response.ok) {
    const fallback = response.status === 401 ? 'Sessão expirada ou credenciais inválidas.' : 'Não foi possível concluir a operação.';
    const message = typeof payload === 'object' && payload && ('detail' in payload || 'title' in payload)
      ? String((payload as { detail?: string; title?: string }).detail ?? (payload as { title?: string }).title)
      : fallback;
    throw new ApiError(response.status, message, payload);
  }

  return payload as T;
}
