import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { BrowserRouter, Navigate, NavLink, Route, Routes, useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import {
  BadgeDollarSign,
  ChefHat,
  ClipboardList,
  Drumstick,
  LayoutDashboard,
  LogOut,
  Menu as MenuIcon,
  PackagePlus,
  Search,
  ShoppingBag,
  Tags,
  Users,
  X,
  type LucideIcon,
} from 'lucide-react';
import { api, ApiError } from './shared/api';
import type { AuthUser, Category, Customer, Order, OrderStatus, Product, Role } from './shared/types';

const money = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });

const statusLabels: Record<OrderStatus, string> = {
  Received: 'Recebido',
  Preparing: 'Preparando',
  OutForDelivery: 'Saiu para entrega',
  Delivered: 'Entregue',
  Cancelled: 'Cancelado',
};

const roleLabels: Record<Role, string> = {
  Admin: 'Admin',
  Atendente: 'Atendente',
  Cozinha: 'Cozinha',
  Entregador: 'Entregador',
};

function hasRole(user: AuthUser, roles: Role[]) {
  return roles.some((role) => user.roles.includes(role));
}

function errorMessage(error: unknown) {
  return error instanceof ApiError ? error.message : 'Nao foi possivel concluir a operacao.';
}

export default function App() {
  return (
    <BrowserRouter>
      <AppRoutes />
    </BrowserRouter>
  );
}

