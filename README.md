# ONG Vestindo Sonhos - Sistema Integrado de Gestão & Aplicativo Móvel

Projeto Integrador Multidisciplinar (PIM) - Análise e Desenvolvimento de Sistemas.

Solução corporativa completa para a gestão de beneficiários, doações financeiras, inventário de vestuário, voluntariado e eventos da ONG **Vestindo Sonhos**, integrando plataforma web administrativa (ASP.NET Core MVC + Bootstrap 5 + Chart.js), API RESTful segura (JWT), aplicativo móvel (Flutter), modo escuro (*Dark Mode*) global e painel administrativo SaaS.

---

## 🏛️ Arquitetura da Solução

```
                    ┌────────────────────────┐
                    │    MySQL Server 8.0    │
                    │  (Triggers, SPs, Views) │
                    └───────────┬────────────┘
                                │
                    ┌───────────┴────────────┐
                    │ ASP.NET Core (.NET 10) │
                    │ Web MVC + RESTful API  │
                    └───────┬────────────┬───┘
                            │            │
            (Cookie Auth)   │            │   (JWT Bearer Token)
                            ▼            ▼
               ┌────────────────┐   ┌────────────────────────┐
               │ Portal Web MVC │   │   App Mobile Flutter   │
               │ (Dashboard +   │   │ (Dashboard, Eventos,   │
               │  SaaS Admin)   │   │  Estoque & Dark Mode)  │
               └────────────────┘   └────────────────────────┘
```

---

## 🚀 Módulos e Funcionalidades

### 1. Dashboard Executivo Principal & Portal Web (ASP.NET Core MVC + Chart.js)
- **Dashboard como Página Principal (`/Financeiro/Dashboard`):**
  - **4 KPIs Executivos:** Total de Beneficiários (+ Dependentes), Voluntários Ativos (+ Eventos), Saldo em Caixa (+ Doações do Mês) e Itens no Inventário (+ Unidades em Estoque).
  - **Gráficos Interativos Dinâmicos (Chart.js 4.4):**
    - Evolução Financeira (Entradas vs Saídas).
    - Distribuição do Inventário por Categoria (Gráfico de Rosca/Doughnut).
    - Beneficiários por Situação Cadastral.
    - Demandas Prioritárias da Comunidade (baseadas exclusivamente em registros reais do banco de dados).
  - **Tabelas de Acesso Rápido:** Últimas Transações Financeiras e Próximos Eventos Agendados.
  - **Sincronização AJAX em Tempo Real:** Endpoint `/Financeiro/DadosDashboardJson` sem necessidade de scripts externos em Python.
- **Modo Escuro Global (*Dark Mode*):**
  - Alternância instantânea entre **Modo Claro** e **Modo Escuro** em todas as telas do sistema, com persistência no navegador (`localStorage`) e adaptação automática dos gráficos Chart.js e componentes Bootstrap 5 (`data-bs-theme`).
- **Plataforma de Configurações SaaS (Exclusiva para Administradores):**
  - Protegida por regra de negócio `[Authorize(Roles = "Administrador")]`.
  - **5 Abas SaaS:** Organização & Metas Mensais, Aparência & Interface, Cadastros Base (Carteiras, Categorias, Tipos de Ativo), Segurança & Governança e Sistema/API + Exportação de Backup JSON completo.

### 2. Back-end & API RESTful (.NET 10)
- **Autenticação Híbrida Inteligente:** Suporte simultâneo a Cookies (para o portal administrativo MVC) e tokens JWT Bearer (`/api`) para o aplicativo mobile.
- **Criptografia e Hashing Seguro:** Senhas protegidas com PBKDF2 (`PasswordHasher<Usuario>`) e fluxo obrigatório de troca de senha temporária no primeiro acesso.
- **Endpoints REST Principais:**
  - `POST /api/auth/login` e `POST /api/auth/trocar-senha-obrigatoria`
  - `GET /api/dashboard`: Indicadores consolidados, distribuição de estoque, últimas transações e próximos eventos.
  - `GET /api/dashboard/configuracoes` & `POST /api/dashboard/configuracoes`: Gestão remota das configurações SaaS (restrito a `Administrador`).
  - `GET/POST/PUT/DELETE /api/beneficiarios`, `/api/inventario`, `/api/financeiro`, `/api/eventos`.

### 3. Aplicativo Móvel (Flutter / Dart)
- **Sincronizado com as regras Web:**
  - **Dashboard Executivo Mobile** como tela principal pós-login (4 KPIs, Fluxo Financeiro + Meta Mensal SaaS, Distribuição de Estoque por Categoria, Demandas Reais da Comunidade, Últimas Transações e Próximos Eventos).
  - **Modo Escuro (*Dark Mode*) Global:** Alternância em tempo real na AppBar e no Menu Lateral, persistida via `SharedPreferences`.
  - **Configurações SaaS Mobile (`ConfiguracoesSaasScreen`):** Visível e acessível exclusivamente para usuários com perfil `Administrador`.
  - **Configurador Dinâmico de IP/Servidor (`ServerConfigDialog`):** Permite alternar entre Wi-Fi local, Cabo USB (`localhost`) e Emulador Android (`10.0.2.2`) sem recompilar o app.

### 4. Banco de Dados & Recursos Avançados (MySQL 8.0)
- **Script Oficial:** [`ScriptVestindoSonhos_MySQL.sql`](./ScriptVestindoSonhos_MySQL.sql) contendo tabelas, *Stored Procedure* (`sp_RelatorioFinanceiroResumido`), *Trigger* (`trg_AtualizaEstoque`) e *Views* analíticas.

