# Frango Frito da Vovó

CRUD full stack para um painel administrativo de delivery de frango frito, criado como projeto de teste para vaga de desenvolvedor .NET sênior.

O objetivo não é entregar um escopo enorme, mas demonstrar uma aplicação pequena tratada com padrão de produção: Clean Architecture, SOLID, autenticação, autorização por perfis, regras de domínio, PostgreSQL, Docker, frontend responsivo e testes automatizados.

## Stack

Backend:

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- ASP.NET Core Identity
- Cookie HttpOnly para autenticação
- Autorização por roles e policies
- OpenAPI em ambiente de desenvolvimento
- xUnit para testes unitários e de integração

Frontend:

- React
- TypeScript
- Vite
- React Router
- TanStack Query
- React Hook Form
- Zod
- lucide-react
- Layout responsivo para celular e desktop
- Largura máxima de 1440px em navegadores desktop

Infraestrutura:

- Docker
- Docker Compose
- PostgreSQL containerizado
- Health check da API

## Como Rodar Com Docker

Pré-requisitos:

- Docker Desktop instalado
- Docker Compose disponível no terminal

Na raiz do projeto:

```powershell
docker compose up --build
```

Ou, para rodar em segundo plano:

```powershell
docker compose up -d --build
```

URLs principais:

- Frontend: http://localhost:3000
- API: http://localhost:8080
- Health check: http://localhost:8080/health
- OpenAPI: http://localhost:8080/openapi/v1.json

Banco de dados:

- Host: `localhost`
- Porta: `5432`
- Database: `frango_frito_da_vovo`
- Usuário: `postgres`
- Senha: `postgres`

Observação sobre .NET 10 no Docker:

O Dockerfile da API fixa a imagem `mcr.microsoft.com/dotnet/sdk:10.0.200`, pois a tag genérica `sdk:10.0` pode resolver para uma versão de SDK que apresentou falha de runtime no ambiente local durante o `dotnet restore`.

## Usuários Seed

A senha inicial para todos os usuários seed é:

```text
Vovo@12345
```

Usuários criados automaticamente:

| Perfil | E-mail |
| --- | --- |
| Admin | `admin@frangofrito.local` |
| Atendente | `atendente@frangofrito.local` |
| Cozinha | `cozinha@frangofrito.local` |
| Entregador | `entregador@frangofrito.local` |

Caso o volume do PostgreSQL já exista, alterações recentes nos dados seed podem não aparecer automaticamente. Para recriar tudo do zero, remova o volume do banco antes de subir novamente os containers.

## Como Rodar Localmente

Suba apenas o PostgreSQL:

```powershell
docker compose up postgres
```

Backend:

```powershell
dotnet restore backend/src/FrangoFrito.Api/FrangoFrito.Api.csproj
dotnet build backend/src/FrangoFrito.Api/FrangoFrito.Api.csproj
dotnet run --project backend/src/FrangoFrito.Api/FrangoFrito.Api.csproj
```

Frontend:

```powershell
cd frontend
npm install
npm run dev
```

URLs locais:

- Frontend Vite: http://localhost:5173
- API local: http://localhost:5080

## Testes E Qualidade

Backend:

```powershell
dotnet test backend/tests/FrangoFrito.UnitTests/FrangoFrito.UnitTests.csproj
dotnet test backend/tests/FrangoFrito.IntegrationTests/FrangoFrito.IntegrationTests.csproj
```

Frontend:

```powershell
cd frontend
npm test
npm run build
```

Validações executadas durante a implementação:

- Build da API com sucesso
- Testes unitários do backend com sucesso
- Testes de integração do backend com sucesso
- Build do frontend com sucesso
- Testes do frontend com sucesso
- Build da API via Docker Compose com sucesso
- Health check da API retornando `Healthy`

## Escopo Funcional

Funcionalidades implementadas:

- Login com cookie HttpOnly
- Logout com redirecionamento para a tela de login
- Consulta do usuário autenticado
- Controle de acesso por perfil
- Dashboard administrativo
- CRUD de categorias
- Exclusão de categoria somente quando ela não está em uso
- CRUD de produtos
- Desativação de produto em vez de exclusão física
- CRUD de clientes
- Criação e consulta de pedidos
- Fluxo de alteração de status do pedido
- Consulta de cardápio público
- Tratamento de erros com mensagens apresentáveis no frontend
- Layout responsivo para dispositivos móveis e desktop

## Autenticação E Autorização

A autenticação é implementada com ASP.NET Core Identity e cookie HttpOnly.

Decisões principais:

- O frontend não manipula token JWT diretamente.
- A sessão fica protegida em cookie HttpOnly.
- A API expõe endpoints para login, logout e usuário atual.
- O backend valida autenticação e autorização em todas as rotas protegidas.
- O frontend adapta navegação e ações conforme as roles do usuário.

Perfis atuais:

- `Admin`
- `Atendente`
- `Cozinha`
- `Entregador`

## Clean Architecture

O backend segue Clean Architecture com separação explícita entre domínio, aplicação, infraestrutura e API.

Direção das dependências:

```text
FrangoFrito.Api -> FrangoFrito.Application -> FrangoFrito.Domain
FrangoFrito.Infrastructure -> FrangoFrito.Application
FrangoFrito.Infrastructure -> FrangoFrito.Domain
```

Regras importantes:

- `Domain` não depende de nenhum outro projeto.
- `Application` depende apenas de `Domain`.
- `Infrastructure` implementa contratos definidos em `Application`.
- `Api` consome casos de uso e abstrações da camada de aplicação.
- Controllers não recebem `DbContext`.
- Controllers não dependem de Entity Framework Core.
- Controllers não dependem diretamente de `UserManager`, `SignInManager` ou entidades de domínio.

