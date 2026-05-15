import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ClipboardList, PackagePlus } from 'lucide-react';
import { api } from '../../../shared/api';
import { DataState, Field, PageHeader, PanelTitle } from '../../../shared/components';
import { hasRole } from '../../../shared/constants/roles';
import type { AuthUser, OrderStatus } from '../../../shared/types';
import { errorMessage } from '../../../shared/utils/errors';
import { OrderCard } from '../components/OrderCard';

export function OrdersPage({ user }: { user: AuthUser }) {
  const queryClient = useQueryClient();
  const [customerId, setCustomerId] = useState('');
  const [productId, setProductId] = useState('');
  const [quantity, setQuantity] = useState(1);
  const [cart, setCart] = useState<Array<{ productId: string; quantity: number }>>([]);
  const orders = useQuery({ queryKey: ['orders'], queryFn: () => api.getOrders({ pageSize: 100 }) });
  const customers = useQuery({
    queryKey: ['customers', 'orders'],
    queryFn: () => api.getCustomers({ pageSize: 100 }),
    enabled: hasRole(user, ['Admin', 'Atendente']),
  });
  const products = useQuery({ queryKey: ['products', 'active'], queryFn: () => api.getProducts({ pageSize: 100, active: true }) });

  useEffect(() => {
    if (!customerId && customers.data?.items[0]) {
      setCustomerId(customers.data.items[0].id);
    }
    if (!productId && products.data?.items[0]) {
      setProductId(products.data.items[0].id);
    }
  }, [customerId, customers.data?.items, productId, products.data?.items]);

  const createOrder = useMutation({
    mutationFn: () => api.createOrder({ customerId, items: cart }),
    onSuccess: () => {
      setCart([]);
      queryClient.invalidateQueries({ queryKey: ['orders'] });
    },
  });
  const statusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: OrderStatus }) => api.updateOrderStatus(id, status),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['orders'] }),
  });

  const productMap = useMemo(
    () => new Map((products.data?.items ?? []).map((product) => [product.id, product])),
    [products.data?.items],
  );

  function addCartItem() {
    if (!productId || quantity <= 0) {
      return;
    }

    setCart((current) => {
      const existing = current.find((item) => item.productId === productId);
      if (existing) {
        return current.map((item) => item.productId === productId ? { ...item, quantity: item.quantity + quantity } : item);
      }

      return [...current, { productId, quantity }];
    });
    setQuantity(1);
  }

  return (
    <section className="page-grid">
      <PageHeader eyebrow="Pedidos" title="Fluxo de entrega" description="Crie pedidos e avance status conforme responsabilidade de cada perfil." />
      <div className="split-layout">
        <div className="panel">
          <PanelTitle icon={ClipboardList} title="Pedidos recentes" />
          <DataState isLoading={orders.isLoading} error={orders.error} empty={!orders.data?.items.length}>
            <div className="order-list">
              {orders.data?.items.map((order) => (
                <OrderCard
                  key={order.id}
                  order={order}
                  user={user}
                  isSaving={statusMutation.isPending}
                  onStatus={(status) => statusMutation.mutate({ id: order.id, status })}
                />
              ))}
            </div>
          </DataState>
        </div>

        {hasRole(user, ['Admin', 'Atendente']) && (
          <aside className="panel form-panel">
            <PanelTitle icon={PackagePlus} title="Novo pedido" />
            <Field label="Cliente">
              <select value={customerId} onChange={(event) => setCustomerId(event.target.value)}>
                {customers.data?.items.map((customer) => (
                  <option key={customer.id} value={customer.id}>{customer.name}</option>
                ))}
              </select>
            </Field>
            <div className="two-columns">
              <Field label="Produto">
                <select value={productId} onChange={(event) => setProductId(event.target.value)}>
                  {products.data?.items.map((product) => (
                    <option key={product.id} value={product.id}>{product.name}</option>
                  ))}
                </select>
              </Field>
              <Field label="Qtd.">
                <input type="number" min={1} value={quantity} onChange={(event) => setQuantity(Number(event.target.value))} />
              </Field>
            </div>
            <button className="ghost-button" type="button" onClick={addCartItem}>Adicionar item</button>
            <div className="cart-list">
              {cart.map((item) => {
                const product = productMap.get(item.productId);
                return (
                  <div className="cart-row" key={item.productId}>
                    <span>{product?.name ?? item.productId}</span>
                    <strong>{item.quantity}x</strong>
                  </div>
                );
              })}
              {cart.length === 0 && <span className="muted">Nenhum item adicionado</span>}
            </div>
            {createOrder.error && <p className="form-error">{errorMessage(createOrder.error)}</p>}
            <button className="primary-button" type="button" disabled={!customerId || cart.length === 0 || createOrder.isPending} onClick={() => createOrder.mutate()}>
              Criar pedido
            </button>
          </aside>
        )}
      </div>
    </section>
  );
}
