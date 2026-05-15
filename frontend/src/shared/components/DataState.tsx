import type { ReactNode } from 'react';
import { errorMessage } from '../utils/errors';

export function DataState({
  isLoading,
  error,
  empty,
  children,
}: {
  isLoading: boolean;
  error: unknown;
  empty: boolean;
  children: ReactNode;
}) {
  if (isLoading) {
    return <p className="muted">Carregando dados...</p>;
  }

  if (error) {
    return <p className="form-error">{errorMessage(error)}</p>;
  }

  if (empty) {
    return <p className="muted">Nenhum registro encontrado.</p>;
  }

  return <>{children}</>;
}
