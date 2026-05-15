import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Drumstick } from 'lucide-react';
import { api } from '../../../shared/api';
import { DataState, PageHeader, PanelTitle, StatusPill, Toolbar } from '../../../shared/components';
import { hasRole } from '../../../shared/constants/roles';
import type { AuthUser, Product } from '../../../shared/types';
import { money } from '../../../shared/utils/formatters';
import { ProductForm } from '../components/ProductForm';
import type { ProductFormData } from '../schemas/productSchema';

export function ProductsPage({ user }: { user: AuthUser }) {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [editing, setEditing] = useState<Product | null>(null);
  const isAdmin = hasRole(user, ['Admin']);
  const categories = useQuery({ queryKey: ['categories'], queryFn: api.getCategories });
  const products = useQuery({
    queryKey: ['products', search],
    queryFn: () => api.getProducts({ pageSize: 50, search }),
  });

  const mutation = useMutation({
    mutationFn: (payload: ProductFormData) =>
      editing ? api.updateProduct(editing.id, payload) : api.createProduct(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      queryClient.invalidateQueries({ queryKey: ['menu'] });
      setEditing(null);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: api.deleteProduct,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['products'] }),
  });

  return (
    <section className="page-grid">
      <PageHeader eyebrow="Cardápio" title="Produtos" description="Gerencie preços, disponibilidade e categorias do delivery." />
      <Toolbar value={search} onChange={setSearch} placeholder="Buscar produto" />

      <div className="split-layout">
        <div className="panel">
          <PanelTitle icon={Drumstick} title="Lista de produtos" />
          <DataState isLoading={products.isLoading} error={products.error} empty={!products.data?.items.length}>
            <div className="table-list">
              {products.data?.items.map((product) => (
                <article className="row-card" key={product.id}>
                  <div>
                    <strong>{product.name}</strong>
                    <span>{product.categoryName} - {money.format(product.price)}</span>
                  </div>
                  <StatusPill active={product.isActive} activeText="Ativo" inactiveText="Inativo" />
                  {isAdmin && (
                    <div className="row-actions">
                      <button type="button" className="ghost-button" onClick={() => setEditing(product)}>Editar</button>
                      <button type="button" className="danger-button" onClick={() => deleteMutation.mutate(product.id)}>Inativar</button>
                    </div>
                  )}
                </article>
              ))}
            </div>
          </DataState>
        </div>

        {isAdmin && (
          <ProductForm
            categories={categories.data ?? []}
            editing={editing}
            isSaving={mutation.isPending}
            error={mutation.error}
            onCancel={() => setEditing(null)}
            onSubmit={(values) => mutation.mutateAsync(values)}
          />
        )}
      </div>
    </section>
  );
}
