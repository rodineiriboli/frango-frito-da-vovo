import type { PagedResult, Product } from '../types';
import { request, type ApiQuery } from './httpClient';

export const productsApi = {
  getProducts: (query?: ApiQuery) => request<PagedResult<Product>>('/products', { query }),
  createProduct: (payload: unknown) => request<Product>('/products', { method: 'POST', body: payload }),
  updateProduct: (id: string, payload: unknown) => request<Product>(`/products/${id}`, { method: 'PUT', body: payload }),
  deleteProduct: (id: string) => request<void>(`/products/${id}`, { method: 'DELETE' }),
};
