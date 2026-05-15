import { useQuery } from '@tanstack/react-query';
import { Navigate, Route, Routes } from 'react-router-dom';
import { LoginPage } from '../features/auth/pages/LoginPage';
import { Splash } from '../shared/components';
import { api } from '../shared/api';
import { Shell } from './layout/Shell';

export function AppRoutes() {
  const meQuery = useQuery({ queryKey: ['me'], queryFn: api.me });

  if (meQuery.isLoading) {
    return <Splash />;
  }

  if (!meQuery.data) {
    return (
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    );
  }

  return <Shell user={meQuery.data} />;
}
