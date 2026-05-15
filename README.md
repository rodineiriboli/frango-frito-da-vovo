# Frango Frito da Vovo

CRUD full stack para um painel administrativo de delivery de frango frito, criado como projeto de teste para vaga .NET senior.

O foco do projeto nao e ter um escopo enorme, mas demonstrar uma entrega pequena com decisoes de producao: arquitetura em camadas, regras de dominio, autenticacao, autorizacao por perfis, PostgreSQL, Docker, frontend responsivo e testes.

## Stack

Backend:

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- ASP.NET Core Identity
- Cookie HttpOnly para autenticacao
- Autorizacao por roles/policies
- OpenAPI em ambiente de desenvolvimento
- xUnit com testes unitarios e de integracao

Frontend:

- React
- TypeScript
- Vite
- React Router
- TanStack Query
- React Hook Form
- Zod
- lucide-react
- Layout responsivo mobile/desktop com largura maxima de 1440px

Infra:

- Docker
- Docker Compose
- PostgreSQL containerizado

## Como rodar com Docker

Na raiz do projeto:

```bash
docker compose up --build
```

Observacao: o Dockerfile da API usa `mcr.microsoft.com/dotnet/sdk:10.0.200` de forma explicita porque a tag flutuante `sdk:10.0`, no momento do desenvolvimento, baixou o SDK `10.0.300` e falhou no Docker Desktop com erro de leitura do `dotnet.runtimeconfig.json`. O runtime final continua usando `mcr.microsoft.com/dotnet/aspnet:10.0`.

URLs:

- Frontend: http://localhost:3000
- API: http://localhost:8080
- Health check: http://localhost:8080/health
- OpenAPI JSON: http://localhost:8080/openapi/v1.json

O banco PostgreSQL sobe com:

```text
Host: localhost
Porta: 5432
Database: frango_frito_da_vovo
Usuario: postgres
Senha: postgres
```

## Usuarios seed

Todos usam a senha:

```text
Vovo@12345
```

Perfis:

```text
admin@frangofrito.local       Admin
atendente@frangofrito.local   Atendente
cozinha@frangofrito.local     Cozinha
entregador@frangofrito.local  Entregador
```

## Como rodar localmente

Suba apenas o PostgreSQL:

```bash
docker compose up postgres
```

Backend:

```bash
cd backend
dotnet restore
dotnet run --project src/FrangoFrito.Api
```

Frontend:

```bash
cd frontend
npm install
npm run dev
```

URLs locais:

- Frontend Vite: http://localhost:5173
- API local: http://localhost:5080

## Testes e qualidade

Backend:

```bash
cd backend
dotnet test
```

Frontend:

```bash
cd frontend
npm test
npm run build
```

## Escopo funcional

- Login/logout
- Usuario autenticado atual
- Roles: Admin, Atendente, Cozinha, Entregador
- CRUD de categorias
- CRUD de produtos
- CRUD de clientes
- Criacao de pedidos
- Itens de pedido
- Fluxo de status do pedido
- Cardapio publico de produtos ativos
- Filtros, busca e paginacao nos principais endpoints
- Seed inicial de usuarios, categorias, produtos e clientes

## Autenticacao e autorizacao

A autenticacao usa ASP.NET Core Identity com cookie HttpOnly. A escolha evita expor token em `localStorage` e deixa a seguranca principal no backend.

O frontend apenas adapta a navegacao conforme as roles do usuario. A autorizacao real fica na API por roles e policies.

Perfis:

- Admin: gerencia categorias, produtos, clientes, pedidos e usuarios.
- Atendente: cadastra clientes e cria/cancela pedidos.
- Cozinha: acompanha pedidos recebidos e inicia preparo.
- Entregador: acompanha entregas e conclui pedidos.

## Arquitetura

```text
backend/
  src/
    FrangoFrito.Api
    FrangoFrito.Application
    FrangoFrito.Domain
    FrangoFrito.Infrastructure
  tests/
    FrangoFrito.UnitTests
    FrangoFrito.IntegrationTests

frontend/
  src/
    shared/
    App.tsx
    styles.css
```

Principios usados:

- Regras de negocio no dominio.
- EF Core isolado na infraestrutura.
- DTOs separados das entidades.
- Identity isolado da API.
- Erros de dominio convertidos para `ProblemDetails`.
- Controle de concorrencia por token de versao nas entidades.
- Migrations versionadas.
- Frontend com estados de loading, erro e vazio.

## Decisoes visuais

O frontend usa uma identidade inspirada em delivery, com vermelho e amarelo como cores principais, sem uso de marca, logo ou assets proprietarios de terceiros.

O layout foi pensado para:

- celular;
- tablet;
- navegador desktop;
- largura maxima de conteudo em 1440px.

## Proximos passos possiveis

- Refresh de sessao mais refinado.
- Auditoria por usuario.
- Upload real de imagens de produto.
- Testes end-to-end com Playwright.
- Pipeline de deploy.
