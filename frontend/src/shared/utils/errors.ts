import { ApiError } from '../api';

export function errorMessage(error: unknown) {
  return error instanceof ApiError ? error.message : 'Não foi possível concluir a operação.';
}