function AppRoutes() {
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

function Splash() {
  return (
    <main className="splash">
      <div className="brand-mark">FF</div>
      <p>Carregando operacao...</p>
    </main>
  );
}

function LoginPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [formError, setFormError] = useState<string | null>(null);
  const { register, handleSubmit, formState } = useForm({
    defaultValues: {
      email: 'admin@frangofrito.local',
      password: 'Vovo@12345',
      rememberMe: true,
    },
  });

  const mutation = useMutation({
    mutationFn: (payload: { email: string; password: string; rememberMe: boolean }) =>
      api.login(payload.email, payload.password, payload.rememberMe),
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
            Manter sessao ativa
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

function Shell({ user }: { user: AuthUser }) {
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
    { to: '/menu', label: 'Cardapio', icon: ShoppingBag, roles: ['Admin', 'Atendente', 'Cozinha', 'Entregador'] as Role[] },
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
            <span>da Vovo</span>
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
            <Route path="/" element={<Dashboard user={user} />} />
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

function Dashboard({ user }: { user: AuthUser }) {
  const orders = useQuery({ queryKey: ['orders', 'dashboard'], queryFn: () => api.getOrders({ pageSize: 100 }) });
  const products = useQuery({ queryKey: ['products', 'dashboard'], queryFn: () => api.getProducts({ pageSize: 100 }) });
  const customers = useQuery({
    queryKey: ['customers', 'dashboard'],
    queryFn: () => api.getCustomers({ pageSize: 100 }),
    enabled: hasRole(user, ['Admin', 'Atendente']),
  });

  const orderItems = orders.data?.items ?? [];
  const revenue = orderItems
    .filter((order) => order.status !== 'Cancelled')
    .reduce((sum, order) => sum + order.total, 0);

  return (
    <section className="page-grid">
      <PageHeader
        eyebrow="Operacao"
        title="Visao geral"
        description="Indicadores rapidos para acompanhar cardapio, pedidos e atendimento."
      />

      <div className="metric-grid">
        <Metric icon={ClipboardList} label="Pedidos" value={orders.data?.totalItems ?? 0} tone="red" />
        <Metric icon={BadgeDollarSign} label="Receita em pedidos" value={money.format(revenue)} tone="yellow" />
        <Metric icon={Drumstick} label="Produtos ativos" value={products.data?.items.filter((item) => item.isActive).length ?? 0} />
        <Metric icon={Users} label="Clientes" value={customers.data?.totalItems ?? '-'} />
      </div>

      <div className="kanban">
        {(['Received', 'Preparing', 'OutForDelivery', 'Delivered'] satisfies OrderStatus[]).map((status) => (
          <article className="kanban-column" key={status}>
            <h3>{statusLabels[status]}</h3>
            {orderItems.filter((order) => order.status === status).slice(0, 4).map((order) => (
              <div className="mini-order" key={order.id}>
                <strong>{order.customerName}</strong>
                <span>{order.totalItems} itens - {money.format(order.total)}</span>
              </div>
            ))}
            {orderItems.filter((order) => order.status === status).length === 0 && <span className="muted">Sem pedidos</span>}
          </article>
        ))}
      </div>
    </section>
  );
}

function ProductsPage({ user }: { user: AuthUser }) {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [editing, setEditing] = useState<Product | null>(null);
  const isAdmin = hasRole(user, ['Admin']);
  const categories = useQuery({ queryKey: ['categories'], queryFn: api.getCategories });
  const products = useQuery({
    queryKey: ['products', search],
    queryFn: () => api.getProducts({ pageSize: 50, search }),
  });

  const mutation = useMutation({
    mutationFn: (payload: ProductFormData) =>
      editing ? api.updateProduct(editing.id, payload) : api.createProduct(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      queryClient.invalidateQueries({ queryKey: ['menu'] });
      setEditing(null);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: api.deleteProduct,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['products'] }),
  });

  return (
    <section className="page-grid">
      <PageHeader eyebrow="Cardapio" title="Produtos" description="Gerencie precos, disponibilidade e categorias do delivery." />
      <Toolbar value={search} onChange={setSearch} placeholder="Buscar produto" />

      <div className="split-layout">
        <div className="panel">
          <PanelTitle icon={Drumstick} title="Lista de produtos" />
          <DataState isLoading={products.isLoading} error={products.error} empty={!products.data?.items.length}>
            <div className="table-list">
              {products.data?.items.map((product) => (
                <article className="row-card" key={product.id}>
                  <div>
                    <strong>{product.name}</strong>
                    <span>{product.categoryName} - {money.format(product.price)}</span>
                  </div>
                  <StatusPill active={product.isActive} activeText="Ativo" inactiveText="Inativo" />
                  {isAdmin && (
                    <div className="row-actions">
                      <button type="button" className="ghost-button" onClick={() => setEditing(product)}>Editar</button>
                      <button type="button" className="danger-button" onClick={() => deleteMutation.mutate(product.id)}>Inativar</button>
                    </div>
                  )}
                </article>
              ))}
            </div>
          </DataState>
        </div>

        {isAdmin && (
          <ProductForm
            categories={categories.data ?? []}
            editing={editing}
            isSaving={mutation.isPending}
            error={mutation.error}
            onCancel={() => setEditing(null)}
            onSubmit={(values) => mutation.mutate(values)}
          />
        )}
      </div>
    </section>
  );
}

const productSchema = z.object({
  name: z.string().min(2, 'Informe o nome.'),
  description: z.string(),
  price: z.coerce.number().positive('Preco deve ser maior que zero.'),
  categoryId: z.string().min(1, 'Selecione a categoria.'),
  isActive: z.boolean(),
  imageUrl: z.string(),
});

type ProductFormInput = z.input<typeof productSchema>;
type ProductFormData = z.output<typeof productSchema>;

function ProductForm({
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
  onSubmit: (values: ProductFormData) => void;
}) {
  const { register, handleSubmit, reset, formState } = useForm<ProductFormInput, unknown, ProductFormData>({
    resolver: zodResolver(productSchema),
    defaultValues: {
      name: '',
      description: '',
      price: 1,
      categoryId: '',
      isActive: true,
      imageUrl: '',
    },
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
          name: '',
          description: '',
          price: 1,
          categoryId: categories[0]?.id ?? '',
          isActive: true,
          imageUrl: '',
        });
  }, [categories, editing, reset]);

  return (
    <form className="panel form-panel" onSubmit={handleSubmit(onSubmit)}>
      <PanelTitle icon={PackagePlus} title={editing ? 'Editar produto' : 'Novo produto'} />
      <Field label="Nome" error={formState.errors.name?.message}>
        <input {...register('name')} />
      </Field>
      <Field label="Descricao" error={formState.errors.description?.message}>
        <textarea rows={3} {...register('description')} />
      </Field>
      <div className="two-columns">
        <Field label="Preco" error={formState.errors.price?.message}>
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

function CategoriesPage() {
  const queryClient = useQueryClient();
  const [editing, setEditing] = useState<Category | null>(null);
  const categories = useQuery({ queryKey: ['categories'], queryFn: api.getCategories });
  const mutation = useMutation({
    mutationFn: (payload: CategoryFormData) => editing ? api.updateCategory(editing.id, payload) : api.createCategory(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      setEditing(null);
    },
  });
  const deleteMutation = useMutation({
    mutationFn: api.deleteCategory,
    onSuccess: (_result, categoryId) => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      if (editing?.id === categoryId) {
        setEditing(null);
      }
    },
  });

  function handleDelete(category: Category) {
    const confirmed = window.confirm(`Excluir a categoria "${category.name}"? Essa acao so sera permitida se ela nao estiver vinculada a produtos.`);
    if (confirmed) {
      deleteMutation.mutate(category.id);
    }
  }

  return (
    <section className="page-grid">
      <PageHeader eyebrow="Administracao" title="Categorias" description="Organize o cardapio para facilitar compra e operacao." />
      <div className="split-layout">
        <div className="panel">
          <PanelTitle icon={Tags} title="Categorias cadastradas" />
          {deleteMutation.error ? <p className="form-error">{errorMessage(deleteMutation.error)}</p> : null}
          <DataState isLoading={categories.isLoading} error={categories.error} empty={!categories.data?.length}>
            <div className="table-list">
              {categories.data?.map((category) => (
                <article className="row-card" key={category.id}>
                  <div>
                    <strong>{category.name}</strong>
                    <span>{category.description || 'Sem descricao'}</span>
                  </div>
                  <StatusPill active={category.isActive} activeText="Ativa" inactiveText="Inativa" />
                  <div className="row-actions">
                    <button type="button" className="ghost-button" onClick={() => setEditing(category)}>Editar</button>
                    <button
                      type="button"
                      className="danger-button"
                      disabled={deleteMutation.isPending}
                      onClick={() => handleDelete(category)}
                    >
                      Excluir
                    </button>
                  </div>
                </article>
              ))}
            </div>
          </DataState>
        </div>
        <CategoryForm editing={editing} isSaving={mutation.isPending} error={mutation.error} onCancel={() => setEditing(null)} onSubmit={(values) => mutation.mutate(values)} />
      </div>
    </section>
  );
}

const categorySchema = z.object({
  name: z.string().min(2, 'Informe o nome.'),
  description: z.string(),
  isActive: z.boolean(),
});

type CategoryFormInput = z.input<typeof categorySchema>;
type CategoryFormData = z.output<typeof categorySchema>;

function CategoryForm({
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
  onSubmit: (values: CategoryFormData) => void;
}) {
  const { register, handleSubmit, reset, formState } = useForm<CategoryFormInput, unknown, CategoryFormData>({
    resolver: zodResolver(categorySchema),
    defaultValues: { name: '', description: '', isActive: true },
  });

  useEffect(() => {
    reset(editing ? {
      name: editing.name,
      description: editing.description,
      isActive: editing.isActive,
    } : { name: '', description: '', isActive: true });
  }, [editing, reset]);

  return (
    <form className="panel form-panel" onSubmit={handleSubmit(onSubmit)}>
      <PanelTitle icon={Tags} title={editing ? 'Editar categoria' : 'Nova categoria'} />
      <Field label="Nome" error={formState.errors.name?.message}>
        <input {...register('name')} />
      </Field>
      <Field label="Descricao">
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

function CustomersPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [editing, setEditing] = useState<Customer | null>(null);
  const customers = useQuery({
    queryKey: ['customers', search],
    queryFn: () => api.getCustomers({ pageSize: 50, search }),
  });
  const mutation = useMutation({
    mutationFn: (payload: CustomerFormData) => editing ? api.updateCustomer(editing.id, payload) : api.createCustomer(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['customers'] });
      setEditing(null);
    },
  });

  return (
    <section className="page-grid">
      <PageHeader eyebrow="Atendimento" title="Clientes" description="Cadastre contatos e enderecos para acelerar novos pedidos." />
      <Toolbar value={search} onChange={setSearch} placeholder="Buscar cliente" />
      <div className="split-layout">
        <div className="panel">
          <PanelTitle icon={Users} title="Base de clientes" />
          <DataState isLoading={customers.isLoading} error={customers.error} empty={!customers.data?.items.length}>
            <div className="table-list">
              {customers.data?.items.map((customer) => (
                <article className="row-card" key={customer.id}>
                  <div>
                    <strong>{customer.name}</strong>
                    <span>{customer.phone} - {customer.address.neighborhood}</span>
                  </div>
                  <button type="button" className="ghost-button" onClick={() => setEditing(customer)}>Editar</button>
                </article>
              ))}
            </div>
          </DataState>
        </div>
        <CustomerForm editing={editing} isSaving={mutation.isPending} error={mutation.error} onCancel={() => setEditing(null)} onSubmit={(values) => mutation.mutate(values)} />
      </div>
    </section>
  );
}

const customerSchema = z.object({
  name: z.string().min(2, 'Informe o nome.'),
  phone: z.string().min(8, 'Informe o telefone.'),
  address: z.object({
    street: z.string().min(2, 'Informe a rua.'),
    number: z.string().min(1, 'Informe o numero.'),
    neighborhood: z.string().min(2, 'Informe o bairro.'),
    city: z.string().min(2, 'Informe a cidade.'),
    state: z.string().min(2, 'UF').max(2, 'UF'),
    zipCode: z.string().min(8, 'Informe o CEP.'),
    complement: z.string(),
  }),
});

type CustomerFormInput = z.input<typeof customerSchema>;
type CustomerFormData = z.output<typeof customerSchema>;

function CustomerForm({
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
  onSubmit: (values: CustomerFormData) => void;
}) {
  const defaults: CustomerFormData = {
    name: '',
    phone: '',
    address: {
      street: '',
      number: '',
      neighborhood: '',
      city: 'Sao Paulo',
      state: 'SP',
      zipCode: '',
      complement: '',
    },
  };
  const { register, handleSubmit, reset, formState } = useForm<CustomerFormInput, unknown, CustomerFormData>({
    resolver: zodResolver(customerSchema),
    defaultValues: defaults,
  });

  useEffect(() => {
    reset(editing ? {
      name: editing.name,
      phone: editing.phone,
      address: editing.address,
    } : defaults);
  }, [editing, reset]);

  return (
    <form className="panel form-panel" onSubmit={handleSubmit(onSubmit)}>
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
        <Field label="Numero" error={formState.errors.address?.number?.message}>
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

function OrdersPage({ user }: { user: AuthUser }) {
  const queryClient = useQueryClient();
  const [customerId, setCustomerId] = useState('');
  const [productId, setProductId] = useState('');
  const [quantity, setQuantity] = useState(1);
  const [cart, setCart] = useState<Array<{ productId: string; quantity: number }>>([]);
  const orders = useQuery({ queryKey: ['orders'], queryFn: () => api.getOrders({ pageSize: 100 }) });
  const customers = useQuery({
    queryKey: ['customers', 'orders'],
    queryFn: () => api.getCustomers({ pageSize: 100 }),
    enabled: hasRole(user, ['Admin', 'Atendente']),
  });
  const products = useQuery({ queryKey: ['products', 'active'], queryFn: () => api.getProducts({ pageSize: 100, active: true }) });

  useEffect(() => {
    if (!customerId && customers.data?.items[0]) {
      setCustomerId(customers.data.items[0].id);
    }
    if (!productId && products.data?.items[0]) {
      setProductId(products.data.items[0].id);
    }
  }, [customerId, customers.data?.items, productId, products.data?.items]);

  const createOrder = useMutation({
    mutationFn: () => api.createOrder({ customerId, items: cart }),
    onSuccess: () => {
      setCart([]);
      queryClient.invalidateQueries({ queryKey: ['orders'] });
    },
  });
  const statusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: OrderStatus }) => api.updateOrderStatus(id, status),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['orders'] }),
  });

  const productMap = useMemo(
    () => new Map((products.data?.items ?? []).map((product) => [product.id, product])),
    [products.data?.items],
  );

  function addCartItem() {
    if (!productId || quantity <= 0) {
      return;
    }

    setCart((current) => {
      const existing = current.find((item) => item.productId === productId);
      if (existing) {
        return current.map((item) => item.productId === productId ? { ...item, quantity: item.quantity + quantity } : item);
      }

      return [...current, { productId, quantity }];
    });
    setQuantity(1);
  }

  return (
    <section className="page-grid">
      <PageHeader eyebrow="Pedidos" title="Fluxo de entrega" description="Crie pedidos e avance status conforme responsabilidade de cada perfil." />
      <div className="split-layout">
        <div className="panel">
          <PanelTitle icon={ClipboardList} title="Pedidos recentes" />
          <DataState isLoading={orders.isLoading} error={orders.error} empty={!orders.data?.items.length}>
            <div className="order-list">
              {orders.data?.items.map((order) => (
                <OrderCard
                  key={order.id}
                  order={order}
                  user={user}
                  isSaving={statusMutation.isPending}
                  onStatus={(status) => statusMutation.mutate({ id: order.id, status })}
                />
              ))}
            </div>
          </DataState>
        </div>

        {hasRole(user, ['Admin', 'Atendente']) && (
          <aside className="panel form-panel">
            <PanelTitle icon={PackagePlus} title="Novo pedido" />
            <Field label="Cliente">
              <select value={customerId} onChange={(event) => setCustomerId(event.target.value)}>
                {customers.data?.items.map((customer) => (
                  <option key={customer.id} value={customer.id}>{customer.name}</option>
                ))}
              </select>
            </Field>
            <div className="two-columns">
              <Field label="Produto">
                <select value={productId} onChange={(event) => setProductId(event.target.value)}>
                  {products.data?.items.map((product) => (
                    <option key={product.id} value={product.id}>{product.name}</option>
                  ))}
                </select>
              </Field>
              <Field label="Qtd.">
                <input type="number" min={1} value={quantity} onChange={(event) => setQuantity(Number(event.target.value))} />
              </Field>
            </div>
            <button className="ghost-button" type="button" onClick={addCartItem}>Adicionar item</button>
            <div className="cart-list">
              {cart.map((item) => {
                const product = productMap.get(item.productId);
                return (
                  <div className="cart-row" key={item.productId}>
                    <span>{product?.name ?? item.productId}</span>
                    <strong>{item.quantity}x</strong>
                  </div>
                );
              })}
              {cart.length === 0 && <span className="muted">Nenhum item adicionado</span>}
            </div>
            {createOrder.error && <p className="form-error">{errorMessage(createOrder.error)}</p>}
            <button className="primary-button" type="button" disabled={!customerId || cart.length === 0 || createOrder.isPending} onClick={() => createOrder.mutate()}>
              Criar pedido
            </button>
          </aside>
        )}
      </div>
    </section>
  );
}

