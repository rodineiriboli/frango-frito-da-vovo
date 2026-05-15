import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { api } from '../../../shared/api';
import { errorMessage } from '../../../shared/utils/errors';

type LoginFormData = {
  email: string;
  password: string;
  rememberMe: boolean;
};

export function LoginPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [formError, setFormError] = useState<string | null>(null);
  const { register, handleSubmit, formState } = useForm<LoginFormData>({
    defaultValues: {
      email: 'admin@frangofrito.local',
      password: 'Vovo@12345',
      rememberMe: true,
    },
  });

  const mutation = useMutation({
    mutationFn: (payload: LoginFormData) => api.login(payload.email, payload.password, payload.rememberMe),
    onSuccess: (user) => {
      queryClient.setQueryData(['me'], user);
      navigate('/');
    },
    onError: (error) => setFormError(errorMessage(error)),
  });

  return (
    <main className="login-screen">
      <section className="login-card" aria-label="Entrar">
        <div className="brand-row">
          <div className="brand-mark">FF</div>
          <div>
            <strong>Backoffice</strong>
            <span>Ambiente administrativo</span>
          </div>
        </div>

        <form
          onSubmit={handleSubmit((values) => {
            setFormError(null);
            mutation.mutate(values);
          })}
          className="stack"
        >
          <label>
            E-mail
            <input type="email" autoComplete="email" {...register('email', { required: true })} />
          </label>
          <label>
            Senha
            <input type="password" autoComplete="current-password" {...register('password', { required: true })} />
          </label>
          <label className="check-row">
            <input type="checkbox" {...register('rememberMe')} />
            Manter sessão ativa
          </label>
          {formError && <p className="form-error">{formError}</p>}
          <button className="primary-button" type="submit" disabled={mutation.isPending || formState.isSubmitting}>
            Entrar
          </button>
        </form>
      </section>
    </main>
  );
}
