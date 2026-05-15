import type { AuthUser, Role } from '../types';

export const roleLabels: Record<Role, string> = {
  Admin: 'Admin',
  Atendente: 'Atendente',
  Cozinha: 'Cozinha',
  Entregador: 'Entregador',
};

export function hasRole(user: AuthUser, roles: Role[]) {
  return roles.some((role) => user.roles.includes(role));
}