### 5. Testes Automatizados (.NET 10 / xUnit + EF Core InMemory)
- **Suíte Completa (`VestindoSonhos.Tests`):** 19 testes automatizados (`[Fact]` e `[Theory]`) cobrindo:
  - **Voluntários (`VoluntariosApiTests`):** Cadastro por Colaborador, higienização de CPF/Telefone, ordenação alfabética, edição e exclusão em cascata.
  - **Beneficiários (`BeneficiariosApiTests`):** Cadastro social com situação padrão `"Ok"`, validação de campos obrigatórios e contagem/exclusão de dependentes.
  - **Financeiro (`FinanceiroApiTests`):** Lançamento de Entradas e Saídas atualizando o saldo das carteiras, rejeição de valores zero/negativos e estorno automático ao excluir transação.
  - **Segurança RBAC & Dashboard (`DashboardAndRegrasNegocioTests`):** Ocultação de saldos e transações financeiras para perfil `Colaborador`, visualização total para `Administrador` e criptografia PBKDF2 (`PasswordHasher<Usuario>`).
- **Execução com 1 Clique:** Execute [`rodar-testes-automatizados.bat`](./rodar-testes-automatizados.bat) ou `dotnet test ./VestindoSonhos.Tests/VestindoSonhos.Tests.csproj`.

---

## 💻 Formas de Execução

### Opção 1: Inicialização Rápida com 1 Clique (Windows)
1. Execute [`importar-banco-local.bat`](./importar-banco-local.bat) para garantir que o banco `VESTINDOSONHOSDB` esteja criado no MySQL local.
2. Dê dois cliques em [`1-INICIAR_SISTEMA.bat`](./1-INICIAR_SISTEMA.bat) (ou em `VestindoSonhos_Portavel\1-CLIQUE_INICIAR_SISTEMA.bat`).
3. Caso deseje regerar a pasta portátil, execute [`gerar-executavel-portatil.bat`](./gerar-executavel-portatil.bat).
4. Acesse no navegador: [http://localhost:5000/Login](http://localhost:5000/Login)

### Credenciais Administrativas Padrão
- **E-mail:** `admin@teste.org`
- **Senha:** `123456`

### 📱 Executar o Aplicativo Mobile (Flutter)
```bash
cd vestindo_sonhos_mobile
flutter pub get
flutter run
```

---

## 📁 Estrutura de Diretórios

```
projeto-pim-sistema-ong-main/
├── VestindoSonhos/                    # Back-end ASP.NET Core .NET 10 (MVC + APIs REST)
│   ├── Controllers/
│   │   ├── Api/                       # APIs RESTful (Dashboard, Beneficiários, Voluntários, Inventário, Financeiro, Eventos)
│   │   └── ...                        # Controladores Web MVC (Financeiro/Dashboard, Definicoes SaaS, etc.)
│   ├── Models/                        # Entidades EF Core e DTOs
│   ├── ViewModels/                    # DashboardViewModel e modelos de visualização
│   ├── Views/                         # Telas Razor MVC com suporte completo a Dark Mode
│   └── wwwroot/                       # CSS, JS (Chart.js + tema global) e assets
├── VestindoSonhos.Tests/              # Suíte de Testes Automatizados (.NET 10 + xUnit + EF Core InMemory)
│   ├── Helpers/                       # Fábrica de DbContext isolado em memória e ClaimsPrincipal (RBAC)
│   ├── VoluntariosApiTests.cs         # Testes automatizados do módulo de Voluntários
│   ├── BeneficiariosApiTests.cs       # Testes automatizados de Beneficiários e Dependentes
│   ├── FinanceiroApiTests.cs          # Testes automatizados de Entradas, Saídas, Saldos e Estornos
│   └── DashboardAndRegrasNegocioTests.cs # Testes de Segurança RBAC (Admin vs Colaborador) e Senhas PBKDF2
├── vestindo_sonhos_mobile/            # Aplicativo Móvel em Flutter (Android, iOS & Web)
│   ├── lib/
│   │   ├── models/                    # Modelos de dados sincronizados com a API
│   │   ├── screens/                   # Telas (Dashboard Home, Login, Config SaaS Admin, Módulos CRUD)
│   │   ├── services/                  # Cliente HTTP REST + Persistência JWT e Dark Mode
│   │   └── widgets/                   # Componentes (Configurador de IP dinâmico)
│   └── pubspec.yaml                   # Dependências do Flutter
├── ScriptVestindoSonhos_MySQL.sql     # Script completo do MySQL 8.0 (Tabelas + Procedure + Trigger)
├── 1-INICIAR_SISTEMA.bat              # Inicializador rápido do servidor Web + API
├── rodar-testes-automatizados.bat     # Executor da suíte de testes automatizados (xUnit) em 1 clique
├── gerar-executavel-portatil.bat      # Gerador do pacote portátil (VestindoSonhos_Portavel)
├── importar-banco-local.bat           # Importador automático do banco MySQL
├── subir-para-github.bat              # Sincronizador automático com o GitHub em 1 clique
└── README.md                          # Documentação geral da solução
```

---

## 👥 Autoria & Licença
Projeto desenvolvido com fins acadêmicos para o Projeto Integrador Multidisciplinar (PIM) - Análise e Desenvolvimento de Sistemas (2026).
