import { useEffect } from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { PackagePlus } from 'lucide-react';
import { Field, PanelTitle } from '../../../shared/components';
import type { Category, Product } from '../../../shared/types';
import { errorMessage } from '../../../shared/utils/errors';
import { productSchema, type ProductFormData, type ProductFormInput } from '../schemas/productSchema';

function getDefaultProductValues(categories: Category[]): ProductFormInput {
  return {
    name: '',
    description: '',
    price: 1,
    categoryId: categories[0]?.id ?? '',
    isActive: true,
    imageUrl: '',
  };
}

export function ProductForm({
  categories,
  editing,
  isSaving,
  error,
  onCancel,
  onSubmit,
}: {
  categories: Category[];
  editing: Product | null;
  isSaving: boolean;
  error: unknown;
  onCancel: () => void;
  onSubmit: (values: ProductFormData) => Promise<unknown>;
}) {
  const { register, handleSubmit, reset, formState } = useForm<ProductFormInput, unknown, ProductFormData>({
    resolver: zodResolver(productSchema),
    defaultValues: getDefaultProductValues(categories),
  });

  useEffect(() => {
    reset(editing
      ? {
          name: editing.name,
          description: editing.description,
          price: editing.price,
          categoryId: editing.categoryId,
          isActive: editing.isActive,
          imageUrl: editing.imageUrl,
        }
      : {
          ...getDefaultProductValues(categories),
        });
  }, [categories, editing, reset]);

  async function handleValidSubmit(values: ProductFormData) {
    const wasEditing = Boolean(editing);
    await onSubmit(values);

    if (!wasEditing) {
      reset(getDefaultProductValues(categories));
    }
  }

  return (
    <form className="panel form-panel" onSubmit={handleSubmit(handleValidSubmit)}>
      <PanelTitle icon={PackagePlus} title={editing ? 'Editar produto' : 'Novo produto'} />
      <Field label="Nome" error={formState.errors.name?.message}>
        <input {...register('name')} />
      </Field>
      <Field label="Descrição" error={formState.errors.description?.message}>
        <textarea rows={3} {...register('description')} />
      </Field>
      <div className="two-columns">
        <Field label="Preço" error={formState.errors.price?.message}>
          <input type="number" step="0.01" {...register('price')} />
        </Field>
        <Field label="Categoria" error={formState.errors.categoryId?.message}>
          <select {...register('categoryId')}>
            {categories.map((category) => (
              <option key={category.id} value={category.id}>{category.name}</option>
            ))}
          </select>
        </Field>
      </div>
      <Field label="Imagem URL">
        <input {...register('imageUrl')} />
      </Field>
      <label className="check-row">
        <input type="checkbox" {...register('isActive')} />
        Produto ativo
      </label>
      {error ? <p className="form-error">{errorMessage(error)}</p> : null}
      <div className="form-actions">
        {editing && <button type="button" className="ghost-button" onClick={onCancel}>Cancelar</button>}
        <button className="primary-button" type="submit" disabled={isSaving}>Salvar</button>
      </div>
    </form>
  );
}