function OrderCard({
  order,
  user,
  isSaving,
  onStatus,
}: {
  order: Order;
  user: AuthUser;
  isSaving: boolean;
  onStatus: (status: OrderStatus) => void;
}) {
  const actions = getStatusActions(order, user);
  return (
    <article className="order-card">
      <div className="order-card__head">
        <div>
          <strong>{order.customerName}</strong>
          <span>{order.totalItems} itens - {money.format(order.total)}</span>
        </div>
        <span className={`status-badge status-${order.status}`}>{statusLabels[order.status]}</span>
      </div>
      <ul>
        {order.items.map((item) => (
          <li key={item.id}>{item.quantity}x {item.productName}</li>
        ))}
      </ul>
      {actions.length > 0 && (
        <div className="row-actions">
          {actions.map((status) => (
            <button key={status} type="button" className="ghost-button" disabled={isSaving} onClick={() => onStatus(status)}>
              {statusLabels[status]}
            </button>
          ))}
        </div>
      )}
    </article>
  );
}

function getStatusActions(order: Order, user: AuthUser): OrderStatus[] {
  if (order.status === 'Delivered' || order.status === 'Cancelled') {
    return [];
  }

  if (user.roles.includes('Admin')) {
    return nextStatuses(order.status);
  }

  if (user.roles.includes('Atendente')) {
    return ['Cancelled'];
  }

  if (user.roles.includes('Cozinha') && order.status === 'Received') {
    return ['Preparing'];
  }

  if (user.roles.includes('Entregador') && order.status === 'Preparing') {
    return ['OutForDelivery'];
  }

  if (user.roles.includes('Entregador') && order.status === 'OutForDelivery') {
    return ['Delivered'];
  }

  return [];
}

