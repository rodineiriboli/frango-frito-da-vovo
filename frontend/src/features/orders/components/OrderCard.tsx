import { statusLabels } from '../../../shared/constants/orderStatus';
import type { AuthUser, Order, OrderStatus } from '../../../shared/types';
import { money } from '../../../shared/utils/formatters';
import { getStatusActions } from '../utils/orderStatus';

export function OrderCard({
  order,
  user,
  isSaving,
  onStatus,
}: {
  order: Order;
  user: AuthUser;
  isSaving: boolean;
  onStatus: (status: OrderStatus) => void;
}) {
  const actions = getStatusActions(order, user);
  return (
    <article className="order-card">
      <div className="order-card__head">
        <div>
          <strong>{order.customerName}</strong>
          <span>{order.totalItems} itens - {money.format(order.total)}</span>
        </div>
        <span className={`status-badge status-${order.status}`}>{statusLabels[order.status]}</span>
      </div>
      <ul>
        {order.items.map((item) => (
          <li key={item.id}>{item.quantity}x {item.productName}</li>
        ))}
      </ul>
      {actions.length > 0 && (
        <div className="row-actions">
          {actions.map((status) => (
            <button key={status} type="button" className="ghost-button" disabled={isSaving} onClick={() => onStatus(status)}>
              {statusLabels[status]}
            </button>
          ))}
        </div>
      )}
    </article>
  );
}
