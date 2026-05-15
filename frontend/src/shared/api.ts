import { authApi } from './api/authApi';
import { categoriesApi } from './api/categoriesApi';
import { customersApi } from './api/customersApi';
import { menuApi } from './api/menuApi';
import { ordersApi } from './api/ordersApi';
import { productsApi } from './api/productsApi';

export { ApiError } from './api/httpClient';
export type { ApiQuery } from './api/httpClient';

export const api = {
  ...authApi,
  ...categoriesApi,
  ...productsApi,
  ...customersApi,
  ...ordersApi,
  ...menuApi,
};
