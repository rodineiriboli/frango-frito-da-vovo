import { useEffect } from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { Tags } from 'lucide-react';
import { Field, PanelTitle } from '../../../shared/components';
import type { Category } from '../../../shared/types';
import { errorMessage } from '../../../shared/utils/errors';
import { categorySchema, type CategoryFormData, type CategoryFormInput } from '../schemas/categorySchema';

const defaultCategoryValues: CategoryFormInput = { name: '', description: '', isActive: true };

export function CategoryForm({
  editing,
  isSaving,
  error,
  onCancel,
  onSubmit,
}: {
  editing: Category | null;
  isSaving: boolean;
  error: unknown;
  onCancel: () => void;
  onSubmit: (values: CategoryFormData) => Promise<unknown>;
}) {
  const { register, handleSubmit, reset, formState } = useForm<CategoryFormInput, unknown, CategoryFormData>({
    resolver: zodResolver(categorySchema),
    defaultValues: defaultCategoryValues,
  });

  useEffect(() => {
    reset(editing ? {
      name: editing.name,
      description: editing.description,
      isActive: editing.isActive,
    } : defaultCategoryValues);
  }, [editing, reset]);

  async function handleValidSubmit(values: CategoryFormData) {
    const wasEditing = Boolean(editing);
    await onSubmit(values);

    if (!wasEditing) {
      reset(defaultCategoryValues);
    }
  }

  return (
    <form className="panel form-panel" onSubmit={handleSubmit(handleValidSubmit)}>
      <PanelTitle icon={Tags} title={editing ? 'Editar categoria' : 'Nova categoria'} />
      <Field label="Nome" error={formState.errors.name?.message}>
        <input {...register('name')} />
      </Field>
      <Field label="Descrição">
        <textarea rows={3} {...register('description')} />
      </Field>
      <label className="check-row">
        <input type="checkbox" {...register('isActive')} />
        Categoria ativa
      </label>
      {error ? <p className="form-error">{errorMessage(error)}</p> : null}
      <div className="form-actions">
        {editing && <button type="button" className="ghost-button" onClick={onCancel}>Cancelar</button>}
        <button className="primary-button" type="submit" disabled={isSaving}>Salvar</button>
      </div>
    </form>
  );
}
