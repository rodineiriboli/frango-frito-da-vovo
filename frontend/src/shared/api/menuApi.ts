import type { MenuCategory } from '../types';
import { request } from './httpClient';

export const menuApi = {
  getMenu: () => request<MenuCategory[]>('/menu'),
};
