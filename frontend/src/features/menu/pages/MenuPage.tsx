import { useQuery } from '@tanstack/react-query';
import { ChefHat } from 'lucide-react';
import { api } from '../../../shared/api';
import { DataState, PageHeader } from '../../../shared/components';
import { money } from '../../../shared/utils/formatters';

export function MenuPage() {
  const menu = useQuery({ queryKey: ['menu'], queryFn: api.getMenu });

  return (
    <section className="page-grid">
      <PageHeader eyebrow="Cardápio público" title="Menu ativo" description="Visualização do que está disponível para o cliente final." />
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
