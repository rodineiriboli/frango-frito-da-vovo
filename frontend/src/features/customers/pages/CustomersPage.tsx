import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Users } from 'lucide-react';
import { api } from '../../../shared/api';
import { DataState, PageHeader, PanelTitle, Toolbar } from '../../../shared/components';
import type { Customer } from '../../../shared/types';
import { CustomerForm } from '../components/CustomerForm';
import type { CustomerFormData } from '../schemas/customerSchema';

export function CustomersPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [editing, setEditing] = useState<Customer | null>(null);
  const customers = useQuery({
    queryKey: ['customers', search],
    queryFn: () => api.getCustomers({ pageSize: 50, search }),
  });
  const mutation = useMutation({
    mutationFn: (payload: CustomerFormData) => editing ? api.updateCustomer(editing.id, payload) : api.createCustomer(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['customers'] });
      setEditing(null);
    },
  });

  return (
    <section className="page-grid">
      <PageHeader eyebrow="Atendimento" title="Clientes" description="Cadastre contatos e endereços para acelerar novos pedidos." />
      <Toolbar value={search} onChange={setSearch} placeholder="Buscar cliente" />
      <div className="split-layout">
        <div className="panel">
          <PanelTitle icon={Users} title="Base de clientes" />
          <DataState isLoading={customers.isLoading} error={customers.error} empty={!customers.data?.items.length}>
            <div className="table-list">
              {customers.data?.items.map((customer) => (
                <article className="row-card" key={customer.id}>
                  <div>
                    <strong>{customer.name}</strong>
                    <span>{customer.phone} - {customer.address.neighborhood}</span>
                  </div>
                  <button type="button" className="ghost-button" onClick={() => setEditing(customer)}>Editar</button>
                </article>
              ))}
            </div>
          </DataState>
        </div>
        <CustomerForm editing={editing} isSaving={mutation.isPending} error={mutation.error} onCancel={() => setEditing(null)} onSubmit={(values) => mutation.mutateAsync(values)} />
      </div>
    </section>
  );
}
