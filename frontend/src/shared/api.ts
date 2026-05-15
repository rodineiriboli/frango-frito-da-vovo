import type {
  AuthUser,
  Category,
  Customer,
  MenuCategory,
  Order,
  PagedResult,
  Product,
} from './types';

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

type RequestOptions = Omit<RequestInit, 'body'> & {
  body?: unknown;
  query?: Record<string, string | number | boolean | undefined | null>;
};

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
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
    const fallback = response.status === 401 ? 'Sessao expirada ou credenciais invalidas.' : 'Nao foi possivel concluir a operacao.';
    const message = typeof payload === 'object' && payload && 'detail' in payload
      ? String((payload as { detail?: string }).detail)
      : fallback;
    throw new ApiError(response.status, message, payload);
  }

  return payload as T;
}

export const api = {
  async login(email: string, password: string, rememberMe: boolean) {
    return request<AuthUser>('/auth/login', {
      method: 'POST',
      body: { email, password, rememberMe },
    });
  },
  async logout() {
    return request<void>('/auth/logout', { method: 'POST' });
  },
  async me() {
    try {
      return await request<AuthUser>('/auth/me');
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) {
        return null;
      }

      throw error;
    }
  },
  getCategories: () => request<Category[]>('/categories'),
  createCategory: (payload: Pick<Category, 'name' | 'description'>) =>
    request<Category>('/categories', { method: 'POST', body: payload }),
  updateCategory: (id: string, payload: Pick<Category, 'name' | 'description' | 'isActive'>) =>
    request<Category>(`/categories/${id}`, { method: 'PUT', body: payload }),
  deleteCategory: (id: string) => request<void>(`/categories/${id}`, { method: 'DELETE' }),
  getProducts: (query?: Record<string, string | number | boolean | undefined>) =>
    request<PagedResult<Product>>('/products', { query }),
  createProduct: (payload: unknown) => request<Product>('/products', { method: 'POST', body: payload }),
  updateProduct: (id: string, payload: unknown) => request<Product>(`/products/${id}`, { method: 'PUT', body: payload }),
  deleteProduct: (id: string) => request<void>(`/products/${id}`, { method: 'DELETE' }),
  getCustomers: (query?: Record<string, string | number | boolean | undefined>) =>
    request<PagedResult<Customer>>('/customers', { query }),
  createCustomer: (payload: unknown) => request<Customer>('/customers', { method: 'POST', body: payload }),
  updateCustomer: (id: string, payload: unknown) => request<Customer>(`/customers/${id}`, { method: 'PUT', body: payload }),
  deleteCustomer: (id: string) => request<void>(`/customers/${id}`, { method: 'DELETE' }),
  getOrders: (query?: Record<string, string | number | boolean | undefined>) =>
    request<PagedResult<Order>>('/orders', { query }),
  createOrder: (payload: unknown) => request<Order>('/orders', { method: 'POST', body: payload }),
  updateOrderStatus: (id: string, status: string) =>
    request<Order>(`/orders/${id}/status`, { method: 'PATCH', body: { status } }),
  getMenu: () => request<MenuCategory[]>('/menu'),
};
