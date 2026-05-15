import type { Category } from '../types';
import { request } from './httpClient';

export const categoriesApi = {
  getCategories: () => request<Category[]>('/categories'),
  createCategory: (payload: Pick<Category, 'name' | 'description'>) =>
    request<Category>('/categories', { method: 'POST', body: payload }),
  updateCategory: (id: string, payload: Pick<Category, 'name' | 'description' | 'isActive'>) =>
    request<Category>(`/categories/${id}`, { method: 'PUT', body: payload }),
  deleteCategory: (id: string) => request<void>(`/categories/${id}`, { method: 'DELETE' }),
};
