import type { OrderStatus } from '../types';

export const statusLabels: Record<OrderStatus, string> = {
  Received: 'Recebido',
  Preparing: 'Preparando',
  OutForDelivery: 'Saiu para entrega',
  Delivered: 'Entregue',
  Cancelled: 'Cancelado',
};
