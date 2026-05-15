import { useEffect } from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { Users } from 'lucide-react';
import { Field, PanelTitle } from '../../../shared/components';
import type { Customer } from '../../../shared/types';
import { errorMessage } from '../../../shared/utils/errors';
import { customerSchema, type CustomerFormData, type CustomerFormInput } from '../schemas/customerSchema';

function getDefaultCustomerValues(): CustomerFormInput {
  return {
    name: '',
    phone: '',
    address: {
      street: '',
      number: '',
      neighborhood: '',
      city: 'São Paulo',
      state: 'SP',
      zipCode: '',
      complement: '',
    },
  };
}

export function CustomerForm({
  editing,
  isSaving,
  error,
  onCancel,
  onSubmit,
}: {
  editing: Customer | null;
  isSaving: boolean;
  error: unknown;
  onCancel: () => void;
  onSubmit: (values: CustomerFormData) => Promise<unknown>;
}) {
  const { register, handleSubmit, reset, formState } = useForm<CustomerFormInput, unknown, CustomerFormData>({
    resolver: zodResolver(customerSchema),
    defaultValues: getDefaultCustomerValues(),
  });

  useEffect(() => {
    reset(editing ? {
      name: editing.name,
      phone: editing.phone,
      address: editing.address,
    } : getDefaultCustomerValues());
  }, [editing, reset]);

  async function handleValidSubmit(values: CustomerFormData) {
    const wasEditing = Boolean(editing);
    await onSubmit(values);

    if (!wasEditing) {
      reset(getDefaultCustomerValues());
    }
  }

  return (
    <form className="panel form-panel" onSubmit={handleSubmit(handleValidSubmit)}>
      <PanelTitle icon={Users} title={editing ? 'Editar cliente' : 'Novo cliente'} />
      <Field label="Nome" error={formState.errors.name?.message}>
        <input {...register('name')} />
      </Field>
      <Field label="Telefone" error={formState.errors.phone?.message}>
        <input {...register('phone')} />
      </Field>
      <div className="two-columns">
        <Field label="Rua" error={formState.errors.address?.street?.message}>
          <input {...register('address.street')} />
        </Field>
        <Field label="Número" error={formState.errors.address?.number?.message}>
          <input {...register('address.number')} />
        </Field>
      </div>
      <div className="two-columns">
        <Field label="Bairro" error={formState.errors.address?.neighborhood?.message}>
          <input {...register('address.neighborhood')} />
        </Field>
        <Field label="CEP" error={formState.errors.address?.zipCode?.message}>
          <input {...register('address.zipCode')} />
        </Field>
      </div>
      <div className="two-columns">
        <Field label="Cidade" error={formState.errors.address?.city?.message}>
          <input {...register('address.city')} />
        </Field>
        <Field label="UF" error={formState.errors.address?.state?.message}>
          <input maxLength={2} {...register('address.state')} />
        </Field>
      </div>
      <Field label="Complemento">
        <input {...register('address.complement')} />
      </Field>
      {error ? <p className="form-error">{errorMessage(error)}</p> : null}
      <div className="form-actions">
        {editing && <button type="button" className="ghost-button" onClick={onCancel}>Cancelar</button>}
        <button className="primary-button" type="submit" disabled={isSaving}>Salvar</button>
      </div>
    </form>
  );
}
