import '@testing-library/jest-dom/vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { afterEach, describe, expect, it, vi } from 'vitest';
import App from './App';

describe('App', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('shows login when the user is not authenticated', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify({}), {
      status: 401,
      headers: { 'Content-Type': 'application/json' },
    })));

    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });

    render(
      <QueryClientProvider client={queryClient}>
        <App />
      </QueryClientProvider>,
    );

    expect(await screen.findByText('Backoffice')).toBeInTheDocument();
    expect(screen.getByText('Ambiente administrativo')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Entrar' })).toBeInTheDocument();
  });

  it('redirects to login after logout', async () => {
    vi.stubGlobal('fetch', vi.fn(async (input, init) => {
      const url = String(input);

      if (url.includes('/auth/me')) {
        return new Response(JSON.stringify({
          id: '019e28f8-c0dd-76bb-a60e-ac0468b87a78',
          fullName: 'Admin Vovo',
          email: 'admin@frangofrito.local',
          roles: ['Admin'],
        }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        });
      }

      if (url.includes('/auth/logout') && init?.method === 'POST') {
        return new Response(null, { status: 204 });
      }

      return new Response(JSON.stringify({
        items: [],
        page: 1,
        pageSize: 100,
        totalItems: 0,
        totalPages: 0,
      }), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      });
    }));

    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });

    render(
      <QueryClientProvider client={queryClient}>
        <App />
      </QueryClientProvider>,
    );

    fireEvent.click(await screen.findByRole('button', { name: 'Sair' }));

    expect(screen.getByRole('button', { name: 'Entrar' })).toBeInTheDocument();
  });
});