Estrutura do backend:

```text
backend/
  src/
    FrangoFrito.Api/
      Controllers/
      Extensions/
      Middleware/
      Program.cs
    FrangoFrito.Application/
      Auth/
      Categories/
      Common/
      Customers/
      Menu/
      Orders/
      Products/
      Security/
    FrangoFrito.Domain/
      Common/
      Entities/
      Enums/
      ValueObjects/
    FrangoFrito.Infrastructure/
      Identity/
      Persistence/
        Migrations/
        Repositories/
      Security/
      Services/
      DependencyInjection.cs
  tests/
    FrangoFrito.UnitTests/
    FrangoFrito.IntegrationTests/
```

## Application Layer

A camada de aplicação concentra os casos de uso e define os contratos que a infraestrutura deve implementar.

Responsabilidades:

- Orquestrar regras de aplicação
- Validar fluxos antes de persistir alterações
- Expor DTOs de entrada e saída
- Definir contratos de repositório
- Definir `IUnitOfWork`
- Definir serviços de autenticação e usuário atual
- Retornar resultados padronizados com `ApplicationResult`

Exemplos de abstrações:

- `ICategoryRepository`
- `IProductRepository`
- `ICustomerRepository`
- `IOrderRepository`
- `IMenuRepository`
- `IUnitOfWork`
- `IAuthService`
- `IUserService`
- `ICurrentUser`

Essa camada não conhece Entity Framework Core, ASP.NET Core, PostgreSQL ou Identity.

## Repository Pattern

O acesso a dados é feito por repositórios específicos por contexto de negócio.

Objetivos:

- Evitar vazamento de detalhes do EF Core para a API
- Facilitar testes unitários de casos de uso
- Manter a camada de aplicação dependente de abstrações
- Centralizar consultas necessárias para cada agregado
- Reduzir acoplamento entre controllers, persistência e regras de negócio

A persistência é concluída por meio de `IUnitOfWork`, mantendo o controle transacional fora dos controllers.

## Domain Layer

A camada de domínio concentra entidades, enums, regras de negócio e erros de domínio.

Exemplos de regras tratadas no domínio:

- Produto não pode ser criado com preço inválido
- Pedido controla seu próprio fluxo de status
- Categoria mantém consistência de nome e descrição
- Entidades usam identificadores fortes e encapsulam estado

## Infrastructure Layer

A infraestrutura implementa contratos da aplicação.

Responsabilidades:

- `FrangoFritoDbContext`
- Mapeamentos do EF Core
- Repositórios concretos
- `UnitOfWork`
- Migrations
- Seed de dados
- Integração com ASP.NET Core Identity
- Implementações de autenticação e usuário atual

## API Layer

A API é responsável por HTTP, autenticação, autorização, serialização e adaptação de resultados.

Responsabilidades:

- Controllers enxutos
- Mapeamento de `ApplicationResult` para respostas HTTP
- `ProblemDetails` para erros
- Registro de dependências
- Configuração de CORS
- Configuração de cookies
- Health check
- OpenAPI em desenvolvimento

## Frontend

O frontend foi organizado por domínio funcional, evitando concentrar a aplicação inteira em um único `App.tsx`.

Estrutura principal:

```text
frontend/
  src/
    app/
      layout/
      App.tsx
      AppRoutes.tsx
    features/
      auth/
      categories/
      customers/
      dashboard/
      menu/
      orders/
      products/
    shared/
      api/
      components/
      constants/
      utils/
    App.tsx
    main.tsx
    styles.css
```

Decisões principais:

- `app` concentra composição geral, providers, rotas e layout.
- `features` concentra telas, componentes e hooks por domínio.
- `shared` concentra componentes reutilizáveis, cliente HTTP, constantes e utilitários.
- `App.tsx` da raiz apenas delega para a aplicação real.
- TanStack Query centraliza cache e estado assíncrono.
- React Hook Form e Zod tratam formulários e validações.
- A interface usa vermelho e amarelo como identidade visual inspirada no iFood.
- O layout foi pensado para celular e navegador desktop, com largura máxima de 1440px.

## Decisões De Produção

Principais cuidados aplicados:

- SOLID como princípio obrigatório
- Controllers sem dependência direta de `DbContext`
- Dependência de abstrações em vez de classes concretas
- Separação clara de responsabilidades
- Casos de uso testáveis sem banco real
- Regras de domínio fora dos controllers
- EF Core isolado na infraestrutura
- Identity isolado atrás de serviços de aplicação
- DTOs separados das entidades de domínio
- Erros de aplicação convertidos para HTTP de forma padronizada
- Mensagens em português com acentuação revisada
- Controle de concorrência em entidades relevantes
- Exclusão protegida para categorias em uso
- Desativação lógica para produtos
- Docker Compose para ambiente reprodutível

## Próximos Passos Possíveis

Melhorias recomendadas caso o projeto evolua:

- Adicionar paginação e filtros avançados nas listagens
- Adicionar FluentValidation na camada de aplicação
- Adicionar logs estruturados com Serilog
- Adicionar observabilidade com OpenTelemetry
- Adicionar testes end-to-end com Playwright
- Adicionar pipeline de CI com build, testes e análise estática
- Adicionar rate limiting nos endpoints de autenticação
- Adicionar refresh de sessão por política explícita
- Adicionar tela de gestão de usuários e perfis
- Adicionar auditoria de alterações relevantes
