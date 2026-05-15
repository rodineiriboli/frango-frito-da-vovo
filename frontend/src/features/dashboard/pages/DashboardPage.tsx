import { useQuery } from '@tanstack/react-query';
import { BadgeDollarSign, ClipboardList, Drumstick, Users } from 'lucide-react';
import { api } from '../../../shared/api';
import { DataState, Metric, PageHeader } from '../../../shared/components';
import { statusLabels } from '../../../shared/constants/orderStatus';
import { hasRole } from '../../../shared/constants/roles';
import type { AuthUser, OrderStatus } from '../../../shared/types';
import { money } from '../../../shared/utils/formatters';

export function DashboardPage({ user }: { user: AuthUser }) {
  const orders = useQuery({ queryKey: ['orders', 'dashboard'], queryFn: () => api.getOrders({ pageSize: 100 }) });
  const products = useQuery({ queryKey: ['products', 'dashboard'], queryFn: () => api.getProducts({ pageSize: 100 }) });
  const customers = useQuery({
    queryKey: ['customers', 'dashboard'],
    queryFn: () => api.getCustomers({ pageSize: 100 }),
    enabled: hasRole(user, ['Admin', 'Atendente']),
  });

  const orderItems = orders.data?.items ?? [];
  const revenue = orderItems
    .filter((order) => order.status !== 'Cancelled')
    .reduce((sum, order) => sum + order.total, 0);

  return (
    <section className="page-grid">
      <PageHeader
        eyebrow="Operação"
        title="Visão geral"
        description="Indicadores rápidos para acompanhar cardápio, pedidos e atendimento."
      />

      <div className="metric-grid">
        <Metric icon={ClipboardList} label="Pedidos" value={orders.data?.totalItems ?? 0} tone="red" />
        <Metric icon={BadgeDollarSign} label="Receita em pedidos" value={money.format(revenue)} tone="yellow" />
        <Metric icon={Drumstick} label="Produtos ativos" value={products.data?.items.filter((item) => item.isActive).length ?? 0} />
        <Metric icon={Users} label="Clientes" value={customers.data?.totalItems ?? '-'} />
      </div>

      <DataState isLoading={orders.isLoading} error={orders.error} empty={false}>
        <div className="kanban">
          {(['Received', 'Preparing', 'OutForDelivery', 'Delivered'] satisfies OrderStatus[]).map((status) => (
            <article className="kanban-column" key={status}>
              <h3>{statusLabels[status]}</h3>
              {orderItems.filter((order) => order.status === status).slice(0, 4).map((order) => (
                <div className="mini-order" key={order.id}>
                  <strong>{order.customerName}</strong>
                  <span>{order.totalItems} itens - {money.format(order.total)}</span>
                </div>
              ))}
              {orderItems.filter((order) => order.status === status).length === 0 && <span className="muted">Sem pedidos</span>}
            </article>
          ))}
        </div>
      </DataState>
    </section>
  );
}