function nextStatuses(status: OrderStatus): OrderStatus[] {
  switch (status) {
    case 'Received':
      return ['Preparing', 'Cancelled'];
    case 'Preparing':
      return ['OutForDelivery', 'Cancelled'];
    case 'OutForDelivery':
      return ['Delivered', 'Cancelled'];
    default:
      return [];
  }
}

function MenuPage() {
  const menu = useQuery({ queryKey: ['menu'], queryFn: api.getMenu });

  return (
    <section className="page-grid">
      <PageHeader eyebrow="Cardapio publico" title="Menu ativo" description="Visualizacao do que esta disponivel para o cliente final." />
      <DataState isLoading={menu.isLoading} error={menu.error} empty={!menu.data?.length}>
        <div className="menu-grid">
          {menu.data?.map((category) => (
            <article className="menu-category" key={category.id}>
              <h3>{category.name}</h3>
              <p>{category.description}</p>
              <div className="menu-products">
                {category.products.map((product) => (
                  <div className="menu-product" key={product.id}>
                    <div className="product-thumb">
                      <ChefHat size={24} />
                    </div>
                    <div>
                      <strong>{product.name}</strong>
                      <span>{product.description}</span>
                    </div>
                    <b>{money.format(product.price)}</b>
                  </div>
                ))}
              </div>
            </article>
          ))}
        </div>
      </DataState>
    </section>
  );
}

