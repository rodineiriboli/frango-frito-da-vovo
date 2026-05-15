import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Tags } from 'lucide-react';
import { api } from '../../../shared/api';
import { DataState, PageHeader, PanelTitle, StatusPill } from '../../../shared/components';
import type { Category } from '../../../shared/types';
import { errorMessage } from '../../../shared/utils/errors';
import { CategoryForm } from '../components/CategoryForm';
import type { CategoryFormData } from '../schemas/categorySchema';

export function CategoriesPage() {
  const queryClient = useQueryClient();
  const [editing, setEditing] = useState<Category | null>(null);
  const categories = useQuery({ queryKey: ['categories'], queryFn: api.getCategories });
  const mutation = useMutation({
    mutationFn: (payload: CategoryFormData) => editing ? api.updateCategory(editing.id, payload) : api.createCategory(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      setEditing(null);
    },
  });
  const deleteMutation = useMutation({
    mutationFn: api.deleteCategory,
    onSuccess: (_result, categoryId) => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      if (editing?.id === categoryId) {
        setEditing(null);
      }
    },
  });

  function handleDelete(category: Category) {
    const confirmed = window.confirm(`Excluir a categoria "${category.name}"? Essa ação só será permitida se ela não estiver vinculada a produtos.`);
    if (confirmed) {
      deleteMutation.mutate(category.id);
    }
  }

  return (
    <section className="page-grid">
      <PageHeader eyebrow="Administração" title="Categorias" description="Organize o cardápio para facilitar compra e operação." />
      <div className="split-layout">
        <div className="panel">
          <PanelTitle icon={Tags} title="Categorias cadastradas" />
          {deleteMutation.error ? <p className="form-error">{errorMessage(deleteMutation.error)}</p> : null}
          <DataState isLoading={categories.isLoading} error={categories.error} empty={!categories.data?.length}>
            <div className="table-list">
              {categories.data?.map((category) => (
                <article className="row-card" key={category.id}>
                  <div>
                    <strong>{category.name}</strong>
                    <span>{category.description || 'Sem descrição'}</span>
                  </div>
                  <StatusPill active={category.isActive} activeText="Ativa" inactiveText="Inativa" />
                  <div className="row-actions">
                    <button type="button" className="ghost-button" onClick={() => setEditing(category)}>Editar</button>
                    <button
                      type="button"
                      className="danger-button"
                      disabled={deleteMutation.isPending}
                      onClick={() => handleDelete(category)}
                    >
                      Excluir
                    </button>
                  </div>
                </article>
              ))}
            </div>
          </DataState>
        </div>
        <CategoryForm editing={editing} isSaving={mutation.isPending} error={mutation.error} onCancel={() => setEditing(null)} onSubmit={(values) => mutation.mutateAsync(values)} />
      </div>
    </section>
  );
}
