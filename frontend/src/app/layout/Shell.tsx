import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Navigate, NavLink, Route, Routes, useNavigate } from 'react-router-dom';
import {
  ClipboardList,
  Drumstick,
  LayoutDashboard,
  LogOut,
  Menu as MenuIcon,
  ShoppingBag,
  Tags,
  Users,
  X,
} from 'lucide-react';
import { CategoriesPage } from '../../features/categories/pages/CategoriesPage';
import { CustomersPage } from '../../features/customers/pages/CustomersPage';
import { DashboardPage } from '../../features/dashboard/pages/DashboardPage';
import { MenuPage } from '../../features/menu/pages/MenuPage';
import { OrdersPage } from '../../features/orders/pages/OrdersPage';
import { ProductsPage } from '../../features/products/pages/ProductsPage';
import { api } from '../../shared/api';
import { hasRole, roleLabels } from '../../shared/constants/roles';
import type { AuthUser, Role } from '../../shared/types';

export function Shell({ user }: { user: AuthUser }) {
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [mobileOpen, setMobileOpen] = useState(false);
  const logout = useMutation({
    mutationFn: api.logout,
    onSettled: () => {
      queryClient.removeQueries({
        predicate: (query) => query.queryKey[0] !== 'me',
      });
      queryClient.setQueryData(['me'], null);
      navigate('/login', { replace: true });
    },
  });

  const nav = [
    { to: '/', label: 'Dashboard', icon: LayoutDashboard, roles: ['Admin', 'Atendente', 'Cozinha', 'Entregador'] as Role[] },
    { to: '/orders', label: 'Pedidos', icon: ClipboardList, roles: ['Admin', 'Atendente', 'Cozinha', 'Entregador'] as Role[] },
    { to: '/products', label: 'Produtos', icon: Drumstick, roles: ['Admin', 'Atendente', 'Cozinha'] as Role[] },
    { to: '/categories', label: 'Categorias', icon: Tags, roles: ['Admin'] as Role[] },
    { to: '/customers', label: 'Clientes', icon: Users, roles: ['Admin', 'Atendente'] as Role[] },
    { to: '/menu', label: 'Cardápio', icon: ShoppingBag, roles: ['Admin', 'Atendente', 'Cozinha', 'Entregador'] as Role[] },
  ].filter((item) => hasRole(user, item.roles));

  const navContent = (
    <nav className="nav-list" aria-label="Principal">
      {nav.map((item) => (
        <NavLink key={item.to} to={item.to} end={item.to === '/'} onClick={() => setMobileOpen(false)}>
          <item.icon size={18} />
          <span>{item.label}</span>
        </NavLink>
      ))}
    </nav>
  );

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand-row brand-row--sidebar">
          <div className="brand-mark">FF</div>
          <div>
            <strong>Frango Frito</strong>
            <span>da Vovó</span>
          </div>
        </div>
        {navContent}
      </aside>

      <div className="workspace">
        <header className="topbar">
          <button className="icon-button mobile-only" type="button" onClick={() => setMobileOpen(true)} aria-label="Abrir menu">
            <MenuIcon size={22} />
          </button>
          <div>
            <strong>{user.fullName}</strong>
            <span>{user.roles.map((role) => roleLabels[role]).join(', ')}</span>
          </div>
          <button className="ghost-button" type="button" onClick={() => logout.mutate()}>
            <LogOut size={18} />
            Sair
          </button>
        </header>

        <main className="content">
          <Routes>
            <Route path="/" element={<DashboardPage user={user} />} />
            <Route path="/orders" element={<OrdersPage user={user} />} />
            <Route path="/products" element={<ProductsPage user={user} />} />
            <Route path="/categories" element={<CategoriesPage />} />
            <Route path="/customers" element={<CustomersPage />} />
            <Route path="/menu" element={<MenuPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </main>
      </div>

      {mobileOpen && (
        <div className="mobile-drawer" role="dialog" aria-modal="true">
          <div className="mobile-drawer__panel">
            <button className="icon-button" type="button" onClick={() => setMobileOpen(false)} aria-label="Fechar menu">
              <X size={22} />
            </button>
            {navContent}
          </div>
        </div>
      )}
    </div>
  );
}
