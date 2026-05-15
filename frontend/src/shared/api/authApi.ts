import type { AuthUser } from '../types';
import { ApiError, request } from './httpClient';

export const authApi = {
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
};