function PageHeader({ eyebrow, title, description }: { eyebrow: string; title: string; description: string }) {
  return (
    <header className="page-header">
      <span className="eyebrow">{eyebrow}</span>
      <h1>{title}</h1>
      <p>{description}</p>
    </header>
  );
}

function Metric({ icon: Icon, label, value, tone }: { icon: LucideIcon; label: string; value: string | number; tone?: 'red' | 'yellow' }) {
  return (
    <article className={`metric metric--${tone ?? 'neutral'}`}>
      <Icon size={22} />
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  );
}

function PanelTitle({ icon: Icon, title }: { icon: LucideIcon; title: string }) {
  return (
    <div className="panel-title">
      <Icon size={19} />
      <h2>{title}</h2>
    </div>
  );
}

function Field({ label, error, children }: { label: string; error?: string; children: ReactNode }) {
  return (
    <label className="field">
      <span>{label}</span>
      {children}
      {error && <small>{error}</small>}
    </label>
  );
}

function Toolbar({ value, onChange, placeholder }: { value: string; onChange: (value: string) => void; placeholder: string }) {
  return (
    <div className="toolbar">
      <Search size={18} />
      <input value={value} onChange={(event) => onChange(event.target.value)} placeholder={placeholder} />
    </div>
  );
}

function StatusPill({ active, activeText, inactiveText }: { active: boolean; activeText: string; inactiveText: string }) {
  return <span className={`pill ${active ? 'pill--on' : 'pill--off'}`}>{active ? activeText : inactiveText}</span>;
}

function DataState({
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
