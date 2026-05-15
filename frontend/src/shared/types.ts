export type Role = 'Admin' | 'Atendente' | 'Cozinha' | 'Entregador';

export type AuthUser = {
  id: string;
  fullName: string;
  email: string;
  roles: Role[];
};

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
};

export type Category = {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
};

export type Product = {
  id: string;
  name: string;
  description: string;
  price: number;
  isActive: boolean;
  imageUrl: string;
  categoryId: string;
  categoryName: string;
  createdAt: string;
  updatedAt?: string | null;
};

export type Address = {
  street: string;
  number: string;
  neighborhood: string;
  city: string;
  state: string;
  zipCode: string;
  complement: string;
};

export type Customer = {
  id: string;
  name: string;
  phone: string;
  address: Address;
  createdAt: string;
  updatedAt?: string | null;
};

export type OrderStatus = 'Received' | 'Preparing' | 'OutForDelivery' | 'Delivered' | 'Cancelled';

export type OrderItem = {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  total: number;
};

export type Order = {
  id: string;
  customerId: string;
  customerName: string;
  status: OrderStatus;
  total: number;
  totalItems: number;
  createdAt: string;
  updatedAt?: string | null;
  items: OrderItem[];
};

export type MenuCategory = {
  id: string;
  name: string;
  description: string;
  products: MenuProduct[];
};

export type MenuProduct = {
  id: string;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
};
