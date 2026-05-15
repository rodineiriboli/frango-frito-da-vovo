import type { AuthUser, Order, OrderStatus } from '../../../shared/types';

export function getStatusActions(order: Order, user: AuthUser): OrderStatus[] {
  if (order.status === 'Delivered' || order.status === 'Cancelled') {
    return [];
  }

  if (user.roles.includes('Admin')) {
    return nextStatuses(order.status);
  }

  if (user.roles.includes('Atendente')) {
    return ['Cancelled'];
  }

  if (user.roles.includes('Cozinha') && order.status === 'Received') {
    return ['Preparing'];
  }

  if (user.roles.includes('Entregador') && order.status === 'Preparing') {
    return ['OutForDelivery'];
  }

  if (user.roles.includes('Entregador') && order.status === 'OutForDelivery') {
    return ['Delivered'];
  }

  return [];
}

function nextStatuses(status: OrderStatus): OrderStatus[] {
  switch (status) {
    case 'Received':
      return ['Preparing', 'Cancelled'];
    case 'Preparing':
      return ['OutForDelivery', 'Cancelled'];
    case 'OutForDelivery':
      return ['Delivered', 'Cancelled'];
    default:
      return [];
  }
}
