import type { Order, PagedResult } from '../types';
import { request, type ApiQuery } from './httpClient';

export const ordersApi = {
  getOrders: (query?: ApiQuery) => request<PagedResult<Order>>('/orders', { query }),
  createOrder: (payload: unknown) => request<Order>('/orders', { method: 'POST', body: payload }),
  updateOrderStatus: (id: string, status: string) =>
    request<Order>(`/orders/${id}/status`, { method: 'PATCH', body: { status } }),
};
