import type { Customer, PagedResult } from '../types';
import { request, type ApiQuery } from './httpClient';

export const customersApi = {
  getCustomers: (query?: ApiQuery) => request<PagedResult<Customer>>('/customers', { query }),
  createCustomer: (payload: unknown) => request<Customer>('/customers', { method: 'POST', body: payload }),
  updateCustomer: (id: string, payload: unknown) => request<Customer>(`/customers/${id}`, { method: 'PUT', body: payload }),
  deleteCustomer: (id: string) => request<void>(`/customers/${id}`, { method: 'DELETE' }),
};
